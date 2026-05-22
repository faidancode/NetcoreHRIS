using FluentValidation;
using NetcoreHRIS.Modules.LeaveRequests.Dtos;

namespace NetcoreHRIS.Modules.LeaveRequests.Validators;

public class CreateLeaveRequestRequestValidator : AbstractValidator<CreateLeaveRequestRequest>
{
    public CreateLeaveRequestRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId is required.");

        RuleFor(x => x.LeaveId)
            .NotEmpty().WithMessage("LeaveId is required.");

        RuleFor(x => x.FromDate)
            .NotEmpty().WithMessage("FromDate is required.");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("ToDate is required.")
            .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("ToDate must be greater than or equal to FromDate.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");

        RuleFor(x => x.AttachmentPath)
            .MaximumLength(500).WithMessage("AttachmentPath must not exceed 500 characters.")
            .When(x => x.AttachmentPath != null);
    }
}

public class UpdateLeaveRequestRequestValidator : AbstractValidator<UpdateLeaveRequestRequest>
{
    public UpdateLeaveRequestRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId cannot be empty.")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.LeaveId)
            .NotEmpty().WithMessage("LeaveId cannot be empty.")
            .When(x => x.LeaveId.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value).WithMessage("ToDate must be greater than or equal to FromDate.")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);

        RuleFor(x => x.Reason)
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.")
            .When(x => x.Reason != null);

        RuleFor(x => x.AttachmentPath)
            .MaximumLength(500).WithMessage("AttachmentPath must not exceed 500 characters.")
            .When(x => x.AttachmentPath != null);
    }
}

public class LeaveRequestListQueryValidator : AbstractValidator<ListLeaveRequestQuery>
{
    public LeaveRequestListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);

        RuleFor(x => x.Sort)
            .Matches(@"^[a-zA-Z]+:(asc|desc)$")
            .When(x => !string.IsNullOrEmpty(x.Sort));
    }
}
