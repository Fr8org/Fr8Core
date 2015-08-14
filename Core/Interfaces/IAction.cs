using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        IEnumerable<ActionRegistrationDO> GetAvailableActions(IDockyardAccountDO curAccount);
        bool SaveOrUpdateAction(ActionDO currentActionDo);
        void Register(string ActionType, string PluginRegistration, string Version);
        ActionDO GetById(int id);
        void Delete(int id);
    }
}