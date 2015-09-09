using System.Collections.Generic;
using Data.Entities;

namespace Core.Interfaces
{
    public interface IActionList
    {
        IEnumerable<ActionListDO> GetAll();
        ActionListDO GetByKey(int curActionListId);
        void AddAction(ActionDO curActionDO, string position);
        void Process(ActionListDO curActionList);
        void UpdateActionListState(ActionListDO curActionListDO);
        void ProcessAction(ActionListDO curActionList);
    }
}
