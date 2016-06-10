using Fr8.Infrastructure.Data.DataTransferObjects;
using FluentValidation;

namespace Data.Validations
{
    public class TerminalValidator : AbstractValidator<TerminalDTO>
    {
        public TerminalValidator()
        {
            RuleFor(incidentDO => incidentDO).NotNull().WithMessage("IncidentDO is null or empty");
            RuleFor(incidentDO => incidentDO.Name).NotEmpty().WithMessage("Name is Required");
            RuleFor(incidentDO => incidentDO.Label).NotEmpty().WithMessage("Label is Required");
            RuleFor(incidentDO => incidentDO.Endpoint).NotEmpty().WithMessage("Endpoint is Required");
            RuleFor(incidentDO => incidentDO.Description).NotEmpty().WithMessage("Description is Required");
        }
    }
}
