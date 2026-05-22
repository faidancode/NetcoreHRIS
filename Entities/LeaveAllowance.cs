namespace NetcoreHRIS.Entities;

public class LeaveAllowance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid LeaveMasterId { get; set; }
    public LeaveMaster LeaveMaster { get; set; } = null!;

    public int Year { get; set; }
    public int QuotaDays { get; set; }
    public string? Notes { get; set; }
}
