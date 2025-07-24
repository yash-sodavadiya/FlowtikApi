using System;
using System.Collections.Generic;

namespace Flowtik.Models;

public partial class TaskQuery
{
    public int Id { get; set; }

    public int TaskId { get; set; }

    public int EmployeeId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? AttachmentPath { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Employee { get; set; } = null!;

    public virtual Task Task { get; set; } = null!;
}
