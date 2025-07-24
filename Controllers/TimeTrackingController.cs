using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flowtik.Models;
using Flowtik.DTOs;
using Flowtik.Helper;

namespace Flowtik.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeTrackingController : ControllerBase
    {
        private readonly TaskmanagerdbContext _context;

        public TimeTrackingController(TaskmanagerdbContext context)
        {
            _context = context;
        }

        // POST: api/TimeTracking/start
        [HttpPost("start")]
        public async Task<ActionResult<TimerControlResponseDto>> StartTaskTimer(StartTaskTimerDto dto)
        {
            try
            {
                // Validate user exists
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                // Validate task exists and is assigned to user
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.TaskId == dto.TaskId && t.AssignedToId == dto.UserId);

                if (task == null)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "Task not found or not assigned to this user."
                    });
                }

                if (task.IsCompleted)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "Cannot start timer for a completed task."
                    });
                }

                // Check if user has any active timer
                var activeTimer = await _context.TaskTimeLogs
                    .FirstOrDefaultAsync(tl => tl.UserId == dto.UserId && tl.EndTime == null);

                if (activeTimer != null)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "User already has an active timer. Stop the current timer first."
                    });
                }

                // Check if user is on break
                var activeBreak = await _context.BreakLogs
                    .FirstOrDefaultAsync(bl => bl.UserId == dto.UserId && bl.EndTime == default(DateTime));

                if (activeBreak != null)
                {
                    // End the break automatically
                    activeBreak.EndTime = DateTime.UtcNow;
                }

                // Start new timer
                var newTimeLog = new TaskTimeLog
                {
                    TaskId = dto.TaskId,
                    UserId = dto.UserId,
                    StartTime = DateTime.UtcNow,
                    EndTime = null
                };

                _context.TaskTimeLogs.Add(newTimeLog);
                await _context.SaveChangesAsync();

                var activeTimerDto = await GetActiveTimerForUser(dto.UserId);
                var dailySummary = await GetDailyTimeSummary(dto.UserId, DateTime.Today);

                return Ok(new TimerControlResponseDto
                {
                    Success = true,
                    Message = $"Timer started for task: {task.Title}",
                    ActiveTimer = activeTimerDto,
                    DailySummary = dailySummary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new TimerControlResponseDto
                {
                    Success = false,
                    Message = $"Error starting timer: {ex.Message}"
                });
            }
        }

        // POST: api/TimeTracking/pause
        [HttpPost("pause")]
        public async Task<ActionResult<TimerControlResponseDto>> PauseTaskTimer(PauseTaskTimerDto dto)
        {
            try
            {
                // Find active timer
                var activeTimer = await _context.TaskTimeLogs
                    .Include(tl => tl.Task)
                    .FirstOrDefaultAsync(tl => tl.UserId == dto.UserId && tl.EndTime == null);

                if (activeTimer == null)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "No active timer found to pause."
                    });
                }

                // Stop the current timer
                activeTimer.EndTime = DateTime.UtcNow;

                // Start a break
                var breakLog = new BreakLog
                {
                    TaskId = activeTimer.TaskId,
                    UserId = dto.UserId,
                    StartTime = DateTime.UtcNow,
                    EndTime = default(DateTime) // Active break
                };

                _context.BreakLogs.Add(breakLog);
                await _context.SaveChangesAsync();

                var dailySummary = await GetDailyTimeSummary(dto.UserId, DateTime.Today);

                return Ok(new TimerControlResponseDto
                {
                    Success = true,
                    Message = $"Timer paused for task: {activeTimer.Task?.Title}. Break started.",
                    DailySummary = dailySummary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new TimerControlResponseDto
                {
                    Success = false,
                    Message = $"Error pausing timer: {ex.Message}"
                });
            }
        }

        // POST: api/TimeTracking/resume
        [HttpPost("resume")]
        public async Task<ActionResult<TimerControlResponseDto>> ResumeTaskTimer(ResumeTaskTimerDto dto)
        {
            try
            {
                // Find active break
                var activeBreak = await _context.BreakLogs
                    .Include(bl => bl.Task)
                    .FirstOrDefaultAsync(bl => bl.UserId == dto.UserId && bl.EndTime == default(DateTime));

                if (activeBreak == null)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "No active break found to resume from."
                    });
                }

                // End the break
                activeBreak.EndTime = DateTime.UtcNow;

                // Resume timer for the same task
                var newTimeLog = new TaskTimeLog
                {
                    TaskId = activeBreak.TaskId,
                    UserId = dto.UserId,
                    StartTime = DateTime.UtcNow,
                    EndTime = null
                };

                _context.TaskTimeLogs.Add(newTimeLog);
                await _context.SaveChangesAsync();

                var activeTimerDto = await GetActiveTimerForUser(dto.UserId);
                var dailySummary = await GetDailyTimeSummary(dto.UserId, DateTime.Today);

                return Ok(new TimerControlResponseDto
                {
                    Success = true,
                    Message = $"Timer resumed for task: {activeBreak.Task?.Title}",
                    ActiveTimer = activeTimerDto,
                    DailySummary = dailySummary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new TimerControlResponseDto
                {
                    Success = false,
                    Message = $"Error resuming timer: {ex.Message}"
                });
            }
        }

        // POST: api/TimeTracking/stop
        [HttpPost("stop")]
        public async Task<ActionResult<TimerControlResponseDto>> StopTaskTimer(StopTaskTimerDto dto)
        {
            try
            {
                // Find active timer
                var activeTimer = await _context.TaskTimeLogs
                    .Include(tl => tl.Task)
                    .FirstOrDefaultAsync(tl => tl.UserId == dto.UserId && tl.EndTime == null);

                if (activeTimer == null)
                {
                    return BadRequest(new TimerControlResponseDto
                    {
                        Success = false,
                        Message = "No active timer found to stop."
                    });
                }

                // Stop the timer
                activeTimer.EndTime = DateTime.UtcNow;

                // Also end any active break
                var activeBreak = await _context.BreakLogs
                    .FirstOrDefaultAsync(bl => bl.UserId == dto.UserId && bl.EndTime == default(DateTime));

                if (activeBreak != null)
                {
                    activeBreak.EndTime = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var dailySummary = await GetDailyTimeSummary(dto.UserId, DateTime.Today);
                var duration = TimeHelper.GetTotalHours(activeTimer.EndTime, activeTimer.StartTime);

                return Ok(new TimerControlResponseDto
                {
                    Success = true,
                    Message = $"Timer stopped for task: {activeTimer.Task?.Title}. Duration: {TimeHelper.FormatHours(duration)}",
                    DailySummary = dailySummary
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new TimerControlResponseDto
                {
                    Success = false,
                    Message = $"Error stopping timer: {ex.Message}"
                });
            }
        }

        // GET: api/TimeTracking/active/{userId}
        [HttpGet("active/{userId}")]
        public async Task<ActionResult<ActiveTimerDto>> GetActiveTimer(int userId)
        {
            var activeTimer = await GetActiveTimerForUser(userId);

            if (activeTimer == null)
            {
                return NotFound("No active timer found for this user.");
            }

            return Ok(activeTimer);
        }

        // GET: api/TimeTracking/daily-summary/{userId}/{date}
        [HttpGet("daily-summary/{userId}/{date}")]
        public async Task<ActionResult<DailyTimeSummaryDto>> GetDailySummary(int userId, DateTime date)
        {
            var summary = await GetDailyTimeSummary(userId, date.Date);
            return Ok(summary);
        }

        // GET: api/TimeTracking/weekly-summary/{userId}/{weekStartDate}
        [HttpGet("weekly-summary/{userId}/{weekStartDate}")]
        public async Task<ActionResult<WeeklyTimeSummaryDto>> GetWeeklySummary(int userId, DateTime weekStartDate)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var startDate = weekStartDate.Date;
            var endDate = startDate.AddDays(7);

            var dailySummaries = new List<DailyTimeSummaryDto>();
            var totalHours = 0m;
            var totalBreakTime = 0m;
            var daysWorked = 0;
            var daysEightHourCompleted = 0;

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                var dailySummary = await GetDailyTimeSummary(userId, currentDate);
                dailySummaries.Add(dailySummary);

                totalHours += dailySummary.TotalHoursWorked;
                totalBreakTime += dailySummary.TotalBreakTime;

                if (dailySummary.TotalHoursWorked > 0)
                    daysWorked++;

                if (dailySummary.IsEightHourCompleted)
                    daysEightHourCompleted++;
            }

            var weeklySummary = new WeeklyTimeSummaryDto
            {
                WeekStartDate = startDate,
                WeekEndDate = endDate.AddDays(-1),
                UserId = userId,
                UserName = user.UserName,
                TotalHoursWorked = totalHours,
                TotalHoursFormatted = TimeHelper.FormatHours(totalHours),
                TotalBreakTime = totalBreakTime,
                AverageHoursPerDay = daysWorked > 0 ? totalHours / daysWorked : 0,
                DaysWorked = daysWorked,
                DaysEightHourCompleted = daysEightHourCompleted,
                DailySummaries = dailySummaries
            };

            return Ok(weeklySummary);
        }

        // GET: api/TimeTracking/task-breakdown/{userId}/{date}
        [HttpGet("task-breakdown/{userId}/{date}")]
        public async Task<ActionResult<List<TaskTimeSummaryDto>>> GetTaskBreakdown(int userId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var timeLogs = await _context.TaskTimeLogs
                .Where(tl => tl.UserId == userId &&
                           tl.StartTime >= startOfDay &&
                           tl.StartTime < endOfDay)
                .Include(tl => tl.Task)
                .ToListAsync();

            var taskBreakdown = timeLogs
                .GroupBy(tl => tl.TaskId)
                .Select(g =>
                {
                    var task = g.First().Task;
                    var actualHours = g.Sum(tl => TimeHelper.GetTotalHours(tl.EndTime, tl.StartTime));
                    var estimatedHours = task?.EstimatedHours ?? 0;
                    var variance = actualHours - estimatedHours;

                    return new TaskTimeSummaryDto
                    {
                        TaskId = g.Key,
                        TaskTitle = task?.Title ?? "Unknown",
                        EstimatedHours = estimatedHours,
                        ActualHours = actualHours,
                        ActualHoursFormatted = TimeHelper.FormatHours(actualHours),
                        VarianceHours = variance,
                        VarianceFormatted = TimeHelper.FormatHours(Math.Abs(variance)),
                        IsOverEstimate = variance > 0,
                        IsCompleted = task?.IsCompleted ?? false,
                        SessionCount = g.Count(),
                        FirstWorkedOn = g.Min(tl => tl.StartTime),
                        LastWorkedOn = g.Max(tl => tl.StartTime)
                    };
                })
                .OrderByDescending(t => t.ActualHours)
                .ToList();

            return Ok(taskBreakdown);
        }

        #region Private Helper Methods

        private async Task<ActiveTimerDto?> GetActiveTimerForUser(int userId)
        {
            var activeTimeLog = await _context.TaskTimeLogs
                .Include(tl => tl.Task)
                .Include(tl => tl.User)
                .FirstOrDefaultAsync(tl => tl.UserId == userId && tl.EndTime == null);

            if (activeTimeLog == null)
                return null;

            // Check if user is on break
            var activeBreak = await _context.BreakLogs
                .FirstOrDefaultAsync(bl => bl.UserId == userId && bl.EndTime == default(DateTime));

            var elapsedHours = TimeHelper.GetTotalHours(null, activeTimeLog.StartTime);
            var breakDuration = activeBreak != null ?
                TimeHelper.GetTotalHours(null, activeBreak.StartTime) : 0;

            return new ActiveTimerDto
            {
                TimeLogId = activeTimeLog.TimeLogsId,
                TaskId = activeTimeLog.TaskId,
                TaskTitle = activeTimeLog.Task?.Title ?? "Unknown",
                UserId = activeTimeLog.UserId,
                UserName = activeTimeLog.User?.UserName,
                StartTime = activeTimeLog.StartTime ?? DateTime.UtcNow,
                ElapsedHours = elapsedHours,
                ElapsedFormatted = TimeHelper.FormatHours(elapsedHours),
                IsOnBreak = activeBreak != null,
                BreakStartTime = activeBreak?.StartTime,
                BreakDuration = breakDuration
            };
        }

        private async Task<DailyTimeSummaryDto> GetDailyTimeSummary(int userId, DateTime date)
        {
            var user = await _context.Users.FindAsync(userId);
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            // Get time logs for the day
            var timeLogs = await _context.TaskTimeLogs
                .Where(tl => tl.UserId == userId &&
                           tl.StartTime >= startOfDay &&
                           tl.StartTime < endOfDay)
                .Include(tl => tl.Task)
                .ToListAsync();

            // Get break logs for the day
            var breakLogs = await _context.BreakLogs
                .Where(bl => bl.UserId == userId &&
                           bl.StartTime >= startOfDay &&
                           bl.StartTime < endOfDay)
                .Include(bl => bl.Task)
                .ToListAsync();

            // Calculate totals
            var totalHoursWorked = timeLogs.Sum(tl => TimeHelper.GetTotalHours(tl.EndTime, tl.StartTime));
            var totalBreakTime = breakLogs.Sum(bl =>
                bl.EndTime != default(DateTime) ?
                TimeHelper.GetTotalHours(bl.EndTime, bl.StartTime) :
                TimeHelper.GetTotalHours(null, bl.StartTime));

            var netWorkingHours = totalHoursWorked; // Break time is separate from working time
            var isEightHourCompleted = TimeHelper.IsEightHourCompleted(netWorkingHours);
            var remainingHours = TimeHelper.GetRemainingHours(netWorkingHours);

            // Task breakdown
            var taskBreakdown = timeLogs
                .GroupBy(tl => tl.TaskId)
                .Select(g =>
                {
                    var task = g.First().Task;
                    var actualHours = g.Sum(tl => TimeHelper.GetTotalHours(tl.EndTime, tl.StartTime));
                    var estimatedHours = task?.EstimatedHours ?? 0;
                    var variance = actualHours - estimatedHours;

                    return new TaskTimeSummaryDto
                    {
                        TaskId = g.Key,
                        TaskTitle = task?.Title ?? "Unknown",
                        EstimatedHours = estimatedHours,
                        ActualHours = actualHours,
                        ActualHoursFormatted = TimeHelper.FormatHours(actualHours),
                        VarianceHours = variance,
                        VarianceFormatted = TimeHelper.FormatHours(Math.Abs(variance)),
                        IsOverEstimate = variance > 0,
                        IsCompleted = task?.IsCompleted ?? false,
                        SessionCount = g.Count(),
                        FirstWorkedOn = g.Min(tl => tl.StartTime),
                        LastWorkedOn = g.Max(tl => tl.StartTime)
                    };
                })
                .OrderByDescending(t => t.ActualHours)
                .ToList();

            // Break breakdown
            var breakBreakdown = breakLogs
                .Select(bl => new BreakSummaryDto
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
                })
                .ToList();

            // Get current active timer
            var activeTimer = await GetActiveTimerForUser(userId);

            return new DailyTimeSummaryDto
            {
                Date = date,
                UserId = userId,
                UserName = user?.UserName,
                TotalHoursWorked = totalHoursWorked,
                TotalHoursFormatted = TimeHelper.FormatHours(totalHoursWorked),
                TotalBreakTime = totalBreakTime,
                TotalBreakFormatted = TimeHelper.FormatHours(totalBreakTime),
                NetWorkingHours = netWorkingHours,
                NetWorkingFormatted = TimeHelper.FormatHours(netWorkingHours),
                IsEightHourCompleted = isEightHourCompleted,
                RemainingHours = remainingHours,
                RemainingFormatted = TimeHelper.FormatHours(remainingHours),
                TaskBreakdown = taskBreakdown,
                BreakBreakdown = breakBreakdown,
                CurrentActiveTimer = activeTimer
            };
        }

        #endregion
    }
}
