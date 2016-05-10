using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Hub.Interfaces
{
    public interface IContainer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <returns></returns>
        Task Run(IUnitOfWork uow, ContainerDO curContainerDO);
        //void SetProcessNextActivity(ProcessDO curProcessDO);
        List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan);

        IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork,
            Fr8AccountDO account, bool isAdmin = false, Guid? id = null);
    }
}