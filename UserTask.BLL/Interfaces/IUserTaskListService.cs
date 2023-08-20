using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserTask.DAL.Models;

namespace UserTask.BLL.Interfaces
{
    public interface IUserTaskListService
    {
        public Task<List<TaskList>> GetUserTaskListsAsync(string userId, int page, int pageSize, bool ascSort);
        public Task<List<User>> GetUsersForTaskAsync(string userId, string taskListId);
        public Task<TaskList> GetTaskListByIdAsync(string userId, string taskListId);
        public Task<TaskList> CreateTaskListAsync(string userId, TaskList newTaskList);
        public Task AttachUserToTaskListAsync(string userId, string attachedUserId, string taskListId);
        public Task DetachUserToTaskListAsync(string userId, string attachedUserId, string taskListId);
        public Task<TaskList> UpdateTaskListAsync(string userId, TaskList newTaskList);
        public Task DeleteTaskListAsync(string userId, string taskListId);
    }
}
