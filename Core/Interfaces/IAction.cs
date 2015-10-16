using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json.Linq;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        ActionDO SaveOrUpdateAction(ActionDO currentActionDo);
        ActionDO SaveOrUpdateAction(IUnitOfWork uow, ActionDO currentActionDo);
        Task<ActionDTO> Configure(ActionDO curActionDO);
        ActionDO GetById(int id);
        ActionDO GetById(IUnitOfWork uow, int id);
        void Delete(int id);
        ActionDO MapFromDTO(ActionDTO curActionDTO);
        Task<int> PrepareToExecute(ActionDO curAction, ContainerDO curContainersDO, IUnitOfWork uow);
        Task<PayloadDTO> Execute(ActionDO curActionDO, ContainerDO curContainerDO);
        string Authenticate(ActionDO curActionDO);
        void AddCrate(ActionDO curActionDO, List<CrateDTO> curCrateDTOLists);
        List<CrateDTO> GetCrates(ActionDO curActionDO);
        Task<ActionDTO> Activate(ActionDO curActionDO);
        Task<ActionDTO> Deactivate(ActionDO curActionDO);
        IEnumerable<CrateDTO> GetCratesByManifestType(string curManifestType, CrateStorageDTO curCrateStorageDTO);
        IEnumerable<CrateDTO> GetCratesByLabel(string curLabel, CrateStorageDTO curCrateStorageDTO);
		StandardConfigurationControlsCM GetConfigurationControls(ActionDO curActionDO);
        StandardConfigurationControlsCM GetControlsManifest(ActionDO curAction);
        Task AuthenticateInternal(DockyardAccountDO user, PluginDO plugin, string username, string password);
        Task<ExternalAuthUrlDTO> GetExternalAuthUrl(DockyardAccountDO user, PluginDO plugin);
        Task AuthenticateExternal(PluginDO plugin, ExternalAuthenticationDTO externalAuthenticateDTO);
        
        void AddCrate(ActionDO curActionDO, CrateDTO curCrateDTO);
        void AddOrReplaceCrate(string label, ActionDO curActionDO, CrateDTO curCrateDTO);
        IEnumerable<JObject> FindKeysByCrateManifestType(ActionDO curActionDO, Manifest curSchema, string key);
    }
}