using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Fr8Data.DataTransferObjects;

namespace HubWeb.ViewModels.Validators
{
    public class PlanDTOValidator : AbstractValidator<PlanEmptyDTO>
    {
        public PlanDTOValidator()
        {
            RuleFor(ptdto => ptdto.Name).NotNull();
            RuleFor(ptdto => ptdto.Name).NotEmpty();
        }
    }
}