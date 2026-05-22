using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.LeaveAllowances.Dtos;
using NetcoreHRIS.Security;

namespace NetcoreHRIS.Modules.LeaveAllowances;

[ApiController]
[Route("api/v{version:apiVersion}/leave-allowances")]
[Authorize]
[EnableRateLimiting("per-user")]
[Produces("application/json")]
public class LeaveAllowancesController : ControllerBase
{
    private readonly ILeaveAllowancesService _service;

    public LeaveAllowancesController(ILeaveAllowancesService service) => _service = service;

    [HttpPost]
    [HasPermission("create", "LeaveAllowance")]
    public async Task<ActionResult<Response<LeaveAllowanceDto>>> Create(
        [FromBody] CreateLeaveAllowanceRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            Response<LeaveAllowanceDto>.Ok(result, "Leave allowance created successfully."));
    }

    [HttpGet]
    [HasPermission("read", "LeaveAllowance")]
    public async Task<ActionResult<Response<IEnumerable<LeaveAllowanceDto>>>> GetAll(
        [FromQuery] ListLeaveAllowanceQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(Response<IEnumerable<LeaveAllowanceDto>>.Ok(
            result.Items,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("read", "LeaveAllowance")]
    public async Task<ActionResult<Response<LeaveAllowanceDto>>> GetById(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.GetByIdAsync(id, ct);
        return Ok(Response<LeaveAllowanceDto>.Ok(result));
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("update", "LeaveAllowance")]
    public async Task<ActionResult<Response<LeaveAllowanceDto>>> Update(
        Guid id,
        [FromBody] UpdateLeaveAllowanceRequest request,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(Response<LeaveAllowanceDto>.Ok(result, "Leave allowance updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("delete", "LeaveAllowance")]
    public async Task<ActionResult<Response<object?>>> Delete(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        await _service.DeleteAsync(id, ct);
        return Ok(Response<object?>.Ok(null, "Leave allowance deleted successfully."));
    }
}
