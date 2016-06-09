using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Fr8Data.States;
using Fr8Infrastructure.Interfaces;
using Newtonsoft.Json;
using StructureMap;
using TerminalBase.Infrastructure;
using TerminalBase.Models;
using Utilities.Configuration.Azure;

namespace TerminalBase.Services
{
    public abstract class DataHubCommunicatorBase : IHubCommunicator
    {
        public ICrateManager Crate { get; set; }
        public string ExplicitData { get; set; }
        protected string TerminalSecret { get; set; }
        protected string TerminalId { get; set; }

        private string _userId;

        public string UserId => _userId;

        protected DataHubCommunicatorBase(string explicitData)
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
            ExplicitData = explicitData;
        }
        
        public void Configure(string terminalName, string userId)
        {
            if (string.IsNullOrEmpty(terminalName))
                throw new ArgumentNullException(nameof(terminalName));

            TerminalSecret = CloudConfigurationManager.GetSetting("TerminalSecret");
            TerminalId = CloudConfigurationManager.GetSetting("TerminalId");

            //we might be on integration test currently
            if (TerminalSecret == null || TerminalId == null)
            {
                TerminalSecret = ConfigurationManager.AppSettings[terminalName + "TerminalSecret"];
                TerminalId = ConfigurationManager.AppSettings[terminalName + "TerminalId"];
            }
            _userId = userId;
            IsConfigured = true;
        }

        protected abstract string LabelPrefix { get; }

        public bool IsConfigured
        {
            get; set;
        }

        private void StripLabelPrefix(IEnumerable<Crate> crates, string prefix)
        {
            foreach (var crate in crates)
            {
                if (crate.Label != prefix && crate.Label.StartsWith(prefix + "_"))
                {
                    crate.Label = crate.Label.Substring((prefix + "_").Length);
                }
            }
        }

        public Task<PayloadDTO> GetPayload(Guid containerId)
        {
            var payload = new PayloadDTO(containerId)
            {
                CrateStorage = new CrateStorageDTO()
            };

            var crateStorage = Crate.GetStorage(ExplicitData);
            using (var updatableStorage = Crate.GetUpdatableStorage(payload))
            {
                var crates = crateStorage
                    .Where(x => x.Label.StartsWith(LabelPrefix + "_PayloadCrate"))
                    .ToList();

                StripLabelPrefix(crates, LabelPrefix + "_PayloadCrate");

                updatableStorage.AddRange(crates);
            }

            return Task.FromResult(payload);
        }

        public Task<UserDTO> GetCurrentUser()
        {
            return Task.FromResult<UserDTO>(
                new UserDTO()
                {
                    EmailAddress = "integration_test_runner@fr8.company",
                    FirstName = "Test",
                    LastName = "User",
                    UserName = "integration_test_runner@fr8.company"
                }
            );
        }

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction)
        {
            var searchLabel = direction == CrateDirection.Upstream
                ? LabelPrefix + "_UpstreamCrate"
                : LabelPrefix + "_DownstreamCrate";

            var crateStorage = Crate.GetStorage(ExplicitData);
            var crates = crateStorage
                .CratesOfType<TManifest>(x => x.Label.StartsWith(searchLabel))
                .ToList();

            StripLabelPrefix(crates, searchLabel);

            return Task.FromResult(crates);
        }

        public Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction)
        {
            var searchLabel = direction == CrateDirection.Upstream
                ? LabelPrefix + "_UpstreamCrate"
                : LabelPrefix + "_DownstreamCrate";

            var crateStorage = Crate.GetStorage(ExplicitData);
            var crates = crateStorage
                .Where(x => x.Label.StartsWith(searchLabel))
                .ToList();

            StripLabelPrefix(crates, searchLabel);

            return Task.FromResult(crates);
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO)
        {

        }

        public Task<FileDTO> SaveFile(string name, Stream stream)
        {
            var fileDO = new FileDTO
            {
                OriginalFileName = name,
            };

            return Task.FromResult(fileDO);
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(bool getLatestsVersionsOnly = false)
        {
            var searchLabel = LabelPrefix + "_ActivityTemplate";

            var crateStorage = Crate.GetStorage(ExplicitData);
            var activityTemplates = crateStorage
                .Where(x => x.Label == searchLabel)
                .Select(x => JsonConvert.DeserializeObject<ActivityTemplateDTO>(
                    x.Get<FieldDescriptionsCM>().Fields[0].Value
                    )
                )
                .ToList();

            return Task.FromResult(activityTemplates);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, bool getLatestsVersionsOnly = false)
        {
            var allTemplates = await GetActivityTemplates();
            var activityTemplates = allTemplates
                .Where(x => x.Category == category)
                .ToList();

            return activityTemplates;
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, bool getLatestsVersionsOnly = false)
        {
            var allTemplates = await GetActivityTemplates();
            if (string.IsNullOrEmpty(tag))
            {
                return allTemplates;
            }

            var activityTemplates = allTemplates
                .Where(x => x.Tags != null && x.Tags.Split(',').Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            return activityTemplates;
        }

        public Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields)
        {
            return Task.FromResult(new List<FieldValidationResult>());
        }
        
        public async Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            var fields = await GetCratesByDirection<FieldDescriptionsCM>(activityId, direction);
            var crates = await GetCratesByDirection<CrateDescriptionCM>(activityId, direction);
            var availableData = new IncomingCratesDTO();

            availableData.AvailableFields.AddRange(fields.SelectMany(x => x.Content.Fields).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0));
            availableData.AvailableFields.AddRange(crates.SelectMany(x => x.Content.CrateDescriptions).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0).SelectMany(x => x.Fields));
            availableData.AvailableCrates.AddRange(crates.SelectMany(x => x.Content.CrateDescriptions).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0));

            return availableData;
        }

        public async Task ApplyNewToken(Guid activityId, Guid authTokenId)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = default(Guid?), bool createPlan = false, Guid? authorizationTokenId = null)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDTO> CreatePlan(PlanNoChildrenDTO planDTO)
        {
            throw new NotImplementedException();
        }

        public Task RunPlan(Guid planId, List<CrateDTO> payload)
        {
            throw new NotImplementedException();
        }
        
        public Task<IEnumerable<PlanDTO>> GetPlansByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDTO> GetPlansByActivity(string activityId)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDTO> UpdatePlan(PlanNoChildrenDTO plan)
        {
            throw new NotImplementedException();
        }

        public Task NotifyUser(TerminalNotificationDTO notificationMessage)
        {
            return Task.FromResult(0);
        }

        public Task RenewToken(AuthorizationTokenDTO token)
        {
            return Task.FromResult(0);
        }

        public Task RenewToken(string id, string externalAccountId, string token)
        {
            throw new NotImplementedException();
        }

        public Task DeletePlan(Guid planId)
        {
            throw new NotImplementedException();
        }
        
        public async Task<Stream> DownloadFile(string filePath)
        {
            throw new NotImplementedException();
        }
        
        public Task<IEnumerable<FileDTO>> GetFiles()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadFile(int fileId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteExistingChildNodesFromActivity(Guid curActivityId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteActivity(Guid curActivityId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CrateDTO>> GetStoredManifests(List<CrateDTO> cratesForMTRequest)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationToken> GetAuthToken(string authTokenId)
        {
            throw new NotImplementedException();
        }

        public Task ScheduleEvent(string externalAccountId, string minutes)
        {
            throw new NotImplementedException();
        }
    }
}
