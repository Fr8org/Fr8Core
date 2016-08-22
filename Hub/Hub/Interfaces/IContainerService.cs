using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface IContainerService
    {
        ContainerDO Create(IUnitOfWork uow, PlanDO plan, params Crate[] payload);
        Task Run(IUnitOfWork uow, ContainerDO container);
        Task Continue(IUnitOfWork uow, ContainerDO container);
        List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan);
        IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, Guid? id = null);
        PagedResultDTO<ContainerDTO> GetByQuery(Fr8AccountDO account, PagedQueryDTO query);
    }
}