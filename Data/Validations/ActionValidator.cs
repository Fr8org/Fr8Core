using Data.Entities;
using FluentValidation;


namespace Data.Validations
{
    public class ActionValidator : AbstractValidator<ActionDO>
    {
        public ActionValidator()
        {
            RuleFor(curActionDO => curActionDO.ActionTemplate)
                .Must(actionTemplate => string.IsNullOrEmpty(actionTemplate.AuthenticationType) || actionTemplate.AuthenticationType == "OAuth")
                .When(curActionDO => curActionDO.ActionTemplate != null)
                .WithMessage("Must be a valid authentication type.");

        }
    }
}
