using Data.Entities;
using FluentValidation;
using StructureMap;
using Data.Interfaces;
using System.Collections.Generic;
namespace Data.Validations
{
    public class SubroutetValidator : AbstractValidator<SubrouteDO>
    {
        public SubroutetValidator()
        {
            // Commented out. See https://maginot.atlassian.net/browse/DO-940 for more description.
            // RuleFor(pntDO => pntDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");

            RuleFor(pntDO => pntDO.Name).NotEmpty().WithMessage("Name is Required");

            // Commented out, since NodeTransitions is not used in other parts of code.
            // RuleFor(pntDO => pntDO.NodeTransitions).NotEmpty().WithMessage("NodeTransitions is Required");

            RuleFor(pntDO => pntDO.ParentActivityId).GreaterThan(0)
                .Must(id =>
                {
                    // We should create internal UnitOfWork.
                    // By the time validation rule gets executed, external UnitOfWork will be disposed.
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var parentTemplateExists = (uow.RouteRepository.GetByKey(id) != null);
                        return parentTemplateExists;
                    }
                })
                .WithMessage("ParentTemplateId must be a valid Id for Route");

            RuleFor(pntDO => pntDO.Criteria).NotNull()
                .Must(lst => lst.Count > 0)
                .WithMessage("Must have at least one child Criteria");

            RuleFor(pntDO => pntDO.Activities).NotNull()
                .Must(lst => lst.Count > 0)
                .WithMessage("Must have at least one child ActionList");
        }
    }
}
