using System;
using System.Collections.Generic;
using FluentValidation;
using StructureMap;
using Data.Entities;
using Data.Interfaces;

namespace Data.Validations
{
      // Is not used anywhere
    /*public class SubPlanetValidator : AbstractValidator<SubPlanDO>
    {
      
        public SubPlantValidator()
        {
            // Commented out. See https://maginot.atlassian.net/browse/DO-940 for more description.
            // RuleFor(pntDO => pntDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");

            RuleFor(pntDO => pntDO.Name).NotEmpty().WithMessage("Name is Required");

            // Commented out, since NodeTransitions is not used in other parts of code.
            // RuleFor(pntDO => pntDO.NodeTransitions).NotEmpty().WithMessage("NodeTransitions is Required");

            RuleFor(pntDO => pntDO.ParentPlanNodeId)
                .NotEqual(Guid.Empty)
                .Must(id =>
                {
                    // We should create internal UnitOfWork.
                    // By the time validation rule gets executed, external UnitOfWork will be disposed.
                    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                    {
                        var parentTemplateExists = id != null && (uow.PlanRepository.GetById<PlanNodeDO>(id.Value) != null);
                        return parentTemplateExists;
                    }
                })
                .WithMessage("ParentTemplateId must be a valid Id for Plan");

            RuleFor(pntDO => pntDO.Criteria).NotNull()
                .Must(lst => lst.Count > 0)
                .WithMessage("Must have at least one child Criteria");

            RuleFor(pntDO => pntDO.ChildNodes).NotNull()
                .Must(lst => lst.Count > 0)
                .WithMessage("Must have at least one child ActionList");
        }
    }*/
}
