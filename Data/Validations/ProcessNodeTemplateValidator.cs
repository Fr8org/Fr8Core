using Data.Entities;
using FluentValidation;
using StructureMap;
using Data.Interfaces;
using System.Collections.Generic;
namespace Data.Validations
{
    public class ProcessNodeTemplatetValidator : AbstractValidator<ProcessNodeTemplateDO>
    {
        public ProcessNodeTemplatetValidator()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RuleFor(pntDO => pntDO.Id).GreaterThan(0).WithMessage("Id must be a positive int");
                RuleFor(pntDO => pntDO.Name).NotEmpty().WithMessage("Name is Required");
                RuleFor(pntDO => pntDO.TransitionKey).NotEmpty().WithMessage("TransitionKey is Required");

                RuleFor(pntDO => pntDO.ParentTemplateId).GreaterThan(0)
                    .Must(id => uow.ProcessNodeRepository.GetByKey(id) != null)
                    .WithMessage("ParentTemplateId must be a valid Id for ProcessTemplate");

                RuleFor(pntDO => pntDO.Criteria).NotNull()
                    .Must(lst => lst.Count > 0 )
                    .WithMessage("Must have at least one child Criteria");

                RuleFor(pntDO => pntDO.ActionLists).NotNull()
                    .Must(lst => lst.Count > 0)
                    .WithMessage("Must have at least one child ActionList");
            }
        }
    }
}
