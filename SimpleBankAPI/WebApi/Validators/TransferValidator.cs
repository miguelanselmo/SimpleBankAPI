using FluentValidation;
using SimpleBankAPI.WebApi.Models;

namespace SimpleBankAPI.WebApi.Validators;

public class TransferValidator : AbstractValidator<transferRequest>
{
    public TransferValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(99).WithMessage("Amount must be greater than 100.");
        RuleFor(x => x.FromAccountId).NotEmpty().WithMessage("Account (from) is required.");
        RuleFor(x => x.ToAccountId).NotEmpty().WithMessage("Account (to) is required.");
    }
}
