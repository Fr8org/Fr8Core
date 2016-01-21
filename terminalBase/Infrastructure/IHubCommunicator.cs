using System;
using System.Collections.Generic;
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
        Task<PayloadDTO> GetPayload(ActionDO actionDO, Guid containerId, string userId);
        Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability, string userId);
        Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(ActionDO actionDO, CrateDirection direction, AvailabilityType availability, string userId);
        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActionDO actionDO, CrateDirection direction, string userId);
        Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction, string userId);

        Task CreateAlarm(AlarmDTO alarmDTO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, ActivityCategory category, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string tag, string userId);
        Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId);
        Task<ActionDTO> ConfigureAction(ActionDTO actionDTO, string userId);
        Task<ActionDO> ConfigureAction(ActionDO actionDO, string userId);
        Task<ActionDTO> CreateAndConfigureAction(int templateId, string name, string userId, string label = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null);
        Task<RouteEmptyDTO> CreateRoute(RouteEmptyDTO routeDTO, string userId);
    }
}
