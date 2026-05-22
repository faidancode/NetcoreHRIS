using FluentValidation;
using NetcoreHRIS.Modules.LeaveMasters.Dtos;

namespace NetcoreHRIS.Modules.LeaveMasters.Validators;

public class CreateLeaveMasterRequestValidator : AbstractValidator<CreateLeaveMasterRequest>
{
    public CreateLeaveMasterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Leave master name is required.")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Leave master code is required.")
            .MinimumLength(2).WithMessage("Code must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.QuotaDays)
            .GreaterThan(0).WithMessage("Quota days must be greater than zero.")
            .LessThanOrEqualTo(365).WithMessage("Quota days must not exceed 365.");
    }
}

public class UpdateLeaveMasterRequestValidator : AbstractValidator<UpdateLeaveMasterRequest>
{
    public UpdateLeaveMasterRequestValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(3).WithMessage("Name must be at least 3 characters.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.")
            .When(x => x.Name != null);

        RuleFor(x => x.Code)
            .MinimumLength(2).WithMessage("Code must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .When(x => x.Code != null);

        RuleFor(x => x.QuotaDays)
            .GreaterThan(0).WithMessage("Quota days must be greater than zero.")
            .LessThanOrEqualTo(365).WithMessage("Quota days must not exceed 365.")
            .When(x => x.QuotaDays.HasValue);
    }
}

public class LeaveMasterListQueryValidator : AbstractValidator<ListLeaveMasterQuery>
{
    public LeaveMasterListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);

        RuleFor(x => x.Sort)
            .Matches(@"^[a-zA-Z]+:(asc|desc)$")
            .When(x => !string.IsNullOrEmpty(x.Sort));
    }
}
