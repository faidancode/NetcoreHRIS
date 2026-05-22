using System.ComponentModel.DataAnnotations;

namespace NetcoreHRIS.Modules.LeaveRequests.Dtos;

public record ListLeaveRequestQuery(
    string? Q = null,
    string? Search = null,
    Guid? EmployeeId = null,
    Guid? LeaveId = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "createdAt:desc"
);

public record CreateLeaveRequestRequest(
    [Required(AllowEmptyStrings = false)]
    Guid EmployeeId,

    [Required(AllowEmptyStrings = false)]
    Guid LeaveId,

    [Required]
    DateOnly FromDate,

    [Required]
    DateOnly ToDate,

    [Required(AllowEmptyStrings = false)]
    [StringLength(1000)]
    string Reason,

    [StringLength(500)]
    string? AttachmentPath = null
);

public record UpdateLeaveRequestRequest(
    Guid? EmployeeId,
    Guid? LeaveId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    [StringLength(1000)]
    string? Reason,
    [StringLength(500)]
    string? AttachmentPath
);

public record LeaveRequestDto(
    Guid Id,
    string RequestNo,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveId,
    string LeaveName,
    DateOnly FromDate,
    DateOnly ToDate,
    string Reason,
    string? AttachmentPath,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
