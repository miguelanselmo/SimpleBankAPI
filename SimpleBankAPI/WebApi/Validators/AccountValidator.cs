using FluentValidation;
using FluentValidation.Validators;
using SimpleBankAPI.Core.Enums;
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
/*
public class CurrencyEnumValidator<T> : PropertyValidator
{

    public CurrencyEnumValidator() : base("Invalid Enum value!") { }

    protected override bool IsValid(PropertyValidatorContext context)
    {
        CurrencyEnum enumVal = (CurrencyEnum)Enum.Parse(typeof(CurrencyEnum), context.PropertyValue);

        if (!Enum.IsDefined(typeof(CurrencyEnum), enumVal))
          return false;
        return true;
    }
}
*/