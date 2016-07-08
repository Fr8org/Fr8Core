using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Interfaces
{
    public interface IHubCommunicator
    {
        string UserId { get; }

        void Authorize(string userId);

        Task<PlanEmptyDTO> LoadPlan(JToken planContents);
        Task<PayloadDTO> GetPayload(Guid containerId);
        Task<List<AuthenticationTokenTerminalDTO>> GetTokens();
        Task<UserDTO> GetCurrentUser();
        Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability);
        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction);
        Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction);
        Task CreateAlarm(AlarmDTO alarmDTO);
        Task<List<ActivityTemplateDTO>> GetActivityTemplates(bool getLatestsVersionsOnly = false);
        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, bool getLatestsVersionsOnly = false);
        Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, bool getLatestsVersionsOnly = false);
        //Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields);
        Task ScheduleEvent(string externalAccountId, string minutes, bool triggerImmediately = false, string additionalConfigAttributes = null);
        Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload, bool force = false); // force flag is used to save or configure activity even if plan is in Running state. 
        Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload, bool force = false);  // force flag is used to save or configure activity even if plan is in Running state. 
        Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null);
        Task<PlanDTO> CreatePlan(PlanEmptyDTO planDTO);
        Task RunPlan(Guid planId, List<CrateDTO> payload);
        Task<List<CrateDTO>> GetStoredManifests(List<CrateDTO> cratesForMTRequest);
        Task<IEnumerable<PlanDTO>> GetPlansByName(string name, PlanVisibility visibility = PlanVisibility.Standard);
        Task<FileDTO> SaveFile(string name, Stream stream);
        Task<Stream> DownloadFile(int fileId);
        Task<IEnumerable<FileDTO>> GetFiles();
        Task ApplyNewToken(Guid activityId, Guid authTokenId);
        Task DeletePlan(Guid planId);
        Task DeleteActivity(Guid curActivityId);
        Task DeleteExistingChildNodesFromActivity(Guid curActivityId);
        Task<PlanDTO> GetPlansByActivity(string activityId);
        Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan);
        Task NotifyUser(TerminalNotificationDTO notificationMessage);
        Task RenewToken(AuthorizationTokenDTO token);
        Task SendEvent(Crate eventPayload);
        Task<List<TManifest>> QueryWarehouse<TManifest>(List<FilterConditionDTO> query) where TManifest : Manifest;
        Task AddOrUpdateWarehouse(params Manifest[] manifests);
        Task DeleteFromWarehouse<TManifest>(List<FilterConditionDTO> query) where TManifest : Manifest;
        Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri);
    }
}
