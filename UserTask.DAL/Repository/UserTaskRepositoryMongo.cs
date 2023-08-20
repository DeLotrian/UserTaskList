using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Diagnostics;
using System.Threading.Tasks;
using UserTask.DAL.Exceptions;
using UserTask.DAL.Interfaces;
using UserTask.DAL.Models;

namespace UserTask.DAL.Repository
{
    public class UserTaskRepositoryMongo : IUserTaskRepository
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<TaskList> _taskLists;
        private readonly MongoClient _client;

        public UserTaskRepositoryMongo(IOptions<UserTaskDbConfigMongo> userTaskDbConfig)
        {
            _client = new MongoClient(userTaskDbConfig.Value.Connection_String);
            var database = _client.GetDatabase(userTaskDbConfig.Value.Database_Name);
            _users = database.GetCollection<User>(userTaskDbConfig.Value.Users_Collection_Name);
            _taskLists = database.GetCollection<TaskList>(userTaskDbConfig.Value.Task_Lists_Collection_Name);
        }

        public async Task<TaskList> CreateTaskListAsync(TaskList newTaskList)
        {
            try
            {
                await _taskLists.InsertOneAsync(newTaskList);
                newTaskList.AttachedUsers ??= new List<string>();
                if (newTaskList.AttachedUsers.Count > 0)
                {
                    foreach (var userId in newTaskList.AttachedUsers)
                    {
                        User selectedUser = await (await _users.FindAsync(u => u.Id == userId)).FirstOrDefaultAsync();
                        selectedUser.AttachedTaskLists.Add(newTaskList.Id);
                        await _users.ReplaceOneAsync(u => u.Id == userId, selectedUser);
                    }
                }
                return newTaskList;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }



        public async Task<TaskList> UpdateTaskListAsync(TaskList newTaskList)
        {
            try
            {
                using (var session = await _client.StartSessionAsync())
                {
                    try
                    {
                        TaskList oldTaskList = await _taskLists.Find(t => t.Id == newTaskList.Id).FirstOrDefaultAsync();

                        if (oldTaskList == null) throw new InvalidParametersException();

                        List<string> oldAttchedUsers = oldTaskList.AttachedUsers ?? new List<string>();
                        List<string> newAttchedUsers = newTaskList.AttachedUsers ?? new List<string>();
                        IEnumerable<string> removedUsers = oldAttchedUsers.Except(newAttchedUsers);
                        IEnumerable<string> addedUsers = newAttchedUsers.Except(oldAttchedUsers);

                        session.StartTransaction();
                        foreach (string userId in removedUsers)
                        {
                            User selectedUser = await (await _users.FindAsync(u => u.Id == userId)).FirstOrDefaultAsync();
                            selectedUser.AttachedTaskLists.Remove(newTaskList.Id);
                            await _users.ReplaceOneAsync(u => u.Id == userId, selectedUser);
                        }

                        foreach (string userId in addedUsers)
                        {
                            User selectedUser = await (await _users.FindAsync(u => u.Id == userId)).FirstOrDefaultAsync();
                            selectedUser.AttachedTaskLists.Add(newTaskList.Id);
                            await _users.ReplaceOneAsync(u => u.Id == userId, selectedUser);
                        }

                        newTaskList.OwnerId = oldTaskList.OwnerId;
                        newTaskList.CreationDate = oldTaskList.CreationDate;
                        newTaskList.Tasks ??= oldTaskList.Tasks;
                        newTaskList.AttachedUsers ??= oldTaskList.AttachedUsers;
                        await _taskLists.ReplaceOneAsync(t => t.Id == newTaskList.Id, newTaskList);
                        session.CommitTransaction();
                    }
                    catch (Exception)
                    {
                        session.AbortTransaction();
                        throw;
                    }
                }
                return newTaskList;
            }
            catch (InvalidParametersException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task DeleteTaskListAsync(string taskListId)
        {
            try
            {
                using (var session = await _client.StartSessionAsync())
                {
                    try
                    {
                        TaskList taskListToDelete = await (await _taskLists.FindAsync(t => t.Id == taskListId)).FirstOrDefaultAsync();
                        List<string> usersToUpdate = taskListToDelete.AttachedUsers ?? new List<string>();

                        if (taskListToDelete == null) throw new InvalidParametersException();

                        session.StartTransaction();
                        foreach (string userId in usersToUpdate)
                        {
                            User selectedUser = await (await _users.FindAsync(u => u.Id == userId)).FirstOrDefaultAsync();
                            selectedUser.AttachedTaskLists.Remove(taskListId);
                            await _users.ReplaceOneAsync(u => u.Id == userId, selectedUser);
                        }

                        await _taskLists.DeleteOneAsync(t => t.Id == taskListId);

                        session.CommitTransaction();
                    }
                    catch (Exception)
                    {
                        session.AbortTransaction();
                        throw;
                    }
                }
            }
            catch (InvalidParametersException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task<List<TaskList>> GetListOfTaskListsForUserAsync(string userId, int page, int pageSize, bool ascSort)
        {
            try
            {
                List<TaskList> taskLists = new List<TaskList>();
                taskLists = await _taskLists.Find(task => task.OwnerId == userId || (task.AttachedUsers != null && task.AttachedUsers.Contains(userId)))
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                if (ascSort)
                {
                    taskLists = taskLists.OrderBy(t => t.CreationDate).ToList();
                }
                else
                {
                    taskLists = taskLists.OrderByDescending(t => t.CreationDate).ToList();
                }

                return taskLists.Select(t => new TaskList()
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList();
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task<List<User>> GetUsersForTaskAsync(string taskListId)
        {
            try
            {
                List<User> users = new List<User>();
                users = await (await _users.FindAsync(u => u.AttachedTaskLists.Contains(taskListId))).ToListAsync();

                return users.Select(u => new User()
                {
                    Id = u.Id,
                    Username = u.Username
                }).ToList();
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task<TaskList> GetTaskListByIdAsync(string taskListId)
        {
            try
            {
                return await (await _taskLists.FindAsync(t => t.Id == taskListId)).FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task AttachUserToTaskListAsync(string userId, string taskListId)
        {
            try
            {
                using (var session = await _client.StartSessionAsync())
                {
                    try
                    {
                        TaskList taskList = await (await _taskLists.FindAsync(t => t.Id == taskListId)).FirstOrDefaultAsync();
                        User user = await (await _users.FindAsync(u => u.Id == userId)).FirstOrDefaultAsync();

                        if (taskList == null || user == null) throw new InvalidParametersException();

                        session.StartTransaction();
                        if (!user.AttachedTaskLists.Contains(taskListId))
                        {
                            user.AttachedTaskLists.Add(taskListId);
                        }

                        if (taskList.AttachedUsers != null && !taskList.AttachedUsers.Contains(userId))
                        {
                            taskList.AttachedUsers.Add(userId);
                        }

                        await _taskLists.ReplaceOneAsync(t => t.Id == taskListId, taskList);
                        await _users.ReplaceOneAsync(u => u.Id == userId, user);

                        session.CommitTransaction();
                    }
                    catch (Exception)
                    {
                        session.AbortTransaction();
                        throw;
                    }
                }
            }
            catch (InvalidParametersException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task DetachUserToTaskListAsync(string userId, string taskListId)
        {
            try
            {
                using (var session = await _client.StartSessionAsync())
                {
                    try
                    {
                        TaskList taskList = await (await _taskLists.FindAsync(t => t.Id == taskListId)).FirstOrDefaultAsync();
                        User user = await (await _users.FindAsync(u => u.Id == userId)).FirstOrDefaultAsync();

                        if (taskList == null || user == null) throw new InvalidParametersException();

                        session.StartTransaction();
                        if (user.AttachedTaskLists != null)
                        {
                            user.AttachedTaskLists.Remove(taskListId);
                        }

                        if (taskList.AttachedUsers != null)
                        {
                            taskList.AttachedUsers.Remove(userId);
                        }

                        await _taskLists.ReplaceOneAsync(t => t.Id == taskListId, taskList);
                        await _users.ReplaceOneAsync(u => u.Id == userId, user);

                        session.CommitTransaction();
                    }
                    catch (Exception)
                    {
                        session.AbortTransaction();
                        throw;
                    }
                }
            }
            catch (InvalidParametersException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task<bool> CheckPermissionAsync(string userId, string taskListId)
        {
            try
            {
                TaskList taskList = await (await _taskLists.FindAsync(t => t.Id == taskListId)).FirstOrDefaultAsync();
                if (taskList == null) throw new InvalidParametersException();
                return taskList.OwnerId == userId ||
                    (taskList.AttachedUsers != null && taskList.AttachedUsers.Contains(userId));
            }
            catch (InvalidParametersException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }

        public async Task<bool> CheckOwnerAsync(string userId, string taskListId)
        {
            try
            {
                TaskList taskList = await (await _taskLists.FindAsync(t => t.Id == taskListId)).FirstOrDefaultAsync();
                if (taskList == null) throw new InvalidParametersException();
                return taskList.OwnerId == userId;
            }
            catch (InvalidParametersException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new DbException("Error during database operations");
            }
        }
    }
}
