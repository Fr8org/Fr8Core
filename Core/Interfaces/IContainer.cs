using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;

namespace Core.Interfaces
{
    public interface IContainer
    {
        ContainerDO Create(IUnitOfWork uow, int processTemplateId, CrateDTO curEvent);
        Task Launch(ProcessTemplateDO curProcessTemplate, CrateDTO curEvent);
        Task Execute(IUnitOfWork uow, ContainerDO curContainerDO);
        //void SetProcessNextActivity(ProcessDO curProcessDO);

        IList<ContainerDO> GetByDockyardAccount(string userId, bool isAdmin = false, int? id = null);

    }
}