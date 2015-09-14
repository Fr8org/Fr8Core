using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Data.Infrastructure.AutoMapper
{
    /// <summary>
    /// AutoMapper converter to convert ProcessTemplateDO to FullProcessTemplateDTO.
    /// </summary>
    public class ProcessTemplateDOFullConverter
        : ITypeConverter<ProcessTemplateDO, FullProcessTemplateDTO>
    {
        public const string UnitOfWork_OptionsKey = "UnitOfWork";


        public FullProcessTemplateDTO Convert(ResolutionContext context)
        {
            var processTemplate = (ProcessTemplateDO)context.SourceValue;
            var uow = (IUnitOfWork)context.Options.Items[UnitOfWork_OptionsKey];

            if (processTemplate == null)
            {
                return null;
            }

            var processNodeTemplateDTOList = uow.ProcessNodeTemplateRepository
                .GetQuery()
                .Include(x => x.Criteria)
                .Include(x => x.ActionLists)
                .Where(x => x.ParentTemplateId == processTemplate.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select(x => new FullProcessNodeTemplateDTO()
                {
                    ProcessNodeTemplate = Mapper.Map<ProcessNodeTemplateDTO>(x),
                    Criteria = Mapper.Map<CriteriaDTO>(x.Criteria),
                    ActionLists = x.ActionLists
                        .Select(y => new FullActionListDTO()
                        {
                            ActionList = Mapper.Map<ActionListDTO>(y),
                            Actions = y.Activities
                                .OfType<ActionDO>()
                                .Select(z => Mapper.Map<ActionDTO>(z))
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            var result = new FullProcessTemplateDTO()
            {
                ProcessTemplate = Mapper.Map<ProcessTemplateDTO>(processTemplate),
                ProcessNodeTemplates = processNodeTemplateDTOList
            };

            return result;
        }
    }
}
