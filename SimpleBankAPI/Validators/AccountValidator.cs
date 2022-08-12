using FluentValidation;
using SimpleBankAPI.Models;

namespace SimpleBankAPI.Validators
{
    public class AccountValidator : AbstractValidator<AccountModel>
    {
        public AccountValidator()
        {
            /*
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Created).NotEmpty();
            RuleFor(x => x.Priority).GreaterThan(0);
            RuleFor(x => x.IssueType).GreaterThan(0);
            */
        }
    }
}
