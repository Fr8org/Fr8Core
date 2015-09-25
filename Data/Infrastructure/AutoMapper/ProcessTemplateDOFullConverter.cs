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
        : ITypeConverter<ProcessTemplateDO, ProcessTemplateDTO>
    {
        public const string UnitOfWork_OptionsKey = "UnitOfWork";


        public ProcessTemplateDTO Convert(ResolutionContext context)
        {
            var processTemplate = (ProcessTemplateDO)context.SourceValue;
            var uow = (IUnitOfWork)context.Options.Items[UnitOfWork_OptionsKey];

            if (processTemplate == null)
            {
                return null;
            }

            var processNodeTemplateDTOList = uow.ProcessNodeTemplateRepository
                .GetQuery()
                .Include(x => x.ActionLists)
                .Where(x => x.ParentTemplateId == processTemplate.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select((ProcessNodeTemplateDO x) =>
                {
                    var pntDTO = Mapper.Map<FullProcessNodeTemplateDTO>(x);
                    pntDTO.ActionLists = x.ActionLists.Select(y =>
                    {
                        var actionList = Mapper.Map<FullActionListDTO>(y);
                        actionList.Actions = y.Activities.OfType<ActionDO>()
                                .Select(z => Mapper.Map<ActionDTO>(z))
                                .ToList();
                        return actionList;
                    }).ToList();
                    return pntDTO;
                }).ToList();

            var result = Mapper.Map<ProcessTemplateDTO>(Mapper.Map<ProcessTemplateOnlyDTO>(processTemplate));
            result.ProcessNodeTemplates = processNodeTemplateDTOList;

            return result;
        }
    }
}
