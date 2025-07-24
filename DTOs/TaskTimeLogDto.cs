using System.ComponentModel.DataAnnotations;

namespace Flowtik.DTOs
{
    // Time Tracking Request DTOs
    // Time Tracking Request DTOs
    public class StartTaskTimerDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class PauseTaskTimerDto
    {
        [Required]
        public int UserId { get; set; }

        public string? BreakReason { get; set; }
    }

    public class ResumeTaskTimerDto
    {
        [Required]
        public int UserId { get; set; }
    }

    public class StopTaskTimerDto
    {
        [Required]
        public int UserId { get; set; }
    }

    // Time Tracking Response DTOs
    public class ActiveTimerDto
    {
        public int TimeLogId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime? StartTime { get; set; }
        public decimal ElapsedHours { get; set; }
        public string ElapsedFormatted { get; set; } = null!;
        public bool IsOnBreak { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public decimal BreakDuration { get; set; }
    }

    public class TaskTimeLogDto
    {
        public int TimeLogsId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal Duration { get; set; }
        public string DurationFormatted { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class DailyTimeSummaryDto
    {
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public string TotalHoursFormatted { get; set; } = null!;
        public decimal TotalBreakTime { get; set; }
        public string TotalBreakFormatted { get; set; } = null!;
        public decimal NetWorkingHours { get; set; }
        public string NetWorkingFormatted { get; set; } = null!;
        public bool IsEightHourCompleted { get; set; }
        public decimal RemainingHours { get; set; }
        public string RemainingFormatted { get; set; } = null!;
        public List<TaskTimeSummaryDto> TaskBreakdown { get; set; } = new();
        public List<BreakSummaryDto> BreakBreakdown { get; set; } = new();
        public ActiveTimerDto? CurrentActiveTimer { get; set; }
    }

    public class TaskTimeSummaryDto
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public string ActualHoursFormatted { get; set; } = null!;
        public decimal VarianceHours { get; set; }
        public string VarianceFormatted { get; set; } = null!;
        public bool IsOverEstimate { get; set; }
        public bool IsCompleted { get; set; }
        public int SessionCount { get; set; }
        public DateTime? FirstWorkedOn { get; set; }
        public DateTime? LastWorkedOn { get; set; }
    }

    public class BreakSummaryDto
    {
        public int BreakId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal Duration { get; set; }
        public string DurationFormatted { get; set; } = null!;
        public bool IsActive { get; set; }
        public string? Reason { get; set; }
    }

    public class WeeklyTimeSummaryDto
    {
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public string TotalHoursFormatted { get; set; } = null!;
        public decimal TotalBreakTime { get; set; }
        public decimal AverageHoursPerDay { get; set; }
        public int DaysWorked { get; set; }
        public int DaysEightHourCompleted { get; set; }
        public List<DailyTimeSummaryDto> DailySummaries { get; set; } = new();
    }

    public class TimerControlResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public ActiveTimerDto? ActiveTimer { get; set; }
        public DailyTimeSummaryDto? DailySummary { get; set; }
    }
}
