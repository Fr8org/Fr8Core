using Data.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Data.Validations
{
    public class EventValidator : AbstractValidator<EventDO>
    {
        public EventValidator()
        {
            RuleFor(eventDO => eventDO.StartDate)
                .NotEmpty()
                .WithMessage("Start Date is Required");

            RuleFor(eventDO => eventDO.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(eventDO => eventDO.StartDate)
                .WithMessage("End date must after Start date");
        }

        //=================================================================
        //Utilities 
        //TO DO: Genericize this
        public void ValidateEvent(EventDO curEventDO)
        {
            ValidationResult results = Validate(curEventDO);
            if (results.IsValid)
                return;

            throw new ValidationException(results.Errors);

        }
    }
}
