using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NetcoreHRIS.Common.Models;
using NetcoreHRIS.Modules.Attendances.Dtos;
using NetcoreHRIS.Modules.LeaveRequests.Dtos;
using NetcoreHRIS.Modules.Reports.Dtos;
using NetcoreHRIS.Tests.Helpers;

namespace NetcoreHRIS.Tests.Integration.Reports;

public class ReportsIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ReportsIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetEmployeeReport_AsAdmin_ReturnsSummaryAndItems()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync($"/api/v1/reports/employees?departmentId={EntityBuilder.EngineeringId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<EmployeeReportDto>>();
        body!.Data!.Summary.TotalEmployees.Should().Be(1);
        body.Data.Items.Should().ContainSingle(x => x.DepartmentId == EntityBuilder.EngineeringId);
        body.Meta!.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetEmployeeReport_WithoutReportPermission_Returns403()
    {
        var client = _factory.CreateViewerClient();

        var response = await client.GetAsync("/api/v1/reports/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAttendanceReport_WithStatusFilter_ReturnsFilteredSummary()
    {
        var client = _factory.CreateAdminClient();
        await client.PostAsJsonAsync("/api/v1/attendances", new
        {
            date = new DateOnly(2026, 6, 1),
            employeeId = EntityBuilder.Employee1Id,
            checkIn = new TimeOnly(8, 45),
            checkOut = new TimeOnly(17, 0)
        });

        var response = await client.GetAsync("/api/v1/reports/attendances?attendanceStatus=Late&fromDate=2026-06-01&toDate=2026-06-01");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<AttendanceReportDto>>();
        body!.Data!.Summary.TotalAttendanceRecords.Should().Be(1);
        body.Data.Summary.TotalLate.Should().Be(1);
        body.Data.Items.Should().ContainSingle(x => x.Status == "Late");
    }

    [Fact]
    public async Task GetLeavesReport_WithLeaveFilter_ReturnsTotalDays()
    {
        var client = _factory.CreateAdminClient();
        await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId = EntityBuilder.Employee1Id,
            leaveId = EntityBuilder.AnnualLeaveId,
            fromDate = new DateOnly(2026, 7, 1),
            toDate = new DateOnly(2026, 7, 3),
            reason = "Family event"
        });

        var response = await client.GetAsync($"/api/v1/reports/leaves?leaveId={EntityBuilder.AnnualLeaveId}&fromDate=2026-07-01&toDate=2026-07-03");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Response<LeavesReportDto>>();
        body!.Data!.Summary.TotalLeaveRequests.Should().Be(1);
        body.Data.Summary.TotalLeaveDays.Should().Be(3);
        body.Data.Items.Should().ContainSingle(x => x.TotalDays == 3);
    }

    [Fact]
    public async Task ExportEmployeeReport_Xlsx_ReturnsExcelFile()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync("/api/v1/reports/employees/export?format=Xlsx");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        response.Content.Headers.ContentDisposition!.FileNameStar.Should().EndWith(".xlsx");
    }

    [Fact]
    public async Task ExportEmployeeReport_Pdf_ReturnsPdfFile()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync("/api/v1/reports/employees/export?format=Pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        response.Content.Headers.ContentDisposition!.FileNameStar.Should().EndWith(".pdf");
    }

    [Fact]
    public async Task ExportEmployeeReport_InvalidFormat_Returns400()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.GetAsync("/api/v1/reports/employees/export?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
