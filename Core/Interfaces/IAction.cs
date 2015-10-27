using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Enums;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json.Linq;
using System;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        ActionDO SaveOrUpdateAction(ActionDO currentActionDo);
        ActionDO SaveOrUpdateAction(IUnitOfWork uow, ActionDO currentActionDo);
        Task<ActionDTO> Configure(ActionDO curActionDO);
        //Task<ActionDO> SaveUpdateAndConfigure(IUnitOfWork uow, ActionDO submittedActionDo);
        ActionDO GetById(int id);
        ActionDO GetById(IUnitOfWork uow, int id);
        //void Delete(int id); -> Delete is moved to ProcessNodeTemplate
        ActionDO MapFromDTO(ActionDTO curActionDTO);
        ActionDO Create(IUnitOfWork uow, int actionTemplateId, string name, string label, RouteNodeDO parentNode);
        Task<RouteNodeDO> CreateAndConfigure(IUnitOfWork uow, int actionTemplateId, string name, string label = null, int? parentNodeId = null, bool createRoute = false);
        
        Task PrepareToExecute(ActionDO curAction, ContainerDO curContainerDO, IUnitOfWork uow);
        Task<PayloadDTO> Run(ActionDO curActionDO, ContainerDO curContainerDO);
       // string Authenticate(ActionDO curActionDO);
        
        Task<ActionDTO> Activate(ActionDO curActionDO);
        Task<ActionDTO> Deactivate(ActionDO curActionDO);
		
        StandardConfigurationControlsCM GetControlsManifest(ActionDO curAction);
        //bool IsAuthenticated(Fr8AccountDO user, PluginDO plugin);
        //Task AuthenticateInternal(Fr8AccountDO user, PluginDO plugin, string username, string password);
        //Task<ExternalAuthUrlDTO> GetExternalAuthUrl(Fr8AccountDO user, PluginDO plugin);
        //Task AuthenticateExternal(PluginDO plugin, ExternalAuthenticationDTO externalAuthenticateDTO);
        

        Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActionDO, Manifest curSchema, string key, string fieldName = "name", GetCrateDirection direction = GetCrateDirection.None);
    }
}