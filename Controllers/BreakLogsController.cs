using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flowtik.Models;
using Flowtik.DTOs;
using Flowtik.Helper;

namespace Flowtik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreakLogsController : ControllerBase
    {
        private readonly TaskmanagerdbContext _context;

        public BreakLogsController(TaskmanagerdbContext context)
        {
            _context = context;
        }

        // GET: api/BreakLogs/user/{userId}/date/{date}
        [HttpGet("user/{userId}/date/{date}")]
        public async Task<ActionResult<List<BreakSummaryDto>>> GetUserBreaksForDate(int userId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var breakLogs = await _context.BreakLogs
                .Where(bl => bl.UserId == userId &&
                           bl.StartTime >= startOfDay &&
                           bl.StartTime < endOfDay)
                .Include(bl => bl.Task)
                .OrderBy(bl => bl.StartTime)
                .ToListAsync();

            var breakSummaries = breakLogs.Select(bl => new BreakSummaryDto
            {
                BreakId = bl.BreakId,
                TaskId = bl.TaskId,
                TaskTitle = bl.Task?.Title ?? "Unknown",
                StartTime = bl.StartTime,
                EndTime = bl.EndTime != default(DateTime) ? bl.EndTime : null,
                Duration = bl.EndTime != default(DateTime) ?
                          TimeHelper.GetTotalHours(bl.EndTime, bl.StartTime) :
                          TimeHelper.GetTotalHours(null, bl.StartTime),
                DurationFormatted = TimeHelper.FormatHours(
                    bl.EndTime != default(DateTime) ?
                    TimeHelper.GetTotalHours(bl.EndTime, bl.StartTime) :
                    TimeHelper.GetTotalHours(null, bl.StartTime)),
                IsActive = bl.EndTime == default(DateTime)
            }).ToList();

            return Ok(breakSummaries);
        }

        // GET: api/BreakLogs/active/{userId}
        [HttpGet("active/{userId}")]
        public async Task<ActionResult<BreakSummaryDto>> GetActiveBreak(int userId)
        {
            var activeBreak = await _context.BreakLogs
                .Include(bl => bl.Task)
                .FirstOrDefaultAsync(bl => bl.UserId == userId && bl.EndTime == default(DateTime));

            if (activeBreak == null)
            {
                return NotFound("No active break found for this user.");
            }

            var breakSummary = new BreakSummaryDto
            {
                BreakId = activeBreak.BreakId,
                TaskId = activeBreak.TaskId,
                TaskTitle = activeBreak.Task?.Title ?? "Unknown",
                StartTime = activeBreak.StartTime,
                EndTime = null,
                Duration = TimeHelper.GetTotalHours(null, activeBreak.StartTime),
                DurationFormatted = TimeHelper.FormatHours(TimeHelper.GetTotalHours(null, activeBreak.StartTime)),
                IsActive = true
            };

            return Ok(breakSummary);
        }

        // POST: api/BreakLogs/end/{breakId}
        [HttpPost("end/{breakId}")]
        public async Task<IActionResult> EndBreak(int breakId)
        {
            var breakLog = await _context.BreakLogs.FindAsync(breakId);
            if (breakLog == null)
            {
                return NotFound("Break not found.");
            }

            if (breakLog.EndTime != default(DateTime))
            {
                return BadRequest("Break is already ended.");
            }

            breakLog.EndTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var duration = TimeHelper.GetTotalHours(breakLog.EndTime, breakLog.StartTime);
            return Ok(new { Message = "Break ended successfully", Duration = TimeHelper.FormatHours(duration) });
        }
    }
}
