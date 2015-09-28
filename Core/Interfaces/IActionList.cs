using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface IActionList
    {
        IEnumerable<ActionListDO> GetAll();
        ActionListDO GetByKey(IUnitOfWork uow, int curActionListId);
        void AddAction(ActionDO curActionDO, string position);
        void Process(ActionListDO curActionList, ProcessDO curProcessDO, IUnitOfWork uow);
        void UpdateActionListState(ActionListDO curActionListDO);
        void ProcessAction(ActionListDO curActionList, ProcessDO curProcessDO, IUnitOfWork uow);
    }
}
