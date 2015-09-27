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
        void ProcessInboundEvents(CrateDTO curCrateStandardEventReport);
        void LaunchProcess(ProcessTemplateDO curProcessTemplate, CrateDTO curEventData = null);
        void LaunchProcesses(List<ProcessTemplateDO> curProcessTemplates, CrateDTO curEventReport);
    }
}
