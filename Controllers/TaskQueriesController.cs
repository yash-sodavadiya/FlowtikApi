using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flowtik.Models;
using Flowtik.DTOs;

namespace Flowtik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskQueriesController : ControllerBase
    {
        private readonly TaskmanagerdbContext _context;

        public TaskQueriesController(TaskmanagerdbContext context)
        {
            _context = context;
        }

        // GET: api/TaskQueries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskQueryResponseDto>>> GetTaskQueries([FromQuery] QueryFilterDto filter)
        {
            var query = _context.TaskQueries
                .Include(tq => tq.Task)
                .ThenInclude(t => t.AssignedTo)
                .Include(tq => tq.User)
                .AsQueryable();

            if (filter.TaskId.HasValue)
                query = query.Where(tq => tq.TaskId == filter.TaskId.Value);

            if (filter.UserId.HasValue)
                query = query.Where(tq => tq.UserId == filter.UserId.Value);

            if (filter.AssignedToId.HasValue)
                query = query.Where(tq => tq.Task.AssignedToId == filter.AssignedToId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(tq => tq.Task.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(tq => tq.Task.CreatedAt <= filter.ToDate.Value);

            var taskQueries = await query
                .Select(tq => new TaskQueryResponseDto
                {
                    QueryId = tq.QueryId,
                    TaskId = tq.TaskId,
                    TaskTitle = tq.Task != null ? tq.Task.Title : "Unknown",
                    UserId = tq.UserId,
                    UserName = tq.User != null ? tq.User.UserName : "Unknown",
                    UserEmail = tq.User != null ? tq.User.Email : null,
                    Subject = tq.Subject,
                    Description = tq.Description,
                    AttachmentPath = tq.AttachmentPath,
                    Status = "Open", // You can add a Status field to your TaskQuery model
                    AssignedToId = tq.Task != null ? tq.Task.AssignedToId : null,
                    AssignedToUserName = tq.Task != null && tq.Task.AssignedTo != null ?
                                        tq.Task.AssignedTo.UserName : null
                })
                .OrderByDescending(tq => tq.QueryId)
                .ToListAsync();

            return Ok(taskQueries);
        }

        // GET: api/TaskQueries/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskQueryResponseDto>> GetTaskQuery(int id)
        {
            var taskQuery = await _context.TaskQueries
                .Include(tq => tq.Task)
                .ThenInclude(t => t.AssignedTo)
                .Include(tq => tq.User)
                .Where(tq => tq.QueryId == id)
                .Select(tq => new TaskQueryResponseDto
                {
                    QueryId = tq.QueryId,
                    TaskId = tq.TaskId,
                    TaskTitle = tq.Task != null ? tq.Task.Title : "Unknown",
                    UserId = tq.UserId,
                    UserName = tq.User != null ? tq.User.UserName : "Unknown",
                    UserEmail = tq.User != null ? tq.User.Email : null,
                    Subject = tq.Subject,
                    Description = tq.Description,
                    AttachmentPath = tq.AttachmentPath,
                    Status = "Open",
                    AssignedToId = tq.Task != null ? tq.Task.AssignedToId : null,
                    AssignedToUserName = tq.Task != null && tq.Task.AssignedTo != null ?
                                        tq.Task.AssignedTo.UserName : null
                })
                .FirstOrDefaultAsync();

            if (taskQuery == null)
            {
                return NotFound();
            }

            return Ok(taskQuery);
        }

        // POST: api/TaskQueries
        [HttpPost]
        public async Task<ActionResult<TaskQueryResponseDto>> CreateTaskQuery(CreateTaskQueryDto createTaskQueryDto)
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(createTaskQueryDto.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            // Validate task exists and is assigned to the user
            var task = await _context.Tasks
                .Where(t => t.TaskId == createTaskQueryDto.TaskId && t.AssignedToId == createTaskQueryDto.UserId)
                .FirstOrDefaultAsync();

            if (task == null)
            {
                return BadRequest("Task not found or not assigned to this user.");
            }

            var taskQuery = new TaskQuery
            {
                TaskId = createTaskQueryDto.TaskId,
                UserId = createTaskQueryDto.UserId,
                Subject = createTaskQueryDto.Subject,
                Description = createTaskQueryDto.Description,
                AttachmentPath = createTaskQueryDto.AttachmentPath ?? string.Empty
            };

            _context.TaskQueries.Add(taskQuery);
            await _context.SaveChangesAsync();

            var queryResponse = await GetTaskQuery(taskQuery.QueryId);
            return CreatedAtAction(nameof(GetTaskQuery), new { id = taskQuery.QueryId }, queryResponse.Value);
        }

        // GET: api/TaskQueries/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskQueryResponseDto>>> GetQueriesByUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var taskQueries = await _context.TaskQueries
                .Where(tq => tq.UserId == userId)
                .Include(tq => tq.Task)
                .ThenInclude(t => t.AssignedTo)
                .Include(tq => tq.User)
                .Select(tq => new TaskQueryResponseDto
                {
                    QueryId = tq.QueryId,
                    TaskId = tq.TaskId,
                    TaskTitle = tq.Task != null ? tq.Task.Title : "Unknown",
                    UserId = tq.UserId,
                    UserName = tq.User != null ? tq.User.UserName : "Unknown",
                    UserEmail = tq.User != null ? tq.User.Email : null,
                    Subject = tq.Subject,
                    Description = tq.Description,
                    AttachmentPath = tq.AttachmentPath,
                    Status = "Open",
                    AssignedToId = tq.Task != null ? tq.Task.AssignedToId : null,
                    AssignedToUserName = tq.Task != null && tq.Task.AssignedTo != null ?
                                        tq.Task.AssignedTo.UserName : null
                })
                .OrderByDescending(tq => tq.QueryId)
                .ToListAsync();

            return Ok(taskQueries);
        }

        // GET: api/TaskQueries/task/{taskId}
        [HttpGet("task/{taskId}")]
        public async Task<ActionResult<IEnumerable<TaskQueryResponseDto>>> GetQueriesByTask(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound("Task not found.");
            }

            var taskQueries = await _context.TaskQueries
                .Where(tq => tq.TaskId == taskId)
                .Include(tq => tq.Task)
                .ThenInclude(t => t.AssignedTo)
                .Include(tq => tq.User)
                .Select(tq => new TaskQueryResponseDto
                {
                    QueryId = tq.QueryId,
                    TaskId = tq.TaskId,
                    TaskTitle = tq.Task != null ? tq.Task.Title : "Unknown",
                    UserId = tq.UserId,
                    UserName = tq.User != null ? tq.User.UserName : "Unknown",
                    UserEmail = tq.User != null ? tq.User.Email : null,
                    Subject = tq.Subject,
                    Description = tq.Description,
                    AttachmentPath = tq.AttachmentPath,
                    Status = "Open",
                    AssignedToId = tq.Task != null ? tq.Task.AssignedToId : null,
                    AssignedToUserName = tq.Task != null && tq.Task.AssignedTo != null ?
                                        tq.Task.AssignedTo.UserName : null
                })
                .OrderByDescending(tq => tq.QueryId)
                .ToListAsync();

            return Ok(taskQueries);
        }

        // DELETE: api/TaskQueries/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskQuery(int id)
        {
            var taskQuery = await _context.TaskQueries.FindAsync(id);
            if (taskQuery == null)
            {
                return NotFound();
            }

            _context.TaskQueries.Remove(taskQuery);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
