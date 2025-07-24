using System;
using System.Collections.Generic;

namespace Flowtik.Models;

public partial class Task
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal EstimatedHours { get; set; }

    public int AssignedToId { get; set; }

    public int CreatedById { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User AssignedTo { get; set; } = null!;

    public virtual ICollection<BreakLog> BreakLogs { get; set; } = new List<BreakLog>();

    public virtual User CreatedBy { get; set; } = null!;

    public virtual ICollection<TaskQuery> TaskQueries { get; set; } = new List<TaskQuery>();

    public virtual ICollection<TaskTimeLog> TaskTimeLogs { get; set; } = new List<TaskTimeLog>();
}
