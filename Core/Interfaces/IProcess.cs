using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    public interface IProcess
    {
        ProcessDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent);
        Task Launch(RouteDO curRoute, CrateDTO curEvent);
        Task Execute(IUnitOfWork uow, ProcessDO curProcessDO);
        //void SetProcessNextActivity(ProcessDO curProcessDO);
    }
}