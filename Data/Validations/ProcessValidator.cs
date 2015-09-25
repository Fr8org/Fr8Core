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
            // Commented out by yakov.gnusin. Breaks when process is saved for the first time.
            // RuleFor(processDO => processDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");

            RuleFor(processDO => processDO.ProcessTemplateId).NotEmpty()
                .GreaterThan(0)
                .Must(id => {
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        return uow.ProcessTemplateRepository.GetByKey(id) != null;
                    }
                })
                .WithMessage("ProcessTemplateId must be a required foreign key for ProcessTemplate");
        }
    }
}
