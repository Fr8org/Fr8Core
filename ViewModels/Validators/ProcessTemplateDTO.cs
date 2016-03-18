using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Data.Interfaces.DataTransferObjects;

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