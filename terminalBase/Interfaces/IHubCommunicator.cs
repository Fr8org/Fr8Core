using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Models;

namespace TerminalBase.Infrastructure
{
    public interface IHubCommunicator
    {
        Task<PayloadDTO> GetPayload(Guid containerId, string userId);
        Task<UserDTO> GetCurrentUser(Guid containerId, string userId);
        Task<FieldDescriptionsCM> GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability, string userId);
        Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability, string userId);
        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction, string userId);
        Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction, string userId);

        Task CreateAlarm(AlarmDTO alarmDTO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, string userId);
        Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId);
        Task<AuthorizationTokenDTO> GetAuthToken(string authTokenId, string curFr8UserId);
        Task ScheduleEvent(string externalAccountId, string curFr8UserId, string minutes);
        Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload, string userId);
        Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload, string userId);
        Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string userId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null);
        Task<PlanDTO> CreatePlan(PlanEmptyDTO planDTO, string userId);
        Task RunPlan(Guid planId, List<CrateDTO> payload, string userId);
        Task<PlanDTO> ActivatePlan(PlanDTO planDO, string userId);
        Task<List<CrateDTO>> GetStoredManifests(string currentFr8UserId, List<CrateDTO> cratesForMTRequest);
        Task<IEnumerable<PlanDTO>> GetPlansByName(string name, string userId, PlanVisibility visibility = PlanVisibility.Standard);
        Task<FileDTO> SaveFile(string name, Stream stream, string userId);
        Task<Stream> DownloadFile(int fileId, string userId);
        Task<IEnumerable<FileDTO>> GetFiles(string userId);
        Task ApplyNewToken(Guid activityId, Guid authTokenId, string userId);
        Task Configure(string terminalName);
        bool IsConfigured { get; set; }
        Task DeletePlan(Guid planId, string userId);
        Task DeleteActivity(Guid curActivityId, string userId);
        Task DeleteExistingChildNodesFromActivity(Guid curActivityId, string userId);
        Task<PlanDTO> GetPlansByActivity(string activityId, string userId);
        Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan, string userId);
        Task NotifyUser(TerminalNotificationDTO notificationMessage, string userId);
    }
}
