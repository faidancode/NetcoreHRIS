using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.Attendances.Dtos;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Integration.Attendances;

public class AttendancesIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public AttendancesIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateAttendance_ValidRequest_Returns201()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/v1/attendances",
            new
            {
                date = new DateOnly(2026, 5, 22),
                employeeId = EntityBuilder.Employee1Id,
                checkIn = new TimeOnly(7, 30),
                checkOut = new TimeOnly(16, 30)
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Response<AttendanceDto>>();
        body!.Data!.Status.Should().Be("OnTime");
    }

    [Fact]
    public async Task CreateAttendance_Duplicate_Returns409()
    {
        var client = _factory.CreateAdminClient();
        var payload = new
        {
            date = new DateOnly(2026, 5, 23),
            employeeId = EntityBuilder.Employee1Id,
            checkIn = new TimeOnly(7, 30),
            checkOut = new TimeOnly(16, 30)
        };

        var first = await client.PostAsJsonAsync("/api/v1/attendances", payload);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/v1/attendances", payload);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetAllAttendances_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.GetAsync("/api/v1/attendances");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<IEnumerable<AttendanceDto>>>();
        body!.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAttendance_Returns200()
    {
        var client = _factory.CreateAdminClient();
        var createResp = await client.PostAsJsonAsync("/api/v1/attendances",
            new
            {
                date = new DateOnly(2026, 5, 24),
                employeeId = EntityBuilder.Employee2Id,
                checkIn = new TimeOnly(7, 30),
                checkOut = new TimeOnly(16, 30)
            });
        var created = await createResp.Content.ReadFromJsonAsync<Response<AttendanceDto>>();

        var response = await client.PatchAsJsonAsync(
            $"/api/v1/attendances/{created!.Data!.Id}",
            new { checkIn = new TimeOnly(8, 15), checkOut = new TimeOnly(17, 15) });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<AttendanceDto>>();
        body!.Data!.Status.Should().Be("Late");
    }

    [Fact]
    public async Task InvalidCheckOut_Returns400()
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/v1/attendances",
            new
            {
                date = new DateOnly(2026, 5, 25),
                employeeId = EntityBuilder.Employee1Id,
                checkIn = new TimeOnly(8, 30),
                checkOut = new TimeOnly(7, 30)
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
