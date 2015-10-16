using System.ComponentModel.DataAnnotations;
using FluentValidation;
using Data.Interfaces.DataTransferObjects;

namespace Web.ViewModels.Validators
{
    public class RouteDTOValidator : AbstractValidator<RouteOnlyDTO>
    {
        public RouteDTOValidator()
        {
            RuleFor(ptdto => ptdto.Name).NotNull();
            RuleFor(ptdto => ptdto.Name).NotEmpty();
        }
    }
}