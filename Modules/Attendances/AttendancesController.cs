using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.Attendances.Dtos;
using NetcoreHRIS.Security;

namespace NetcoreHRIS.Modules.Attendances;

[ApiController]
[Route("api/v{version:apiVersion}/attendances")]
[Authorize]
[EnableRateLimiting("per-user")]
[Produces("application/json")]
public class AttendancesController : ControllerBase
{
    private readonly IAttendancesService _service;

    public AttendancesController(IAttendancesService service) => _service = service;

    [HttpPost]
    [HasPermission("create", "Attendance")]
    public async Task<ActionResult<Response<AttendanceDto>>> Create(
        [FromBody] CreateAttendanceRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            Response<AttendanceDto>.Ok(result, "Attendance created successfully."));
    }

    [HttpGet]
    [HasPermission("read", "Attendance")]
    public async Task<ActionResult<Response<IEnumerable<AttendanceDto>>>> GetAll(
        [FromQuery] ListAttendanceQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(Response<IEnumerable<AttendanceDto>>.Ok(
            result.Items,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("read", "Attendance")]
    public async Task<ActionResult<Response<AttendanceDto>>> GetById(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.GetByIdAsync(id, ct);
        return Ok(Response<AttendanceDto>.Ok(result));
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("update", "Attendance")]
    public async Task<ActionResult<Response<AttendanceDto>>> Update(
        Guid id,
        [FromBody] UpdateAttendanceRequest request,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(Response<AttendanceDto>.Ok(result, "Attendance updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("delete", "Attendance")]
    public async Task<ActionResult<Response<object?>>> Delete(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        await _service.DeleteAsync(id, ct);
        return Ok(Response<object?>.Ok(null, "Attendance deleted successfully."));
    }
}
