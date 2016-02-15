using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Constants;
using Data.Interfaces.Manifests;

namespace TerminalBase.Infrastructure
{
    public interface IHubCommunicator
    {
        Task<PayloadDTO> GetPayload(ActivityDO activityDO, Guid containerId, string userId);
        Task<UserDTO> GetCurrentUser(ActivityDO activityDO, Guid containerId, string userId);
        Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability, string userId);
        Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability, string userId);
        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction, string userId);
        Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction, string userId);

        Task CreateAlarm(AlarmDTO alarmDTO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityDO activityDO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityDO activityDO, ActivityCategory category, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityDO activityDO, string tag, string userId);
        Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId);
        Task<ActivityDTO> ConfigureActivity(ActivityDTO activityDTO, string userId);
        Task<ActivityDO> ConfigureActivity(ActivityDO activityDO, string userId);
        Task<ActivityDTO> CreateAndConfigureActivity(int templateId, string userId, string label = null, int? order = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null);
        Task<RouteFullDTO> CreatePlan(RouteEmptyDTO planDTO, string userId);
        Task<PlanDO> ActivatePlan(PlanDO planDO, string userId);
        Task<IEnumerable<RouteFullDTO>> GetPlansByName(string name, string userId);
        Task<FileDO> SaveFile(string name, Stream stream, string userId);
        Task<Stream> DownloadFile(int fileId, string userId);
        Task<IEnumerable<FileDTO>> GetFiles(string userId);
        Task Configure(string terminalName);
        bool IsConfigured { get; set; }
    }
}
