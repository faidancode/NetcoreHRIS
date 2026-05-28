using Microsoft.EntityFrameworkCore;
using NetcoreHRIS.Common.Exceptions;
using NetcoreHRIS.Common.Extensions;
using NetcoreHRIS.Data;
using NetcoreHRIS.Entities;
using NetcoreHRIS.Modules.Reports.Dtos;

namespace NetcoreHRIS.Modules.Reports;

public interface IReportsService
{
    Task<ReportPagedResult<EmployeeReportDto>> GetEmployeeReportAsync(EmployeeReportQuery query, CancellationToken ct);
    Task<ReportPagedResult<AttendanceReportDto>> GetAttendanceReportAsync(AttendanceReportQuery query, CancellationToken ct);
    Task<ReportPagedResult<LeavesReportDto>> GetLeavesReportAsync(LeavesReportQuery query, CancellationToken ct);
    Task<ReportExportResult> ExportEmployeeReportAsync(EmployeeReportQuery query, ReportExportFormat format, CancellationToken ct);
    Task<ReportExportResult> ExportAttendanceReportAsync(AttendanceReportQuery query, ReportExportFormat format, CancellationToken ct);
    Task<ReportExportResult> ExportLeavesReportAsync(LeavesReportQuery query, ReportExportFormat format, CancellationToken ct);
}

public class ReportsService : IReportsService
{
    private const int ExportLimit = 10_000;
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db) => _db = db;

    public async Task<ReportPagedResult<EmployeeReportDto>> GetEmployeeReportAsync(EmployeeReportQuery query, CancellationToken ct)
    {
        var dbQuery = ApplyEmployeeFilters(BaseEmployeeQuery(), query);
        var total = await dbQuery.CountAsync(ct);
        var allItems = (await dbQuery.ToListAsync(ct)).Select(MapEmployeeItem).ToList();
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var pageItems = (await ApplyEmployeeSorting(dbQuery, query.Sort)
            .ApplyPagination(page, limit)
            .ToListAsync(ct))
            .Select(MapEmployeeItem)
            .ToList();

        return new ReportPagedResult<EmployeeReportDto>(
            new EmployeeReportDto(BuildEmployeeSummary(allItems), pageItems),
            total,
            page,
            limit);
    }

    public async Task<ReportPagedResult<AttendanceReportDto>> GetAttendanceReportAsync(AttendanceReportQuery query, CancellationToken ct)
    {
        var dbQuery = ApplyAttendanceFilters(BaseAttendanceQuery(), query);
        var total = await dbQuery.CountAsync(ct);
        var allItems = (await dbQuery.ToListAsync(ct)).Select(MapAttendanceItem).ToList();
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var pageItems = (await ApplyAttendanceSorting(dbQuery, query.Sort)
            .ApplyPagination(page, limit)
            .ToListAsync(ct))
            .Select(MapAttendanceItem)
            .ToList();

        return new ReportPagedResult<AttendanceReportDto>(
            new AttendanceReportDto(BuildAttendanceSummary(allItems), pageItems),
            total,
            page,
            limit);
    }

    public async Task<ReportPagedResult<LeavesReportDto>> GetLeavesReportAsync(LeavesReportQuery query, CancellationToken ct)
    {
        var dbQuery = ApplyLeavesFilters(BaseLeavesQuery(), query);
        var total = await dbQuery.CountAsync(ct);
        var allItems = (await dbQuery.ToListAsync(ct)).Select(MapLeavesItem).ToList();
        var page = NormalizePage(query.Page);
        var limit = NormalizeLimit(query.Limit);
        var pageItems = (await ApplyLeavesSorting(dbQuery, query.Sort)
            .ApplyPagination(page, limit)
            .ToListAsync(ct))
            .Select(MapLeavesItem)
            .ToList();

        return new ReportPagedResult<LeavesReportDto>(
            new LeavesReportDto(BuildLeavesSummary(allItems), pageItems),
            total,
            page,
            limit);
    }

    public async Task<ReportExportResult> ExportEmployeeReportAsync(EmployeeReportQuery query, ReportExportFormat format, CancellationToken ct)
    {
        var items = (await ApplyEmployeeSorting(ApplyEmployeeFilters(BaseEmployeeQuery(), query), query.Sort)
            .Take(ExportLimit)
            .ToListAsync(ct))
            .Select(MapEmployeeItem)
            .ToList();
        var summary = BuildEmployeeSummary(items);
        return ReportsExportBuilder.BuildEmployeeReport(format, summary, items, BuildFileName("employee-report", format));
    }

    public async Task<ReportExportResult> ExportAttendanceReportAsync(AttendanceReportQuery query, ReportExportFormat format, CancellationToken ct)
    {
        var items = (await ApplyAttendanceSorting(ApplyAttendanceFilters(BaseAttendanceQuery(), query), query.Sort)
            .Take(ExportLimit)
            .ToListAsync(ct))
            .Select(MapAttendanceItem)
            .ToList();
        var summary = BuildAttendanceSummary(items);
        return ReportsExportBuilder.BuildAttendanceReport(format, summary, items, BuildFileName("attendance-report", format));
    }

    public async Task<ReportExportResult> ExportLeavesReportAsync(LeavesReportQuery query, ReportExportFormat format, CancellationToken ct)
    {
        var items = (await ApplyLeavesSorting(ApplyLeavesFilters(BaseLeavesQuery(), query), query.Sort)
            .Take(ExportLimit)
            .ToListAsync(ct))
            .Select(MapLeavesItem)
            .ToList();
        var summary = BuildLeavesSummary(items);
        return ReportsExportBuilder.BuildLeavesReport(format, summary, items, BuildFileName("leaves-report", format));
    }

    private IQueryable<Employee> BaseEmployeeQuery()
        => _db.Employees
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Position);

    private IQueryable<Attendance> BaseAttendanceQuery()
        => _db.Attendances
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
            .Include(x => x.Employee)
                .ThenInclude(x => x.Position);

    private IQueryable<LeaveRequest> BaseLeavesQuery()
        => _db.LeaveRequests
            .AsNoTracking()
            .Include(x => x.Employee)
                .ThenInclude(x => x.Department)
            .Include(x => x.Employee)
                .ThenInclude(x => x.Position)
            .Include(x => x.LeaveMaster);

    private static IQueryable<Employee> ApplyEmployeeFilters(IQueryable<Employee> query, EmployeeReportQuery filters)
    {
        var term = (filters.Search ?? filters.Q)?.Trim().ToLower();

        if (filters.EmployeeId.HasValue)
            query = query.Where(x => x.Id == filters.EmployeeId.Value);
        if (filters.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == filters.DepartmentId.Value);
        if (filters.PositionId.HasValue)
            query = query.Where(x => x.PositionId == filters.PositionId.Value);
        if (filters.EmployeeStatus.HasValue)
            query = query.Where(x => x.EmployeeStatus == filters.EmployeeStatus.Value);
        if (filters.EmploymentType.HasValue)
            query = query.Where(x => x.EmploymentType == filters.EmploymentType.Value);
        if (filters.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filters.IsActive.Value);
        if (filters.Gender.HasValue)
            query = query.Where(x => x.Gender == filters.Gender.Value);
        if (filters.FromDate.HasValue)
            query = query.Where(x => x.DateOfJoining >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(x => x.DateOfJoining <= filters.ToDate.Value);
        if (!string.IsNullOrEmpty(term))
            query = query.Where(x =>
                x.FullName.ToLower().Contains(term) ||
                x.Nip.ToLower().Contains(term) ||
                (x.Department != null && x.Department.Name.ToLower().Contains(term)) ||
                x.Position.Name.ToLower().Contains(term));

        return query;
    }

    private static IQueryable<Attendance> ApplyAttendanceFilters(IQueryable<Attendance> query, AttendanceReportQuery filters)
    {
        var term = (filters.Search ?? filters.Q)?.Trim().ToLower();

        if (filters.EmployeeId.HasValue)
            query = query.Where(x => x.EmployeeId == filters.EmployeeId.Value);
        if (filters.DepartmentId.HasValue)
            query = query.Where(x => x.Employee.DepartmentId == filters.DepartmentId.Value);
        if (filters.PositionId.HasValue)
            query = query.Where(x => x.Employee.PositionId == filters.PositionId.Value);
        if (filters.EmployeeStatus.HasValue)
            query = query.Where(x => x.Employee.EmployeeStatus == filters.EmployeeStatus.Value);
        if (filters.EmploymentType.HasValue)
            query = query.Where(x => x.Employee.EmploymentType == filters.EmploymentType.Value);
        if (filters.IsActive.HasValue)
            query = query.Where(x => x.Employee.IsActive == filters.IsActive.Value);
        if (filters.Gender.HasValue)
            query = query.Where(x => x.Employee.Gender == filters.Gender.Value);
        if (filters.Date.HasValue)
            query = query.Where(x => x.Date == filters.Date.Value);
        if (filters.FromDate.HasValue)
            query = query.Where(x => x.Date >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(x => x.Date <= filters.ToDate.Value);
        if (filters.AttendanceStatus.HasValue)
            query = query.Where(x => x.Status == filters.AttendanceStatus.Value);
        if (!string.IsNullOrEmpty(term))
            query = query.Where(x =>
                x.Employee.FullName.ToLower().Contains(term) ||
                x.Employee.Nip.ToLower().Contains(term) ||
                (x.Employee.Department != null && x.Employee.Department.Name.ToLower().Contains(term)) ||
                x.Employee.Position.Name.ToLower().Contains(term));

        return query;
    }

    private static IQueryable<LeaveRequest> ApplyLeavesFilters(IQueryable<LeaveRequest> query, LeavesReportQuery filters)
    {
        var term = (filters.Search ?? filters.Q)?.Trim().ToLower();

        if (filters.EmployeeId.HasValue)
            query = query.Where(x => x.EmployeeId == filters.EmployeeId.Value);
        if (filters.DepartmentId.HasValue)
            query = query.Where(x => x.Employee.DepartmentId == filters.DepartmentId.Value);
        if (filters.PositionId.HasValue)
            query = query.Where(x => x.Employee.PositionId == filters.PositionId.Value);
        if (filters.EmployeeStatus.HasValue)
            query = query.Where(x => x.Employee.EmployeeStatus == filters.EmployeeStatus.Value);
        if (filters.EmploymentType.HasValue)
            query = query.Where(x => x.Employee.EmploymentType == filters.EmploymentType.Value);
        if (filters.IsActive.HasValue)
            query = query.Where(x => x.Employee.IsActive == filters.IsActive.Value);
        if (filters.Gender.HasValue)
            query = query.Where(x => x.Employee.Gender == filters.Gender.Value);
        if (filters.FromDate.HasValue)
            query = query.Where(x => x.FromDate >= filters.FromDate.Value);
        if (filters.ToDate.HasValue)
            query = query.Where(x => x.ToDate <= filters.ToDate.Value);
        if (filters.LeaveId.HasValue)
            query = query.Where(x => x.LeaveMasterId == filters.LeaveId.Value);
        if (!string.IsNullOrWhiteSpace(filters.RequestNo))
        {
            var requestNo = filters.RequestNo.Trim().ToLower();
            query = query.Where(x => x.RequestNo.ToLower().Contains(requestNo));
        }
        if (!string.IsNullOrEmpty(term))
            query = query.Where(x =>
                x.RequestNo.ToLower().Contains(term) ||
                x.Employee.FullName.ToLower().Contains(term) ||
                x.Employee.Nip.ToLower().Contains(term) ||
                (x.Employee.Department != null && x.Employee.Department.Name.ToLower().Contains(term)) ||
                x.Employee.Position.Name.ToLower().Contains(term) ||
                x.LeaveMaster.Name.ToLower().Contains(term) ||
                x.LeaveMaster.Code.ToLower().Contains(term) ||
                x.Reason.ToLower().Contains(term));

        return query;
    }

    private static IQueryable<Employee> ApplyEmployeeSorting(IQueryable<Employee> query, string? sort)
        => sort switch
        {
            "fullName:asc" => query.OrderBy(x => x.FullName),
            "fullName:desc" => query.OrderByDescending(x => x.FullName),
            "nip:asc" => query.OrderBy(x => x.Nip),
            "nip:desc" => query.OrderByDescending(x => x.Nip),
            "dateOfJoining:asc" => query.OrderBy(x => x.DateOfJoining),
            "dateOfJoining:desc" => query.OrderByDescending(x => x.DateOfJoining),
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

    private static IQueryable<Attendance> ApplyAttendanceSorting(IQueryable<Attendance> query, string? sort)
        => sort switch
        {
            "date:asc" => query.OrderBy(x => x.Date),
            "date:desc" => query.OrderByDescending(x => x.Date),
            "checkIn:asc" => query.OrderBy(x => x.CheckIn),
            "checkIn:desc" => query.OrderByDescending(x => x.CheckIn),
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.Date)
        };

    private static IQueryable<LeaveRequest> ApplyLeavesSorting(IQueryable<LeaveRequest> query, string? sort)
        => sort switch
        {
            "requestNo:asc" => query.OrderBy(x => x.RequestNo),
            "requestNo:desc" => query.OrderByDescending(x => x.RequestNo),
            "fromDate:asc" => query.OrderBy(x => x.FromDate),
            "fromDate:desc" => query.OrderByDescending(x => x.FromDate),
            "toDate:asc" => query.OrderBy(x => x.ToDate),
            "toDate:desc" => query.OrderByDescending(x => x.ToDate),
            "createdAt:asc" => query.OrderBy(x => x.CreatedAt),
            "createdAt:desc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

    private static EmployeeReportItemDto MapEmployeeItem(Employee x)
        => new(
            x.Id,
            x.FullName,
            x.Nip,
            x.Gender.ToString(),
            x.EmploymentType.ToString(),
            x.EmployeeStatus.ToString(),
            x.IsActive,
            x.DepartmentId,
            x.Department != null ? x.Department.Name : null,
            x.PositionId,
            x.Position.Name,
            x.DateOfJoining,
            x.CreatedAt);

    private static AttendanceReportItemDto MapAttendanceItem(Attendance x)
        => new(
            x.Id,
            x.Date,
            x.EmployeeId,
            x.Employee.FullName,
            x.Employee.Nip,
            x.Employee.DepartmentId,
            x.Employee.Department != null ? x.Employee.Department.Name : null,
            x.Employee.PositionId,
            x.Employee.Position.Name,
            x.CheckIn,
            x.CheckOut,
            x.Status.ToString());

    private static LeavesReportItemDto MapLeavesItem(LeaveRequest x)
        => new(
            x.Id,
            x.RequestNo,
            x.EmployeeId,
            x.Employee.FullName,
            x.Employee.Nip,
            x.Employee.DepartmentId,
            x.Employee.Department != null ? x.Employee.Department.Name : null,
            x.LeaveMasterId,
            x.LeaveMaster.Name,
            x.FromDate,
            x.ToDate,
            x.ToDate.DayNumber - x.FromDate.DayNumber + 1,
            x.Reason,
            x.CreatedAt);

    private static EmployeeReportSummaryDto BuildEmployeeSummary(IReadOnlyCollection<EmployeeReportItemDto> items)
        => new(
            items.Count,
            items.Count(x => x.IsActive),
            items.Count(x => !x.IsActive),
            items.Count(x => x.EmploymentType == EmploymentType.Permanent.ToString()),
            items.Count(x => x.EmploymentType == EmploymentType.Contract.ToString()),
            items.Count(x => x.Gender == Gender.Male.ToString()),
            items.Count(x => x.Gender == Gender.Female.ToString()),
            GroupCount(items, x => x.DepartmentId?.ToString() ?? "unassigned", x => x.DepartmentName ?? "Unassigned"),
            GroupCount(items, x => x.PositionId.ToString(), x => x.PositionName),
            GroupCount(items, x => x.EmployeeStatus, x => x.EmployeeStatus));

    private static AttendanceReportSummaryDto BuildAttendanceSummary(IReadOnlyCollection<AttendanceReportItemDto> items)
        => new(
            items.Count,
            items.Count(x => x.Status == AttendanceStatus.OnTime.ToString()),
            items.Count(x => x.Status == AttendanceStatus.Late.ToString()),
            items.Select(x => x.EmployeeId).Distinct().Count(),
            items.Count(x => x.CheckOut == null),
            GroupCount(items, x => x.DepartmentId?.ToString() ?? "unassigned", x => x.DepartmentName ?? "Unassigned"),
            GroupCount(items, x => x.Date.ToString("yyyy-MM-dd"), x => x.Date.ToString("yyyy-MM-dd")),
            GroupCount(items, x => x.Status, x => x.Status));

    private static LeavesReportSummaryDto BuildLeavesSummary(IReadOnlyCollection<LeavesReportItemDto> items)
        => new(
            items.Count,
            items.Sum(x => x.TotalDays),
            items.Select(x => x.EmployeeId).Distinct().Count(),
            GroupCount(items, x => x.LeaveId.ToString(), x => x.LeaveName),
            GroupCount(items, x => x.DepartmentId?.ToString() ?? "unassigned", x => x.DepartmentName ?? "Unassigned"),
            GroupNumber(items, x => x.EmployeeId.ToString(), x => x.EmployeeName, x => x.TotalDays),
            GroupCount(items, x => $"{x.FromDate.Year:D4}-{x.FromDate.Month:D2}", x => $"{x.FromDate.Year:D4}-{x.FromDate.Month:D2}"));

    private static IReadOnlyCollection<ReportGroupCountDto> GroupCount<T>(
        IEnumerable<T> items,
        Func<T, string> keySelector,
        Func<T, string> labelSelector)
        => items
            .GroupBy(keySelector)
            .Select(x => new ReportGroupCountDto(x.Key, labelSelector(x.First()), x.Count()))
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Label)
            .ToList();

    private static IReadOnlyCollection<ReportGroupNumberDto> GroupNumber<T>(
        IEnumerable<T> items,
        Func<T, string> keySelector,
        Func<T, string> labelSelector,
        Func<T, int> valueSelector)
        => items
            .GroupBy(keySelector)
            .Select(x => new ReportGroupNumberDto(x.Key, labelSelector(x.First()), x.Sum(valueSelector)))
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Label)
            .ToList();

    private static int NormalizePage(int page) => page < 1 ? 1 : page;

    private static int NormalizeLimit(int limit) => limit < 1 ? 10 : Math.Min(limit, 100);

    private static string BuildFileName(string prefix, ReportExportFormat format)
        => $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToString().ToLowerInvariant()}";

    internal static void EnsureSupportedFormat(ReportExportFormat format)
    {
        if (!Enum.IsDefined(format))
            throw new AppException("Format must be either xlsx or pdf.", 400, "INVALID_REPORT_FORMAT");
    }
}
