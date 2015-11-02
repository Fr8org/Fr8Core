using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IContainer
    {
        ContainerDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent);
        Task Launch(RouteDO curRoute, CrateDTO curEvent);
        Task Execute(IUnitOfWork uow, ContainerDO curContainerDO);
        //void SetProcessNextActivity(ProcessDO curProcessDO);

        IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, int? id = null);
    }
}