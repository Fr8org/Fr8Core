using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Constants;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;


namespace Hub.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        ActionDO SaveOrUpdateAction(ActionDO currentActionDo);
        ActionDO SaveOrUpdateAction(IUnitOfWork uow, ActionDO currentActionDo);
        Task<ActionDTO> Configure(string userId, ActionDO curActionDO, bool saveResult = true);
        //Task<ActionDO> SaveUpdateAndConfigure(IUnitOfWork uow, ActionDO submittedActionDo);
        ActionDO GetById(Guid id);
        ActionDO GetById(IUnitOfWork uow, Guid id);
        //void Delete(int id); -> Delete is moved to ProcessNodeTemplate
        ActionDO MapFromDTO(ActionDTO curActionDTO);
        ActionDO Create(IUnitOfWork uow, int actionTemplateId, string name, string label, RouteNodeDO parentNode, Guid? authorizationTokenId = null);

        Task<RouteNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, int actionTemplateId, string name,
                                             string label = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null);

        Task PrepareToExecute(ActionDO curAction, ActionState curActionState, ContainerDO curContainerDO, IUnitOfWork uow);
        Task<PayloadDTO> Run(ActionDO curActionDO, ActionState curActionState, ContainerDO curContainerDO);
       // string Authenticate(ActionDO curActionDO);
        
        Task<ActionDTO> Activate(ActionDO curActionDO);
        Task<ActionDTO> Deactivate(ActionDO curActionDO);
		
        StandardConfigurationControlsCM GetControlsManifest(ActionDO curAction);
        //bool IsAuthenticated(Fr8AccountDO user, PluginDO plugin);
        //Task AuthenticateInternal(Fr8AccountDO user, PluginDO plugin, string username, string password);
        //Task<ExternalAuthUrlDTO> GetExternalAuthUrl(Fr8AccountDO user, PluginDO plugin);
        //Task AuthenticateExternal(PluginDO plugin, ExternalAuthenticationDTO externalAuthenticateDTO);

        //Task<IEnumerable<T>> FindCratesByManifestType<T>(ActionDO curActionDO, GetCrateDirection direction = GetCrateDirection.None);


//        Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActionDO, Data.Interfaces.Manifests.Manifest curSchema, string key, string fieldName = "name", GetCrateDirection direction = GetCrateDirection.None);
    }
}