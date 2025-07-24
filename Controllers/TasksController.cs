using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flowtik.Models;
using Flowtik.DTOs;
using TaskEntity = Flowtik.Models.Task;
using Flowtik.Helper;

namespace Flowtik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskmanagerdbContext _context;

        public TasksController(TaskmanagerdbContext context)
        {
            _context = context;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.TaskQueries)
                .Include(t => t.TaskTimeLogs)
                .ToListAsync();

            var taskDtos = tasks.Select(task => new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                AssignedToId = task.AssignedToId,
                AssignedToUserName = task.AssignedTo?.UserName,
                AssignedToEmail = task.AssignedTo?.Email,
                CreatedById = task.CreatedById,
                CreatedByUserName = task.CreatedBy?.UserName,
                CreatedByEmail = task.CreatedBy?.Email,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                TotalHoursWorked = task.TaskTimeLogs.Sum(log => TimeHelper.GetTotalHours(log.EndTime, log.StartTime)),
                IsCurrentlyActive = task.TaskTimeLogs.Any(log => log.EndTime == null),
                LastStartTime = task.TaskTimeLogs.FirstOrDefault(log => log.EndTime == null)?.StartTime,
                QueriesCount = task.TaskQueries.Count
            }).ToList();

            return taskDtos;
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Include(t => t.TaskQueries)
                .Include(t => t.TaskTimeLogs)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
                return NotFound();

            var dto = new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                AssignedToId = task.AssignedToId,
                AssignedToUserName = task.AssignedTo?.UserName,
                AssignedToEmail = task.AssignedTo?.Email,
                CreatedById = task.CreatedById,
                CreatedByUserName = task.CreatedBy?.UserName,
                CreatedByEmail = task.CreatedBy?.Email,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                TotalHoursWorked = task.TaskTimeLogs.Sum(log => TimeHelper.GetTotalHours(log.EndTime, log.StartTime)),
                IsCurrentlyActive = task.TaskTimeLogs.Any(log => log.EndTime == null),
                LastStartTime = task.TaskTimeLogs.FirstOrDefault(log => log.EndTime == null)?.StartTime,
                QueriesCount = task.TaskQueries.Count
            };
            return dto;
        }

        // GET: api/Tasks/assigned/3
        [HttpGet("assigned/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAssignedTasks(int userId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.AssignedToId == userId)
                .Include(t => t.TaskTimeLogs)
                .Include(t => t.TaskQueries)
                .ToListAsync();

            var taskDtos = tasks.Select(task => new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                EstimatedHours = task.EstimatedHours,
                AssignedToId = task.AssignedToId,
                AssignedToUserName = task.AssignedTo?.UserName,
                CreatedAt = task.CreatedAt,
                IsCompleted = task.IsCompleted,
                TotalHoursWorked = task.TaskTimeLogs.Sum(log => TimeHelper.GetTotalHours(log.EndTime, log.StartTime)),
                IsCurrentlyActive = task.TaskTimeLogs.Any(log => log.EndTime == null),
                LastStartTime = task.TaskTimeLogs.FirstOrDefault(log => log.EndTime == null)?.StartTime,
                QueriesCount = task.TaskQueries.Count
            }).ToList();

            return taskDtos;
        }

        // POST: api/Tasks/start/5
        [HttpPost("start/{taskId}")]
        public async Task<IActionResult> StartTask(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound("Task not found");

            var userId = task.AssignedToId;

            // Stop other running tasks for the user
            var runningLogs = await _context.TaskTimeLogs
                .Where(tl => tl.UserId == userId && tl.EndTime == null)
                .ToListAsync();

            foreach (var log in runningLogs)
            {
                log.EndTime = DateTime.UtcNow;
            }

            // Start new time log for this task
            var newLog = new TaskTimeLog
            {
                TaskId = taskId,
                UserId = userId,
                StartTime = DateTime.UtcNow
            };
            _context.TaskTimeLogs.Add(newLog);
            await _context.SaveChangesAsync();

            return Ok("Task started.");
        }

        // POST: api/Tasks/stop/3
        [HttpPost("stop/{userId}")]
        public async Task<IActionResult> StopTask(int userId)
        {
            var runningLog = await _context.TaskTimeLogs
                .Where(tl => tl.UserId == userId && tl.EndTime == null)
                .FirstOrDefaultAsync();

            if (runningLog == null)
                return BadRequest("No active task found to stop.");

            runningLog.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Task stopped.");
        }

        // POST: api/Tasks/manual
        [HttpPost("manual")]
        public async Task<IActionResult> AddManualTask([FromBody] CreateTaskDto dto)
        {
            var task = new TaskEntity
            {
                Title = dto.Title,
                Description = dto.Description,
                EstimatedHours = dto.EstimatedHours,
                AssignedToId = dto.AssignedToId,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Manual task created successfully", TaskId = task.TaskId });
        }
    }
}
