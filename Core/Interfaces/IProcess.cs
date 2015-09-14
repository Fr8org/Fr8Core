using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(int processTemplateId, CrateDTO curEvent);
        void Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent);
        void Execute(ProcessDO curProcessDO);
    }
}