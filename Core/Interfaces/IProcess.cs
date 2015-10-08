using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(int processTemplateId, CrateDTO curEvent);
        void Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent);
        Task Execute(ProcessDO curProcessDO);
        void SetProcessNextActivity(ProcessDO curProcessDO);
    }
}