using System.ComponentModel.DataAnnotations;

namespace NetcoreHRIS.Modules.Attendances.Dtos;

public record ListAttendanceQuery(
    string? Q = null,
    string? Search = null,
    Guid? EmployeeId = null,
    DateOnly? Date = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "date:desc"
);

public record CreateAttendanceRequest(
    [Required(AllowEmptyStrings = false)]
    DateOnly Date,

    [Required(AllowEmptyStrings = false)]
    Guid EmployeeId,

    [Required]
    TimeOnly? CheckIn,

    [Required]
    TimeOnly? CheckOut
);

public record UpdateAttendanceRequest(
    DateOnly? Date,
    Guid? EmployeeId,
    TimeOnly? CheckIn,
    TimeOnly? CheckOut
);

public record AttendanceDto(
    Guid Id,
    DateOnly Date,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeNip,
    TimeOnly CheckIn,
    TimeOnly? CheckOut,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
