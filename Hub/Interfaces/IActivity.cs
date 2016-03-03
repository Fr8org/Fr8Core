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
    public interface IActivity
    {
        IEnumerable<TViewModel> GetAllActivities<TViewModel>();
        ActivityDO SaveOrUpdateActivity(IUnitOfWork uow, ActivityDO currentActivityDo);
        Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO, bool saveResult = true);
        //Task<ActionDO> SaveUpdateAndConfigure(IUnitOfWork uow, ActionDO submittedActionDo);
        ActivityDO GetById(IUnitOfWork uow, Guid id);
        //void Delete(int id); -> Delete is moved to ProcessNodeTemplate
        ActivityDO MapFromDTO(ActivityDTO curActivityDTO);

//      ActivityDO Create(IUnitOfWork uow, int actionTemplateId, string name, string label, int? order, PlanNodeDO parentNode, Guid? authorizationTokenId = null);

        Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, int actionTemplateId, 
                                             string label = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null);

        Task PrepareToExecute(ActivityDO curActivity, ActivityState curActionState, ContainerDO curContainerDO, IUnitOfWork uow);
        Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityState curActionState, ContainerDO curContainerDO);
       // string Authenticate(ActionDO curActivityDO);
        
        Task<ActivityDTO> Activate(ActivityDO curActivityDO);
        Task<ActivityDTO> Deactivate(ActivityDO curActivityDO);
        StandardConfigurationControlsCM GetControlsManifest(ActivityDO curActivity);
        Task<T> GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution = false);
        List<string> GetSolutionList(string terminalName);
        void Delete(Guid id);
        //bool IsAuthenticated(Fr8AccountDO user, PluginDO plugin);
        //Task AuthenticateInternal(Fr8AccountDO user, PluginDO plugin, string username, string password);
        //Task<ExternalAuthUrlDTO> GetExternalAuthUrl(Fr8AccountDO user, PluginDO plugin);
        //Task AuthenticateExternal(PluginDO plugin, ExternalAuthenticationDTO externalAuthenticateDTO);

        //Task<IEnumerable<T>> FindCratesByManifestType<T>(ActionDO curActivityDO, GetCrateDirection direction = GetCrateDirection.None);


//        Task<IEnumerable<JObject>> FindKeysByCrateManifestType(ActionDO curActivityDO, Data.Interfaces.Manifests.Manifest curSchema, string key, string fieldName = "name", GetCrateDirection direction = GetCrateDirection.None);
    }
}