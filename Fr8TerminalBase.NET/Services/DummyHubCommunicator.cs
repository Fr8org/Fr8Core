using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json.Linq;

namespace Fr8.TerminalBase.Services
{
    public class DummyHubCommunicator : IHubCommunicator
    {

        public Task<PayloadDTO> GetPayload(Guid containerId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<AuthenticationTokenTerminalDTO>> GetTokens()
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<UserDTO> GetCurrentUser()
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task CreateAlarm(AlarmDTO alarmDTO)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(bool getLatestsVersionsOnly = false)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(Guid category, bool getLatestsVersionsOnly = false)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, bool getLatestsVersionsOnly = false)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task ScheduleEvent(string externalAccountId, string minutes, bool triggerImmediately = false,
            string additionalConfigAttributes = null, string additionToJobId = null)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload, bool force = false)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload, bool force = false)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = null,
            bool createPlan = false, Guid? authorizationTokenId = null)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<PlanDTO> CreatePlan(PlanNoChildrenDTO planDTO)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task RunPlan(Guid planId, IEnumerable<Crate> payload)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<CrateDTO>> GetStoredManifests(List<CrateDTO> cratesForMTRequest)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<IEnumerable<PlanDTO>> GetPlansByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<FileDTO> SaveFile(string name, Stream stream)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<Stream> DownloadFile(int fileId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<IEnumerable<FileDTO>> GetFiles()
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task ApplyNewToken(Guid activityId, Guid authTokenId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task DeletePlan(Guid planId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task DeleteActivity(Guid curActivityId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task DeleteExistingChildNodesFromActivity(Guid curActivityId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<PlanDTO> GetPlansByActivity(string activityId)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<PlanDTO> UpdatePlan(PlanNoChildrenDTO plan)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task NotifyUser(NotificationMessageDTO notificationMessage)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task RenewToken(AuthorizationTokenDTO token)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task SendEvent(Crate eventPayload)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task<List<TManifest>> QueryWarehouse<TManifest>(List<FilterConditionDTO> query) where TManifest : Manifest
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task AddOrUpdateWarehouse(params Manifest[] manifests)
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }

        public Task DeleteFromWarehouse<TManifest>(List<FilterConditionDTO> query) where TManifest : Manifest
        {
            throw new NotImplementedException("Terminals can't communicate with an unknown hub");
        }
    }
}
