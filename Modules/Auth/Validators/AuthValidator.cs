using FluentValidation;
using NetcoreHRIS.Modules.Auth.Dtos;

namespace NetcoreHRIS.Modules.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class RefreshRequestValidator : AbstractValidator<RefreshRequest>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .MaximumLength(4096)
            .When(x => !string.IsNullOrWhiteSpace(x.RefreshToken));
    }
}
