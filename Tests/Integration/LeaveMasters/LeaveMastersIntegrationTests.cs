using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.LeaveMasters.Dtos;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Integration.LeaveMasters;

public class LeaveMastersIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public LeaveMastersIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateLeaveMaster_ValidRequest_Returns201()
    {
        var client = _factory.CreateAdminClient();
        var code = $"ML{Guid.NewGuid():N}"[..10];

        var response = await client.PostAsJsonAsync("/api/v1/leave-masters",
            new { name = "Marriage Leave", code, quotaDays = 3, isActive = true });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveMasterDto>>();
        body!.Data!.Code.Should().Be(code);
    }

    [Fact]
    public async Task CreateLeaveMaster_DuplicateCode_Returns409()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/v1/leave-masters",
            new { name = "Annual Leave Duplicate", code = "AL", quotaDays = 5 });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllLeaveMasters_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.GetAsync("/api/v1/leave-masters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<IEnumerable<LeaveMasterDto>>>();
        body!.Data.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetLeaveMasterById_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.GetAsync($"/api/v1/leave-masters/{EntityBuilder.AnnualLeaveId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveMasterDto>>();
        body!.Data!.Code.Should().Be("AL");
    }

    [Fact]
    public async Task UpdateLeaveMaster_Returns200()
    {
        var client = _factory.CreateAdminClient();

        var createResp = await client.PostAsJsonAsync("/api/v1/leave-masters",
            new { name = $"Temp Leave {Guid.NewGuid():N}", code = $"TL{Guid.NewGuid():N}"[..10], quotaDays = 2 });
        var created = await createResp.Content.ReadFromJsonAsync<Response<LeaveMasterDto>>();

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/leave-masters/{created!.Data!.Id}",
            new { name = "Sick Leave Updated", quotaDays = 7, isActive = false });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<LeaveMasterDto>>();
        body!.Data!.Name.Should().Be("Sick Leave Updated");
    }

    [Fact]
    public async Task DeleteLeaveMaster_Returns200()
    {
        var client = _factory.CreateAdminClient();

        var createResp = await client.PostAsJsonAsync("/api/v1/leave-masters",
            new { name = $"Temp Delete {Guid.NewGuid():N}", code = $"TD{Guid.NewGuid():N}"[..10], quotaDays = 2 });
        var created = await createResp.Content.ReadFromJsonAsync<Response<LeaveMasterDto>>();

        var response = await client.DeleteAsync($"/api/v1/leave-masters/{created!.Data!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateLeaveMaster_Unauthenticated_Returns401()
    {
        var response = await _factory.CreateAnonClient()
            .PostAsJsonAsync("/api/v1/leave-masters", new { name = "Test Leave", code = "TL", quotaDays = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateLeaveMaster_ViewerRole_Returns403()
    {
        var response = await _factory.CreateViewerClient()
            .PostAsJsonAsync("/api/v1/leave-masters", new { name = "Test Leave", code = "TL2", quotaDays = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
