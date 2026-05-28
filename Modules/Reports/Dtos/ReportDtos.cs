using NetcoreHRIS.Entities;

namespace NetcoreHRIS.Modules.Reports.Dtos;

public enum ReportExportFormat
{
    Xlsx,
    Pdf
}

public record ReportGroupCountDto(
    string Key,
    string Label,
    int Total
);

public record ReportGroupNumberDto(
    string Key,
    string Label,
    int Total
);

public record EmployeeReportQuery(
    string? Q = null,
    string? Search = null,
    Guid? DepartmentId = null,
    Guid? PositionId = null,
    Guid? EmployeeId = null,
    EmployeeStatus? EmployeeStatus = null,
    EmploymentType? EmploymentType = null,
    bool? IsActive = null,
    Gender? Gender = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "createdAt:desc"
);

public record EmployeeReportSummaryDto(
    int TotalEmployees,
    int TotalActiveEmployees,
    int TotalInactiveEmployees,
    int TotalPermanentEmployees,
    int TotalContractEmployees,
    int TotalMaleEmployees,
    int TotalFemaleEmployees,
    IReadOnlyCollection<ReportGroupCountDto> TotalByDepartment,
    IReadOnlyCollection<ReportGroupCountDto> TotalByPosition,
    IReadOnlyCollection<ReportGroupCountDto> TotalByEmployeeStatus
);

public record EmployeeReportItemDto(
    Guid Id,
    string FullName,
    string Nip,
    string Gender,
    string EmploymentType,
    string EmployeeStatus,
    bool IsActive,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid PositionId,
    string PositionName,
    DateOnly DateOfJoining,
    DateTime CreatedAt
);

public record EmployeeReportDto(
    EmployeeReportSummaryDto Summary,
    IReadOnlyCollection<EmployeeReportItemDto> Items
);

public record AttendanceReportQuery(
    string? Q = null,
    string? Search = null,
    Guid? DepartmentId = null,
    Guid? PositionId = null,
    Guid? EmployeeId = null,
    EmployeeStatus? EmployeeStatus = null,
    EmploymentType? EmploymentType = null,
    bool? IsActive = null,
    Gender? Gender = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    DateOnly? Date = null,
    AttendanceStatus? AttendanceStatus = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "date:desc"
);

public record AttendanceReportSummaryDto(
    int TotalAttendanceRecords,
    int TotalOnTime,
    int TotalLate,
    int TotalEmployeesWithAttendance,
    int TotalMissingCheckOut,
    IReadOnlyCollection<ReportGroupCountDto> AttendanceByDepartment,
    IReadOnlyCollection<ReportGroupCountDto> AttendanceByDate,
    IReadOnlyCollection<ReportGroupCountDto> AttendanceByStatus
);

public record AttendanceReportItemDto(
    Guid Id,
    DateOnly Date,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeNip,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid PositionId,
    string PositionName,
    TimeOnly CheckIn,
    TimeOnly? CheckOut,
    string Status
);

public record AttendanceReportDto(
    AttendanceReportSummaryDto Summary,
    IReadOnlyCollection<AttendanceReportItemDto> Items
);

public record LeavesReportQuery(
    string? Q = null,
    string? Search = null,
    Guid? DepartmentId = null,
    Guid? PositionId = null,
    Guid? EmployeeId = null,
    EmployeeStatus? EmployeeStatus = null,
    EmploymentType? EmploymentType = null,
    bool? IsActive = null,
    Gender? Gender = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? LeaveId = null,
    string? RequestNo = null,
    int Page = 1,
    int Limit = 10,
    string Sort = "createdAt:desc"
);

public record LeavesReportSummaryDto(
    int TotalLeaveRequests,
    int TotalLeaveDays,
    int TotalEmployeesTakingLeave,
    IReadOnlyCollection<ReportGroupCountDto> TotalByLeaveType,
    IReadOnlyCollection<ReportGroupCountDto> TotalByDepartment,
    IReadOnlyCollection<ReportGroupNumberDto> LeaveDaysByEmployee,
    IReadOnlyCollection<ReportGroupCountDto> LeaveRequestsByMonth
);

public record LeavesReportItemDto(
    Guid Id,
    string RequestNo,
    Guid EmployeeId,
    string EmployeeName,
    string EmployeeNip,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid LeaveId,
    string LeaveName,
    DateOnly FromDate,
    DateOnly ToDate,
    int TotalDays,
    string Reason,
    DateTime CreatedAt
);

public record LeavesReportDto(
    LeavesReportSummaryDto Summary,
    IReadOnlyCollection<LeavesReportItemDto> Items
);

public record ReportExportResult(
    byte[] Content,
    string ContentType,
    string FileName
);

public record ReportPagedResult<T>(
    T Data,
    int Total,
    int Page,
    int Limit
);
