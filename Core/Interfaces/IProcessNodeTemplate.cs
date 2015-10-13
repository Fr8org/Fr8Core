using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    /// <summary>
    /// ProcessNodeTemplate service.
    /// </summary>
    public interface IProcessNodeTemplate
    {
        void Create(IUnitOfWork uow, ProcessNodeTemplateDO processNodeTemplate);
        void Update(IUnitOfWork uow, ProcessNodeTemplateDO processNodeTemplate);
        void Delete(IUnitOfWork uow, int id);
        void AddAction(IUnitOfWork uow, ActionDO resultActionDo);
    }
}
