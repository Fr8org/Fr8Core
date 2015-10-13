using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent);
        Task Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent);
        Task Execute(IUnitOfWork uow, ProcessDO curProcessDO);

        IList<ProcessDO> GetProcessOfAccount(string userId, bool isAdmin = false, int? id = null, int? status = null);

        //void SetProcessNextActivity(ProcessDO curProcessDO);
    }
}