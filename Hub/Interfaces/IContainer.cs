using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IContainer
    {
        ContainerDO Create(IUnitOfWork uow, Guid processTemplateId, Crate curEvent);
        Task<ContainerDO> Launch(RouteDO curRoute, Crate curEvent);
        Task Execute(IUnitOfWork uow, ContainerDO curContainerDO);
        //void SetProcessNextActivity(ProcessDO curProcessDO);

        IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork,
            Fr8AccountDO account, bool isAdmin = false, Guid? id = null);
    }
}