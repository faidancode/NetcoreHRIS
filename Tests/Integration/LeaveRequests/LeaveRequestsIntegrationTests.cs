using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.LeaveRequests.Dtos;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Integration.LeaveRequests;

public class LeaveRequestsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public LeaveRequestsIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateLeaveRequest_ValidRequest_Returns201()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/v1/leave-requests",
            new
            {
                employeeId = EntityBuilder.Employee1Id,
                leaveId = EntityBuilder.AnnualLeaveId,
                fromDate = new DateOnly(2026, 1, 10),
                toDate = new DateOnly(2026, 1, 12),
                reason = "Family event"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveRequestDto>>();
        body!.Data!.RequestNo.Should().StartWith("LR-");
    }

    [Fact]
    public async Task GetAllLeaveRequests_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var createResp = await client.PostAsJsonAsync("/api/v1/leave-requests",
            new
            {
                employeeId = EntityBuilder.Employee1Id,
                leaveId = EntityBuilder.SickLeaveId,
                fromDate = new DateOnly(2026, 2, 10),
                toDate = new DateOnly(2026, 2, 11),
                reason = "Medical"
            });
        createResp.EnsureSuccessStatusCode();

        var response = await client.GetAsync("/api/v1/leave-requests");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<IEnumerable<LeaveRequestDto>>>();
        body!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLeaveRequest_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var createResp = await client.PostAsJsonAsync("/api/v1/leave-requests",
            new
            {
                employeeId = EntityBuilder.Employee2Id,
                leaveId = EntityBuilder.AnnualLeaveId,
                fromDate = new DateOnly(2026, 3, 10),
                toDate = new DateOnly(2026, 3, 12),
                reason = "Personal"
            });
        var created = await createResp.Content.ReadFromJsonAsync<Response<LeaveRequestDto>>();

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/leave-requests/{created!.Data!.Id}",
            new { reason = "Updated reason" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveRequestDto>>();
        body!.Data!.Reason.Should().Be("Updated reason");
    }

    [Fact]
    public async Task InvalidDateRange_Returns400()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/v1/leave-requests",
            new
            {
                employeeId = EntityBuilder.Employee1Id,
                leaveId = EntityBuilder.AnnualLeaveId,
                fromDate = new DateOnly(2026, 4, 12),
                toDate = new DateOnly(2026, 4, 10),
                reason = "Bad range"
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
