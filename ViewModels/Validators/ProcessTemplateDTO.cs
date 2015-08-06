using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Web.ViewModels.Validators
{
    public class ProcessTemplateDTOValidator : AbstractValidator<ProcessTemplateDTO>
    {
        public ProcessTemplateDTOValidator()
        {
            RuleFor(ptdto => ptdto.Name).NotNull();
            RuleFor(ptdto => ptdto.Name).NotEmpty();
        }
    }
}