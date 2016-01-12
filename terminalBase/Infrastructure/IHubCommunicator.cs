using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Constants;

namespace TerminalBase.Infrastructure
{
    public interface IHubCommunicator
    {
        Task<PayloadDTO> GetPayload(ActionDO actionDO, Guid containerId, string userId);

        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActionDO actionDO, CrateDirection direction, string userId);

        Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction, string userId);

        Task CreateAlarm(AlarmDTO alarmDTO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, ActivityCategory category, string userId);

        Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string tag, string userId);
        Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId);
    }
}
