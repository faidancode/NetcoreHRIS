namespace NetcoreHRIS.Entities;

public enum AttendanceStatus
{
    OnTime,
    Late
}

public class Attendance : BaseEntity
{
    public DateOnly Date { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public TimeOnly CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.OnTime;
}
