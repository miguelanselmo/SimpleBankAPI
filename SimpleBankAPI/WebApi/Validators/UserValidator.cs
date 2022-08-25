using FluentValidation;
using SimpleBankAPI.WebApi.Models;

namespace SimpleBankAPI.WebApi.Validators;

public class RegisterValidator : AbstractValidator<registerRequest>
{
    public RegisterValidator()
    {
        RuleFor(x => x.UserName).MinimumLength(8).WithMessage("UserName is required (minimum length: 8).");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email address is required.");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("UserName is required (minimum length: 8).");
        RuleFor(x => x.FullName).MinimumLength(16).WithMessage("Full name is required (minimum length: 16).");
    }
}

internal class LoginValidator : AbstractValidator<loginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserName).MinimumLength(8).WithMessage("UserName is required (minimum length: 8).");
        RuleFor(x => x.Password).MinimumLength(8).WithMessage("UserName is required (minimum length: 8).");
    }
}