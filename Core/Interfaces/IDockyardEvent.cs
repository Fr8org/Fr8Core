using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.ManifestSchemas;

namespace Core.Interfaces
{
    public interface IDockyardEvent
    {
        Task ProcessInboundEvents(CrateDTO curCrateStandardEventReport);
        Task LaunchProcess(ProcessTemplateDO curProcessTemplate, CrateDTO curEventData = null);
        Task LaunchProcesses(List<ProcessTemplateDO> curProcessTemplates, CrateDTO curEventReport);
    }
}
