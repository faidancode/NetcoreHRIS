using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.Reports.Dtos;
using NetcoreHRIS.Security;

namespace NetcoreHRIS.Modules.Reports;

[ApiController]
[Route("api/v{version:apiVersion}/reports")]
[Authorize]
[EnableRateLimiting("per-user")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _service;

    public ReportsController(IReportsService service) => _service = service;

    [HttpGet("employees")]
    [HasPermission("read", "Report")]
    public async Task<ActionResult<Response<EmployeeReportDto>>> GetEmployeeReport(
        [FromQuery] EmployeeReportQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetEmployeeReportAsync(query, ct);
        return Ok(Response<EmployeeReportDto>.Ok(
            result.Data,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("employees/export")]
    [HasPermission("read", "Report")]
    public async Task<IActionResult> ExportEmployeeReport(
        [FromQuery] EmployeeReportQuery query,
        [FromQuery] ReportExportFormat format,
        CancellationToken ct)
    {
        var result = await _service.ExportEmployeeReportAsync(query, format, ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("attendances")]
    [HasPermission("read", "Report")]
    public async Task<ActionResult<Response<AttendanceReportDto>>> GetAttendanceReport(
        [FromQuery] AttendanceReportQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetAttendanceReportAsync(query, ct);
        return Ok(Response<AttendanceReportDto>.Ok(
            result.Data,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("attendances/export")]
    [HasPermission("read", "Report")]
    public async Task<IActionResult> ExportAttendanceReport(
        [FromQuery] AttendanceReportQuery query,
        [FromQuery] ReportExportFormat format,
        CancellationToken ct)
    {
        var result = await _service.ExportAttendanceReportAsync(query, format, ct);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpGet("leaves")]
    [HasPermission("read", "Report")]
    public async Task<ActionResult<Response<LeavesReportDto>>> GetLeavesReport(
        [FromQuery] LeavesReportQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetLeavesReportAsync(query, ct);
        return Ok(Response<LeavesReportDto>.Ok(
            result.Data,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("leaves/export")]
    [HasPermission("read", "Report")]
    public async Task<IActionResult> ExportLeavesReport(
        [FromQuery] LeavesReportQuery query,
        [FromQuery] ReportExportFormat format,
        CancellationToken ct)
    {
        var result = await _service.ExportLeavesReportAsync(query, format, ct);
        return File(result.Content, result.ContentType, result.FileName);
    }
}
