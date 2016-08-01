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
        string UserId { get; }

        void Configure(string terminalName, string userId);

        Task<PayloadDTO> GetPayload(Guid containerId);
        Task<UserDTO> GetCurrentUser();
        Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability);
        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction);
        Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction);
        Task CreateAlarm(AlarmDTO alarmDTO);
        Task<List<ActivityTemplateDTO>> GetActivityTemplates(bool getLatestsVersionsOnly = false);
        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, bool getLatestsVersionsOnly = false);
        Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, bool getLatestsVersionsOnly = false);
        Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields);
        Task<AuthorizationToken> GetAuthToken(string authTokenId);
        Task ScheduleEvent(string externalAccountId, string minutes);
        Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload);
        Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload);
        Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null);
        Task<PlanDTO> CreatePlan(PlanNoChildrenDTO planDTO);
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
        Task<PlanDTO> UpdatePlan(PlanNoChildrenDTO plan);
        Task NotifyUser(TerminalNotificationDTO notificationMessage);
        Task RenewToken(AuthorizationTokenDTO token);
    }
}
