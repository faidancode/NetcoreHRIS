using FluentValidation;
using NetcoreHRIS.Modules.Reports.Dtos;

namespace NetcoreHRIS.Modules.Reports.Validators;

public class EmployeeReportQueryValidator : AbstractValidator<EmployeeReportQuery>
{
    public EmployeeReportQueryValidator()
    {
        Include(new ReportQueryRules<EmployeeReportQuery>());
        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

public class AttendanceReportQueryValidator : AbstractValidator<AttendanceReportQuery>
{
    public AttendanceReportQueryValidator()
    {
        Include(new ReportQueryRules<AttendanceReportQuery>());
        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

public class LeavesReportQueryValidator : AbstractValidator<LeavesReportQuery>
{
    public LeavesReportQueryValidator()
    {
        Include(new ReportQueryRules<LeavesReportQuery>());
        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}

public class ReportExportFormatValidator : AbstractValidator<ReportExportFormat?>
{
    public ReportExportFormatValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("Format is required.")
            .IsInEnum()
            .WithMessage("Format must be either xlsx or pdf.");
    }
}

internal class ReportQueryRules<T> : AbstractValidator<T>
{
    public ReportQueryRules()
    {
        RuleFor(x => GetIntProperty(x, "Page")).GreaterThan(0).WithName("Page");
        RuleFor(x => GetIntProperty(x, "Limit")).InclusiveBetween(1, 100).WithName("Limit");
        RuleFor(x => GetStringProperty(x, "Sort"))
            .Matches(@"^[a-zA-Z]+:(asc|desc)$")
            .When(x => !string.IsNullOrEmpty(GetStringProperty(x, "Sort")))
            .WithName("Sort");
    }

    private static int GetIntProperty(T value, string property)
        => (int)(typeof(T).GetProperty(property)?.GetValue(value) ?? 0);

    private static string? GetStringProperty(T value, string property)
        => (string?)typeof(T).GetProperty(property)?.GetValue(value);
}
