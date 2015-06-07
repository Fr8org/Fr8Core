using Data.Entities;
using FluentValidation;


namespace Data.Validations
{
    public class IncidentValidator : AbstractValidator<IncidentDO>
    {
        public IncidentValidator()
        {
            RuleFor(incidentDO => incidentDO.PrimaryCategory).NotEmpty().WithMessage("PrimaryCategory is Required");
            RuleFor(incidentDO => incidentDO.Priority).NotEmpty().WithMessage("Priority is Required");
            //RuleFor(incidentDO => incidentDO.Notes).NotEmpty().WithMessage("Notes is Required");
        }
    }
}
