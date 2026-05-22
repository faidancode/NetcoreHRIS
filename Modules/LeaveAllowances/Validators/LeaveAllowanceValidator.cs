using FluentValidation;
using NetcoreHRIS.Modules.LeaveAllowances.Dtos;

namespace NetcoreHRIS.Modules.LeaveAllowances.Validators;

public class CreateLeaveAllowanceRequestValidator : AbstractValidator<CreateLeaveAllowanceRequest>
{
    public CreateLeaveAllowanceRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId is required.");

        RuleFor(x => x.LeaveId)
            .NotEmpty().WithMessage("LeaveId is required.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.");

        RuleFor(x => x.QuotaDays)
            .InclusiveBetween(1, 365).WithMessage("Quota days must be between 1 and 365.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => x.Notes != null);
    }
}

public class UpdateLeaveAllowanceRequestValidator : AbstractValidator<UpdateLeaveAllowanceRequest>
{
    public UpdateLeaveAllowanceRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId cannot be empty.")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.LeaveId)
            .NotEmpty().WithMessage("LeaveId cannot be empty.")
            .When(x => x.LeaveId.HasValue);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.")
            .When(x => x.Year.HasValue);

        RuleFor(x => x.QuotaDays)
            .InclusiveBetween(1, 365).WithMessage("Quota days must be between 1 and 365.")
            .When(x => x.QuotaDays.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
            .When(x => x.Notes != null);
    }
}

public class LeaveAllowanceListQueryValidator : AbstractValidator<ListLeaveAllowanceQuery>
{
    public LeaveAllowanceListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);

        RuleFor(x => x.Sort)
            .Matches(@"^[a-zA-Z]+:(asc|desc)$")
            .When(x => !string.IsNullOrEmpty(x.Sort));
    }
}
