using System.ComponentModel.DataAnnotations;

namespace Flowtik.DTOs
{
    // Request DTOs
    public class CreateTaskDto
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0.01, 999.99)]
        public decimal EstimatedHours { get; set; }

        [Required]
        public int AssignedToId { get; set; }

        [Required]
        public int CreatedById { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required]
        [StringLength(50)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0.01, 999.99)]
        public decimal EstimatedHours { get; set; }

        public int AssignedToId { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class StartTaskDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class StopTaskDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    public class PauseTaskDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string? Reason { get; set; }
    }

    public class ResumeTaskDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }
    }

    // Response DTOs
    public class TaskResponseDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal EstimatedHours { get; set; }
        public int AssignedToId { get; set; }
        public string? AssignedToUserName { get; set; }
        public string? AssignedToEmail { get; set; }
        public int CreatedById { get; set; }
        public string? CreatedByUserName { get; set; }
        public string? CreatedByEmail { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CreatedAt { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public DateTime? LastStartTime { get; set; }
        public int QueriesCount { get; set; }
    }

    public class TaskSummaryDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = null!;
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public bool IsCompleted { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? LastWorkedOn { get; set; }
    }

    public class DailyAttendanceDto
    {
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal TotalBreakTime { get; set; }
        public bool IsEightHourCompleted { get; set; }
        public decimal RemainingHours { get; set; }
        public List<TaskSummaryDto> Tasks { get; set; } = new();
        public List<BreakLogDto> Breaks { get; set; } = new();
        public TaskResponseDto? CurrentActiveTask { get; set; }
    }

    public class BreakLogDto
    {
        public int BreakId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal Duration { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserTaskStatsDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int TotalTasksAssigned { get; set; }
        public int CompletedTasks { get; set; }
        public int ActiveTasks { get; set; }
        public decimal TotalHoursWorked { get; set; }
        public decimal AverageHoursPerTask { get; set; }
        public DateTime? LastActivity { get; set; }
    }
}
