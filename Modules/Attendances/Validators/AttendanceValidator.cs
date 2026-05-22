using FluentValidation;
using NetcoreHRIS.Modules.Attendances.Dtos;

namespace NetcoreHRIS.Modules.Attendances.Validators;

public class CreateAttendanceRequestValidator : AbstractValidator<CreateAttendanceRequest>
{
    public CreateAttendanceRequestValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId is required.");

        RuleFor(x => x.CheckIn)
            .NotNull().WithMessage("CheckIn is required.");

        RuleFor(x => x.CheckOut)
            .NotNull().WithMessage("CheckOut is required.");

        RuleFor(x => x.CheckOut)
            .GreaterThanOrEqualTo(x => x.CheckIn!.Value)
            .WithMessage("CheckOut must be greater than or equal to CheckIn.")
            .When(x => x.CheckIn.HasValue && x.CheckOut.HasValue);
    }
}

public class UpdateAttendanceRequestValidator : AbstractValidator<UpdateAttendanceRequest>
{
    public UpdateAttendanceRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("EmployeeId cannot be empty.")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.CheckOut)
            .GreaterThanOrEqualTo(x => x.CheckIn!.Value)
            .WithMessage("CheckOut must be greater than or equal to CheckIn.")
            .When(x => x.CheckIn.HasValue && x.CheckOut.HasValue);
    }
}

public class AttendanceListQueryValidator : AbstractValidator<ListAttendanceQuery>
{
    public AttendanceListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);

        RuleFor(x => x.Sort)
            .Matches(@"^[a-zA-Z]+:(asc|desc)$")
            .When(x => !string.IsNullOrEmpty(x.Sort));
    }
}
