using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        IEnumerable<TViewModel> GetAllActionLists<TViewModel>();
        IEnumerable<string> GetAvailableActions(IDockyardAccountDO curAccount);
        bool SaveOrUpdateAction(ActionDO currentActionDo);
    }
}