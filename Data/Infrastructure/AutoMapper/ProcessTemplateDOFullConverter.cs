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
                .Include(x => x.Actions)
                .Where(x => x.ParentTemplateId == processTemplate.Id)
                .OrderBy(x => x.Id)
                .ToList()
                .Select((ProcessNodeTemplateDO x) =>
                {
                    var pntDTO = Mapper.Map<FullProcessNodeTemplateDTO>(x);
                    pntDTO.Actions = x.Actions.OfType<ActionDO>().Select(Mapper.Map<ActionDTO>).ToList();
                    return pntDTO;
                }).ToList();

            var result = Mapper.Map<ProcessTemplateDTO>(Mapper.Map<ProcessTemplateOnlyDTO>(processTemplate));
            result.ProcessNodeTemplates = processNodeTemplateDTOList;

            return result;
        }
    }
}
