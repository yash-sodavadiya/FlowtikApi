using System.ComponentModel.DataAnnotations;

namespace Flowtik.DTOs
{
    public class CreateTaskQueryDto
    {
        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        public string? AttachmentPath { get; set; }
    }

    public class TaskQueryResponseDto
    {
        public int QueryId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = null!;
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string Subject { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? AttachmentPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = "Open";
        public int? AssignedToId { get; set; }
        public string? AssignedToUserName { get; set; }
    }

    public class QueryFilterDto
    {
        public int? TaskId { get; set; }
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? AssignedToId { get; set; }
    }

    public class QueryStatsDto
    {
        public int TotalQueries { get; set; }
        public int OpenQueries { get; set; }
        public int ResolvedQueries { get; set; }
        public int PendingQueries { get; set; }
        public List<TaskQueryResponseDto> RecentQueries { get; set; } = new();
    }
}
