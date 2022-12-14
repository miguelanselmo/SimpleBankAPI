using FluentValidation;
using SimpleBankAPI.WebApi.Models;

namespace SimpleBankAPI.WebApi.Validators;

public class CreateAccountValidator : AbstractValidator<createAccountRequest>
{
    public CreateAccountValidator()
    {
        //var validator = new InlineValidator<Currency>();
        RuleFor(x => x.Amount).GreaterThan(99).WithMessage("Minimum amount is 100.");
        //RuleFor(x => x.Currency).IsInEnum().WithMessage("Currency not valid.");
        //_ = validator.RuleFor(x => x.Currency).IsInEnum().WithMessage("Currency not valid.");
    }
}

