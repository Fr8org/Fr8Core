using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using FluentValidation;
using StructureMap;

namespace Data.Validations
{
    public class ContainerValidator : AbstractValidator<ContainerDO>
    {
        public ContainerValidator()
        {
            // Commented out by yakov.gnusin. Breaks when process is saved for the first time.
            // RuleFor(containerDO => containerDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");



            RuleFor(containerDO => containerDO.PlanId).NotEmpty()
                .NotEqual(Guid.Empty)
                .Must(id => {
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        return uow.PlanRepository.GetById<PlanNodeDO>(id) != null;
                    }
                })
                .WithMessage("PlanId must be a required foreign key for Plan");
        }
    }
}
