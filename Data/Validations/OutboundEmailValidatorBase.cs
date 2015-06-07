using System.Linq;
using Data.Entities;
using FluentValidation;

namespace Data.Validations
{
    public abstract class OutboundEmailValidatorBase : AbstractValidator<EmailDO>
    {
        protected OutboundEmailValidatorBase()
        {
            RuleFor(e => e.Subject).NotEmpty().Length(5, int.MaxValue).WithMessage("Email subject must be at least 5 characters long.");
            RuleFor(e => e.To).Must(to => to.Any()).WithMessage("Email must have at least one TO recipient.");
        }
    }
}
