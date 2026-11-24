using Clinic.Application.DTOs.Auth;
using FluentValidation;

namespace Clinic.Application.Validators;

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
