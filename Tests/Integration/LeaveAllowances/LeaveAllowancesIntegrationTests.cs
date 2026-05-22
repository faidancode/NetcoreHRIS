using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.LeaveAllowances.Dtos;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Integration.LeaveAllowances;

public class LeaveAllowancesIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public LeaveAllowancesIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateLeaveAllowance_ValidRequest_Returns201()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/v1/leave-allowances",
            new { employeeId = EntityBuilder.Employee1Id, leaveId = EntityBuilder.AnnualLeaveId, year = 2026, quotaDays = 12, notes = "Annual quota" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveAllowanceDto>>();
        body!.Data!.EmployeeId.Should().Be(EntityBuilder.Employee1Id);
    }

    [Fact]
    public async Task CreateLeaveAllowance_Duplicate_Returns409()
    {
        var client = _factory.CreateAdminClient();
        var payload = new { employeeId = EntityBuilder.Employee1Id, leaveId = EntityBuilder.AnnualLeaveId, year = 2027, quotaDays = 12 };

        var first = await client.PostAsJsonAsync("/api/v1/leave-allowances", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/leave-allowances", payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllLeaveAllowances_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.GetAsync("/api/v1/leave-allowances");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<IEnumerable<LeaveAllowanceDto>>>();
        body!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLeaveAllowanceById_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var createResp = await client.PostAsJsonAsync("/api/v1/leave-allowances",
            new { employeeId = EntityBuilder.Employee1Id, leaveId = EntityBuilder.SickLeaveId, year = 2028, quotaDays = 6 });
        var created = await createResp.Content.ReadFromJsonAsync<Response<LeaveAllowanceDto>>();

        var response = await client.GetAsync($"/api/v1/leave-allowances/{created!.Data!.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateLeaveAllowance_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var createResp = await client.PostAsJsonAsync("/api/v1/leave-allowances",
            new { employeeId = EntityBuilder.Employee2Id, leaveId = EntityBuilder.AnnualLeaveId, year = 2029, quotaDays = 12 });
        var created = await createResp.Content.ReadFromJsonAsync<Response<LeaveAllowanceDto>>();

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/leave-allowances/{created!.Data!.Id}",
            new { quotaDays = 14, notes = "Updated quota" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveAllowanceDto>>();
        body!.Data!.QuotaDays.Should().Be(14);
    }

    [Fact]
    public async Task CreateLeaveAllowance_Unauthenticated_Returns401()
    {
        var response = await _factory.CreateAnonClient().PostAsJsonAsync("/api/v1/leave-allowances",
            new { employeeId = EntityBuilder.Employee1Id, leaveId = EntityBuilder.AnnualLeaveId, year = 2030, quotaDays = 12 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
