using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.Dashboard.Dtos;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Integration.Dashboard;

public class DashboardIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public DashboardIntegrationTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetDashboard_ReturnsSummary()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync("/api/v1/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<Response<DashboardSummaryDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.TotalDepartments.Should().Be(2);
        body.Data!.TotalPositions.Should().Be(2);
        body.Data!.TotalActiveEmployees.Should().Be(2);
        body.Data!.TotalMaleEmployees.Should().Be(1);
        body.Data!.TotalFemaleEmployees.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboard_Unauthenticated_Returns401()
    {
        var response = await _factory.CreateAnonClient().GetAsync("/api/v1/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
