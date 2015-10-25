using Data.Entities;
using FluentValidation;


namespace Data.Validations
{
    public class ActionValidator : AbstractValidator<ActionDO>
    {
        public ActionValidator()
        {
            // TODO: remove this, DO-1397
            // RuleFor(curActionDO => curActionDO.ActivityTemplate)
            //     .Must(actionTemplate => string.IsNullOrEmpty(actionTemplate.AuthenticationType) || actionTemplate.AuthenticationType == "OAuth")
            //     .When(curActionDO => curActionDO.ActivityTemplate != null)
            //     .WithMessage("Must be a valid authentication type.");
        }
    }
}
