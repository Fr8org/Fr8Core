using Data.Entities;
using FluentValidation;


namespace Data.Validations
{
    public class ActionValidator : AbstractValidator<ActivityDO>
    {
        public ActionValidator()
        {
            // TODO: remove this, DO-1397
            // RuleFor(curActivityDO => curActivityDO.ActivityTemplate)
            //     .Must(actionTemplate => string.IsNullOrEmpty(actionTemplate.AuthenticationType) || actionTemplate.AuthenticationType == "OAuth")
            //     .When(curActivityDO => curActivityDO.ActivityTemplate != null)
            //     .WithMessage("Must be a valid authentication type.");
        }
    }
}
