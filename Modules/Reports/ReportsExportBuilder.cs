using ClosedXML.Excel;
using NetcoreHRIS.Modules.Reports.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NetcoreHRIS.Modules.Reports;

public static class ReportsExportBuilder
{
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    public static ReportExportResult BuildEmployeeReport(
        ReportExportFormat format,
        EmployeeReportSummaryDto summary,
        IReadOnlyCollection<EmployeeReportItemDto> items,
        string fileName)
    {
        ReportsService.EnsureSupportedFormat(format);
        return format == ReportExportFormat.Pdf
            ? new ReportExportResult(BuildPdf("Employee Report", EmployeeSummaryRows(summary), EmployeeHeaders(), EmployeeRows(items)), PdfContentType, fileName)
            : new ReportExportResult(BuildWorkbook("Employee Report", EmployeeSummaryRows(summary), EmployeeHeaders(), EmployeeRows(items)), XlsxContentType, fileName);
    }

    public static ReportExportResult BuildAttendanceReport(
        ReportExportFormat format,
        AttendanceReportSummaryDto summary,
        IReadOnlyCollection<AttendanceReportItemDto> items,
        string fileName)
    {
        ReportsService.EnsureSupportedFormat(format);
        return format == ReportExportFormat.Pdf
            ? new ReportExportResult(BuildPdf("Attendance Report", AttendanceSummaryRows(summary), AttendanceHeaders(), AttendanceRows(items)), PdfContentType, fileName)
            : new ReportExportResult(BuildWorkbook("Attendance Report", AttendanceSummaryRows(summary), AttendanceHeaders(), AttendanceRows(items)), XlsxContentType, fileName);
    }

    public static ReportExportResult BuildLeavesReport(
        ReportExportFormat format,
        LeavesReportSummaryDto summary,
        IReadOnlyCollection<LeavesReportItemDto> items,
        string fileName)
    {
        ReportsService.EnsureSupportedFormat(format);
        return format == ReportExportFormat.Pdf
            ? new ReportExportResult(BuildPdf("Leaves Report", LeavesSummaryRows(summary), LeavesHeaders(), LeavesRows(items)), PdfContentType, fileName)
            : new ReportExportResult(BuildWorkbook("Leaves Report", LeavesSummaryRows(summary), LeavesHeaders(), LeavesRows(items)), XlsxContentType, fileName);
    }

    private static byte[] BuildWorkbook(
        string title,
        IReadOnlyCollection<(string Label, object Value)> summaryRows,
        IReadOnlyCollection<string> headers,
        IReadOnlyCollection<IReadOnlyCollection<object?>> detailRows)
    {
        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = title;
        summary.Cell(2, 1).Value = "Generated At";
        summary.Cell(2, 2).Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'");

        var row = 4;
        foreach (var item in summaryRows)
        {
            summary.Cell(row, 1).Value = item.Label;
            summary.Cell(row, 2).Value = item.Value?.ToString() ?? string.Empty;
            row++;
        }

        summary.Columns().AdjustToContents();

        var details = workbook.Worksheets.Add("Details");
        for (var col = 0; col < headers.Count; col++)
        {
            details.Cell(1, col + 1).Value = headers.ElementAt(col);
            details.Cell(1, col + 1).Style.Font.Bold = true;
        }

        row = 2;
        foreach (var detail in detailRows)
        {
            var col = 1;
            foreach (var value in detail)
            {
                details.Cell(row, col).Value = value?.ToString() ?? string.Empty;
                col++;
            }
            row++;
        }

        details.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(
        string title,
        IReadOnlyCollection<(string Label, object Value)> summaryRows,
        IReadOnlyCollection<string> headers,
        IReadOnlyCollection<IReadOnlyCollection<object?>> detailRows)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));
                page.Header().Column(column =>
                {
                    column.Item().Text(title).FontSize(18).Bold();
                    column.Item().Text($"Generated At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC").FontSize(8);
                });
                page.Content().Column(column =>
                {
                    column.Spacing(12);
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var item in summaryRows)
                        {
                            table.Cell().Element(SummaryCell).Text(item.Label);
                            table.Cell().Element(SummaryCell).Text(item.Value?.ToString() ?? string.Empty);
                        }
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in headers)
                                columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            foreach (var value in headers)
                                header.Cell().Element(HeaderCell).Text(value);
                        });

                        foreach (var row in detailRows.Take(200))
                        {
                            foreach (var value in row)
                                table.Cell().Element(BodyCell).Text(value?.ToString() ?? string.Empty);
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    private static IContainer SummaryCell(IContainer container)
        => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);

    private static IContainer HeaderCell(IContainer container)
        => container.Background(Colors.Grey.Lighten3).Border(1).BorderColor(Colors.Grey.Lighten1).Padding(3);

    private static IContainer BodyCell(IContainer container)
        => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3);

    private static IReadOnlyCollection<(string Label, object Value)> EmployeeSummaryRows(EmployeeReportSummaryDto summary)
        => new List<(string, object)>
        {
            ("Total Employees", summary.TotalEmployees),
            ("Total Active Employees", summary.TotalActiveEmployees),
            ("Total Inactive Employees", summary.TotalInactiveEmployees),
            ("Total Permanent Employees", summary.TotalPermanentEmployees),
            ("Total Contract Employees", summary.TotalContractEmployees),
            ("Total Male Employees", summary.TotalMaleEmployees),
            ("Total Female Employees", summary.TotalFemaleEmployees)
        };

    private static IReadOnlyCollection<string> EmployeeHeaders()
        => new[] { "NIP", "Full Name", "Gender", "Employment Type", "Employee Status", "Active", "Department", "Position", "Date Of Joining" };

    private static IReadOnlyCollection<IReadOnlyCollection<object?>> EmployeeRows(IEnumerable<EmployeeReportItemDto> items)
        => items.Select(x => (IReadOnlyCollection<object?>)new object?[]
        {
            x.Nip, x.FullName, x.Gender, x.EmploymentType, x.EmployeeStatus, x.IsActive, x.DepartmentName, x.PositionName, x.DateOfJoining
        }).ToList();

    private static IReadOnlyCollection<(string Label, object Value)> AttendanceSummaryRows(AttendanceReportSummaryDto summary)
        => new List<(string, object)>
        {
            ("Total Attendance Records", summary.TotalAttendanceRecords),
            ("Total On Time", summary.TotalOnTime),
            ("Total Late", summary.TotalLate),
            ("Total Employees With Attendance", summary.TotalEmployeesWithAttendance),
            ("Total Missing Check Out", summary.TotalMissingCheckOut)
        };

    private static IReadOnlyCollection<string> AttendanceHeaders()
        => new[] { "Date", "NIP", "Employee", "Department", "Position", "Check In", "Check Out", "Status" };

    private static IReadOnlyCollection<IReadOnlyCollection<object?>> AttendanceRows(IEnumerable<AttendanceReportItemDto> items)
        => items.Select(x => (IReadOnlyCollection<object?>)new object?[]
        {
            x.Date, x.EmployeeNip, x.EmployeeName, x.DepartmentName, x.PositionName, x.CheckIn, x.CheckOut, x.Status
        }).ToList();

    private static IReadOnlyCollection<(string Label, object Value)> LeavesSummaryRows(LeavesReportSummaryDto summary)
        => new List<(string, object)>
        {
            ("Total Leave Requests", summary.TotalLeaveRequests),
            ("Total Leave Days", summary.TotalLeaveDays),
            ("Total Employees Taking Leave", summary.TotalEmployeesTakingLeave)
        };

    private static IReadOnlyCollection<string> LeavesHeaders()
        => new[] { "Request No", "NIP", "Employee", "Department", "Leave Type", "From Date", "To Date", "Total Days", "Reason" };

    private static IReadOnlyCollection<IReadOnlyCollection<object?>> LeavesRows(IEnumerable<LeavesReportItemDto> items)
        => items.Select(x => (IReadOnlyCollection<object?>)new object?[]
        {
            x.RequestNo, x.EmployeeNip, x.EmployeeName, x.DepartmentName, x.LeaveName, x.FromDate, x.ToDate, x.TotalDays, x.Reason
        }).ToList();
}
