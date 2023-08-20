using Microsoft.AspNetCore.Mvc;
using System.Net;
using UserTask.BLL.Exceptions;
using UserTask.BLL.Interfaces;
using UserTask.DAL.Models;
using ZstdSharp.Unsafe;

namespace UserTask.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserTaskListController : Controller
    {
        private readonly IUserTaskListService _userTaskListService;

        public UserTaskListController(IUserTaskListService userTaskListService)
        {
            _userTaskListService = userTaskListService;
        }

        [HttpGet("user/taskLists")]
        public async Task<IActionResult> GetTaskListsAsync(string userId, int page, int pageSize, bool ascSort)
        {
            try
            {
                return Ok(await _userTaskListService.GetUserTaskListsAsync(userId, page, pageSize, ascSort));
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("taskList/users")]
        public async Task<IActionResult> GetTaskListsAsync(string userId, string taskId)
        {
            try
            {
                return Ok(await _userTaskListService.GetUsersForTaskAsync(userId, taskId));
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("taskList")]
        public async Task<IActionResult> GetTaskListByIdAsync(string userId, string taskId)
        {
            try
            {
                return Ok(await _userTaskListService.GetTaskListByIdAsync(userId, taskId));
            }
            catch (AccessDeniedException ex)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }






        [HttpPost("taskList")]
        public async Task<IActionResult> CreateTaskListsAsync([FromQuery]string userId, [FromBody] TaskList taskList)
        {
            try
            {
                return Ok(await _userTaskListService.CreateTaskListAsync(userId, taskList));
            }
            catch
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut("taskList")]
        public async Task<IActionResult> UpdateTaskListsAsync([FromQuery] string userId, [FromBody] TaskList taskList)
        {
            try
            {
                return Ok(await _userTaskListService.UpdateTaskListAsync(userId, taskList));
            }
            catch (AccessDeniedException ex)
            {
                return Unauthorized();
            }
            catch (IncorrectDataProvidedException ex)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut("taskList/attach")]
        public async Task<IActionResult> AttachUserToTaskList(string userId, string attachedUserId, string taskListId)
        {
            try
            {
                await _userTaskListService.AttachUserToTaskListAsync(userId, attachedUserId, taskListId);
                return Ok();
            }
            catch (AccessDeniedException ex)
            {
                return Unauthorized();
            }
            catch (IncorrectDataProvidedException ex)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut("taskList/detach")]
        public async Task<IActionResult> DetachUserToTaskList(string userId, string attachedUserId, string taskListId)
        {
            try
            {
                await _userTaskListService.DetachUserToTaskListAsync(userId, attachedUserId, taskListId);
                return Ok();
            }
            catch (AccessDeniedException ex)
            {
                return Unauthorized();
            }
            catch (IncorrectDataProvidedException ex)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete("taskList")]
        public async Task<IActionResult> DeleteTaskListsAsync(string userId, string taskListId)
        {
            try
            {
                await _userTaskListService.DeleteTaskListAsync(userId, taskListId);
                return Ok();
            }
            catch (AccessDeniedException ex)
            {
                return Unauthorized();
            }
            catch (IncorrectDataProvidedException ex)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
