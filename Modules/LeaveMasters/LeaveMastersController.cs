using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.LeaveMasters.Dtos;
using NetcoreHRIS.Security;

namespace NetcoreHRIS.Modules.LeaveMasters;

[ApiController]
[Route("api/v{version:apiVersion}/leave-masters")]
[Authorize]
[EnableRateLimiting("per-user")]
[Produces("application/json")]
public class LeaveMastersController : ControllerBase
{
    private readonly ILeaveMastersService _service;

    public LeaveMastersController(ILeaveMastersService service) => _service = service;

    [HttpPost]
    [HasPermission("create", "LeaveMaster")]
    public async Task<ActionResult<Response<LeaveMasterDto>>> Create(
        [FromBody] CreateLeaveMasterRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            Response<LeaveMasterDto>.Ok(result, "Leave master created successfully."));
    }

    [HttpGet]
    [HasPermission("read", "LeaveMaster")]
    public async Task<ActionResult<Response<IEnumerable<LeaveMasterDto>>>> GetAll(
        [FromQuery] ListLeaveMasterQuery query,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(query, ct);
        return Ok(Response<IEnumerable<LeaveMasterDto>>.Ok(
            result.Items,
            meta: PaginationMeta.Create(result.Page, result.Limit, result.Total)));
    }

    [HttpGet("{id:guid}")]
    [HasPermission("read", "LeaveMaster")]
    public async Task<ActionResult<Response<LeaveMasterDto>>> GetById(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.GetByIdAsync(id, ct);
        return Ok(Response<LeaveMasterDto>.Ok(result));
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("update", "LeaveMaster")]
    public async Task<ActionResult<Response<LeaveMasterDto>>> Update(
        Guid id,
        [FromBody] UpdateLeaveMasterRequest request,
        CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        var result = await _service.UpdateAsync(id, request, ct);
        return Ok(Response<LeaveMasterDto>.Ok(result, "Leave master updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("delete", "LeaveMaster")]
    public async Task<ActionResult<Response<object?>>> Delete(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty)
            throw new BadHttpRequestException("Invalid ID");

        await _service.DeleteAsync(id, ct);
        return Ok(Response<object?>.Ok(null, "Leave master deleted successfully."));
    }
}
