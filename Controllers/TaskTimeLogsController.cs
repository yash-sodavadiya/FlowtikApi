using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flowtik.Models;
using Flowtik.DTOs;

namespace Flowtik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskTimeLogsController : ControllerBase
    {
        private readonly TaskmanagerdbContext _context;

        public TaskTimeLogsController(TaskmanagerdbContext context)
        {
            _context = context;
        }

        // GET: api/TaskTimeLogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskTimeLogDto>>> GetTaskTimeLogs()
        {
            var logs = await _context.TaskTimeLogs
                .Include(t => t.Task)
                .Include(t => t.User)
                .Select(log => new TaskTimeLogDto
                {
                    TimeLogsId = log.TimeLogsId,
                    TaskId = log.TaskId,
                    TaskTitle = log.Task != null ? log.Task.Title : "",
                    UserId = log.UserId,
                    UserName = log.User != null ? log.User.UserName : "",
                    StartTime = log.StartTime,
                    EndTime = log.EndTime
                })
                .ToListAsync();

            return Ok(logs);
        }

        // GET: api/TaskTimeLogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskTimeLogDto>> GetTaskTimeLog(int id)
        {
            var log = await _context.TaskTimeLogs
                .Include(t => t.Task)
                .Include(t => t.User)
                .Where(l => l.TimeLogsId == id)
                .Select(log => new TaskTimeLogDto
                {
                    TimeLogsId = log.TimeLogsId,
                    TaskId = log.TaskId,
                    TaskTitle = log.Task != null ? log.Task.Title : "",
                    UserId = log.UserId,
                    UserName = log.User != null ? log.User.UserName : "",
                    StartTime = log.StartTime,
                    EndTime = log.EndTime
                })
                .FirstOrDefaultAsync();

            if (log == null)
                return NotFound();

            return Ok(log);
        }


        // PUT: api/TaskTimeLogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskTimeLog(int id, TaskTimeLog taskTimeLog)
        {
            if (id != taskTimeLog.TimeLogsId)
            {
                return BadRequest();
            }

            _context.Entry(taskTimeLog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskTimeLogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TaskTimeLogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TaskTimeLog>> PostTaskTimeLog(TaskTimeLog taskTimeLog)
        {
            _context.TaskTimeLogs.Add(taskTimeLog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTaskTimeLog", new { id = taskTimeLog.TimeLogsId }, taskTimeLog);
        }

        // DELETE: api/TaskTimeLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskTimeLog(int id)
        {
            var taskTimeLog = await _context.TaskTimeLogs.FindAsync(id);
            if (taskTimeLog == null)
            {
                return NotFound();
            }

            _context.TaskTimeLogs.Remove(taskTimeLog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskTimeLogExists(int id)
        {
            return _context.TaskTimeLogs.Any(e => e.TimeLogsId == id);
        }

        public class TaskTimeLogDto
        {
            public int TimeLogsId { get; set; }
            public int TaskId { get; set; }
            public string TaskTitle { get; set; }
            public int UserId { get; set; }
            public string UserName { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public decimal Duration { get; set; }
            public string DurationFormatted { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
