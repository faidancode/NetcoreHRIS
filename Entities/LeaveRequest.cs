namespace NetcoreHRIS.Entities;

public class LeaveRequest : BaseEntity
{
    public string RequestNo { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid LeaveMasterId { get; set; }
    public LeaveMaster LeaveMaster { get; set; } = null!;

    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
}
