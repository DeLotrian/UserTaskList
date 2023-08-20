using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UserTask.DAL.Models;

namespace UserTask.DAL.Interfaces
{
    public interface IUserTaskRepository
    {
        public Task<List<TaskList>> GetListOfTaskListsForUserAsync(string userId, int page, int pageSize, bool ascSort);
        public Task<List<User>> GetUsersForTaskAsync(string taskListId);
        public Task<TaskList> GetTaskListByIdAsync(string taskListId);
        public Task<TaskList> CreateTaskListAsync(TaskList newTaskList);
        public Task<TaskList> UpdateTaskListAsync(TaskList newTaskList);
        public Task AttachUserToTaskListAsync(string userId, string taskListId);
        public Task DetachUserToTaskListAsync(string userId, string taskListId);
        public Task DeleteTaskListAsync(string taskListId);
        public Task<bool> CheckPermissionAsync(string userId, string taskListId);
        public Task<bool> CheckOwnerAsync(string userId, string taskListId);
    }
}
