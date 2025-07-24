using System;
using System.Collections.Generic;

namespace Flowtik.Models;

public partial class TaskTimeLog
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual User Employee { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
