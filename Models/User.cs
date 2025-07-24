using System;
using System.Collections.Generic;

namespace Flowtik.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BreakLog> BreakLogs { get; set; } = new List<BreakLog>();

    public virtual Role? Role { get; set; } = null!;

    public virtual Task? TaskAssignedTo { get; set; }

    public virtual ICollection<Task> TaskCreatedBies { get; set; } = new List<Task>();

    public virtual ICollection<TaskQuery> TaskQueries { get; set; } = new List<TaskQuery>();

    public virtual ICollection<TaskTimeLog> TaskTimeLogs { get; set; } = new List<TaskTimeLog>();
}
