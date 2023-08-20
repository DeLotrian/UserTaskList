using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTask.BLL.Exceptions;
using UserTask.BLL.Interfaces;
using UserTask.DAL.Exceptions;
using UserTask.DAL.Interfaces;
using UserTask.DAL.Models;

namespace UserTask.BLL.Services
{
    public class UserTaskListService : IUserTaskListService
    {
        private readonly IUserTaskRepository _userTaskRepository;

        public UserTaskListService(IUserTaskRepository userTaskRepository)
        {
            _userTaskRepository = userTaskRepository;
        }

        public async Task<TaskList> CreateTaskListAsync(string userId, TaskList newTaskList)
        {
            try
            {
                newTaskList.Id = String.Empty;
                newTaskList.OwnerId = userId;
                newTaskList.CreationDate = DateTime.UtcNow;

                return await _userTaskRepository.CreateTaskListAsync(newTaskList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TaskList> UpdateTaskListAsync(string userId, TaskList newTaskList)
        {
            try
            {
                if (await _userTaskRepository.CheckPermissionAsync(userId, newTaskList.Id))
                {
                    return await _userTaskRepository.UpdateTaskListAsync(newTaskList);
                }
                else
                {
                    throw new AccessDeniedException($"User {userId} doesn't have permissions to update task list {newTaskList.Id}");
                }
            }
            catch (InvalidParametersException)
            {
                throw new IncorrectDataProvidedException();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteTaskListAsync(string userId, string taskListId)
        {
            try
            {
                if (await _userTaskRepository.CheckOwnerAsync(userId, taskListId))
                {
                    await _userTaskRepository.DeleteTaskListAsync(taskListId);
                }
                else
                {
                    throw new AccessDeniedException($"User {userId} doesn't have permissions to update task list {taskListId}");
                }
            }
            catch (InvalidParametersException)
            {
                throw new IncorrectDataProvidedException();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<User>> GetUsersForTaskAsync(string userId, string taskListId)
        {
            try
            {
                if (await _userTaskRepository.CheckPermissionAsync(userId, taskListId))
                {
                    return await _userTaskRepository.GetUsersForTaskAsync(taskListId);
                }
                else
                {
                    throw new AccessDeniedException($"User {userId} doesn't have permissions to read task list {taskListId}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TaskList>> GetUserTaskListsAsync(string userId, int page, int pageSize, bool ascSort)
        {
            try
            {
                return await _userTaskRepository.GetListOfTaskListsForUserAsync(userId, page, pageSize, ascSort);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TaskList> GetTaskListByIdAsync(string userId, string taskListId)
        {
            try
            {
                if (await _userTaskRepository.CheckPermissionAsync(userId, taskListId))
                {
                    return await _userTaskRepository.GetTaskListByIdAsync(taskListId);
                }
                else
                {
                    throw new AccessDeniedException($"User {userId} doesn't have permissions to get task list {taskListId}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AttachUserToTaskListAsync(string userId, string attachedUserId, string taskListId)
        {
            try
            {
                if (await _userTaskRepository.CheckPermissionAsync(userId, taskListId))
                {
                    await _userTaskRepository.AttachUserToTaskListAsync(attachedUserId, taskListId);
                }
                else
                {
                    throw new AccessDeniedException($"User {userId} doesn't have permissions to modify task list {taskListId}");
                }
            }
            catch (InvalidParametersException)
            {
                throw new IncorrectDataProvidedException();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DetachUserToTaskListAsync(string userId, string attachedUserId, string taskListId)
        {
            try
            {
                if (await _userTaskRepository.CheckPermissionAsync(userId, taskListId))
                {
                    await _userTaskRepository.DetachUserToTaskListAsync(attachedUserId, taskListId);
                }
                else
                {
                    throw new AccessDeniedException($"User {userId} doesn't have permissions to modify task list {taskListId}");
                }
            }
            catch (InvalidParametersException)
            {
                throw new IncorrectDataProvidedException();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
