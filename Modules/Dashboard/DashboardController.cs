using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.Dashboard.Dtos;

namespace NetcoreHRIS.Modules.Dashboard;

[ApiController]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
[EnableRateLimiting("per-user")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<Response<DashboardSummaryDto>>> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return Ok(Response<DashboardSummaryDto>.Ok(result));
    }
}
