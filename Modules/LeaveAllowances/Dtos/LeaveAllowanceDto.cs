using System.ComponentModel.DataAnnotations;

namespace NetcoreHRIS.Modules.LeaveAllowances.Dtos;

public record ListLeaveAllowanceQuery(
    string? Q = null,
    string? Search = null,
    Guid? EmployeeId = null,
    Guid? LeaveId = null,
    int? Year = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "createdAt:desc"
);

public record CreateLeaveAllowanceRequest(
    [Required(AllowEmptyStrings = false)]
    Guid EmployeeId,

    [Required(AllowEmptyStrings = false)]
    Guid LeaveId,

    [Range(2000, 2100)]
    int Year,

    [Range(1, 365)]
    int QuotaDays,

    [StringLength(500)]
    string? Notes = null
);

public record UpdateLeaveAllowanceRequest(
    Guid? EmployeeId,
    Guid? LeaveId,
    int? Year,
    int? QuotaDays,
    [StringLength(500)]
    string? Notes
);

public record LeaveAllowanceDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveId,
    string LeaveName,
    int Year,
    int QuotaDays,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
