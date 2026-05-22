using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.LeaveRequests.Dtos;
using NetcoreHRIS.Security;

namespace NetcoreHRIS.Modules.LeaveRequests;

[ApiController]
[Route("api/v{version:apiVersion}/leave-requests")]
[Authorize]
[EnableRateLimiting("per-user")]
[Produces("application/json")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestsService _service;

    public LeaveRequestsController(ILeaveRequestsService service) => _service = service;

    [HttpPost]
    [HasPermission("create", "LeaveRequest")]
    public async Task<ActionResult<Response<LeaveRequestDto>>> Create(
        [FromBody] CreateLeaveRequestRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            Response<LeaveRequestDto>.Ok(result, "Leave request created successfully."));
    }

    [HttpGet]
    [HasPermission("read", "LeaveRequest")]
    public async Task<ActionResult<Response<IEnumerable<LeaveRequestDto>>>> GetAll(
        [FromQuery] ListLeaveRequestQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(Response<IEnumerable<LeaveRequestDto>>.Ok(
            result.Items,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("read", "LeaveRequest")]
    public async Task<ActionResult<Response<LeaveRequestDto>>> GetById(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.GetByIdAsync(id, ct);
        return Ok(Response<LeaveRequestDto>.Ok(result));
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("update", "LeaveRequest")]
    public async Task<ActionResult<Response<LeaveRequestDto>>> Update(
        Guid id,
        [FromBody] UpdateLeaveRequestRequest request,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(Response<LeaveRequestDto>.Ok(result, "Leave request updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("delete", "LeaveRequest")]
    public async Task<ActionResult<Response<object?>>> Delete(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        await _service.DeleteAsync(id, ct);
        return Ok(Response<object?>.Ok(null, "Leave request deleted successfully."));
    }
}
