using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        IEnumerable<ActionTemplateDO> GetAvailableActions(IDockyardAccountDO curAccount);
        bool SaveOrUpdateAction(ActionDO currentActionDo);
        //void Register(string ActionType, string PluginRegistration, string Version);
        string GetConfigurationSettings(ActionTemplateDO curActionTemplateDo);
        IEnumerable<string> GetFieldDataSources(IUnitOfWork uow, ActionDO curActionDO);

        Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curActionDO);
        //void Register(string ActionType, string PluginRegistration, string Version);
        ActionDO GetById(int id);
        void Delete(int id);
        Task<int> Process(ActionDO curAction);
    }
}