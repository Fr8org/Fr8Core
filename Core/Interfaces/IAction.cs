using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        IEnumerable<ActionTemplateDO> GetAvailableActions(IDockyardAccountDO curAccount);
        bool SaveOrUpdateAction(ActionDO currentActionDo);
        //void Register(string ActionType, string PluginRegistration, string Version);
        string GetConfigurationSettings(ActionDO curActionDO);
        IEnumerable<string> GetFieldDataSources(IUnitOfWork uow, ActionDO curActionDO);

        Task<IEnumerable<string>> GetFieldMappingTargets(ActionDO curActionDO);
        //void Register(string ActionType, string PluginRegistration, string Version);
        ActionDO GetById(int id);
        void Delete(int id);
        Task<int> Process(ActionDO curAction);
        string AddCrate(CrateDTO crateDTO, string crateStorage);
        List<CrateDTO> GetCrates(string crateStorage);
    }
}