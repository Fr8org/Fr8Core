using Data.Entities;
using FluentValidation;
using StructureMap;
using Data.Interfaces;
using System.Collections.Generic;
namespace Data.Validations
{
    public class ContainerValidator : AbstractValidator<ContainerDO>
    {
        public ContainerValidator()
        {
            // Commented out by yakov.gnusin. Breaks when process is saved for the first time.
            // RuleFor(containerDO => containerDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");

            RuleFor(containerDO => containerDO.RouteId).NotEmpty()
                .GreaterThan(0)
                .Must(id => {
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        return uow.RouteRepository.GetByKey(id) != null;
                    }
                })
                .WithMessage("RouteId must be a required foreign key for Route");
        }
    }
}
