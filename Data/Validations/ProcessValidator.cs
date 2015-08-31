using Data.Entities;
using FluentValidation;
using StructureMap;
using Data.Interfaces;
using System.Collections.Generic;
namespace Data.Validations
{
    public class ProcessValidator : AbstractValidator<ProcessDO>
    {
        public ProcessValidator()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RuleFor(processDO => processDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");

                RuleFor(processDO => processDO.ProcessTemplateId).NotEmpty()
                    .GreaterThan(0)
                    .Must(id => uow.ProcessTemplateRepository.GetByKey(id) != null)
                    .WithMessage("ProcessTemplateId must be a required foreign key for ProcessTemplate");

            }
        }
    }
}
