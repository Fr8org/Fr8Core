using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Data.Constants;
using System.IO;
using Data.Interfaces;

namespace TerminalBase.Infrastructure
{
    public abstract class DataHubCommunicatorBase : IHubCommunicator
    {
        public ICrateManager Crate { get; set; }
        public string ExplicitData { get; set; }

        protected DataHubCommunicatorBase(string explicitData)
        {
            Crate = ObjectFactory.GetInstance<ICrateManager>();
            this.ExplicitData = explicitData;
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

        public virtual Task Configure(string terminalName)
        {
            return Task.FromResult<object>(null);
        }

        public Task<PayloadDTO> GetPayload(ActivityDO activityDO, Guid containerId, string userId)
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

        public Task<UserDTO> GetCurrentUser(ActivityDO activityDO, Guid containerId, string userId)
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

        public Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction, string userId)
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

        public Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction, string userId)
        {
            var searchLabel = direction == CrateDirection.Upstream
                ? LabelPrefix + "_UpstreamCrate"
                : LabelPrefix + "_DownstreamCrate";

            var crateStorage = Crate.GetStorage(activityDO);
            var crates = crateStorage
                .Where(x => x.Label.StartsWith(searchLabel))
                .ToList();

            StripLabelPrefix(crates, searchLabel);

            return Task.FromResult(crates);
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO, string userId)
        {

        }

        public Task<FileDO> SaveFile(string name, Stream stream, string userId)
        {
            var fileDO = new FileDO
            {
                OriginalFileName = name,
                CreateDate = DateTime.Now,
                Id = 0,
                LastUpdated = DateTime.Now
            };

            return Task.FromResult(fileDO);
        }

        public Task<List<ActivityTemplateDTO>> GetActivityTemplates(string userId)
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

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, string userId)
        {
            var allTemplates = await GetActivityTemplates(userId);
            var activityTemplates = allTemplates
                .Where(x => x.Category == category)
                .ToList();

            return activityTemplates;
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, string userId)
        {
            var allTemplates = await GetActivityTemplates(userId);
            if (string.IsNullOrEmpty(tag))
            {
                return allTemplates;
            }

            var activityTemplates = allTemplates
                .Where(x => x.Tags != null && x.Tags.Split(',').Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            return activityTemplates;
        }

        public Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId)
        {
            return Task.FromResult(new List<FieldValidationResult>());
        }

        public async Task<FieldDescriptionsCM> GetDesignTimeFieldsByDirection(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            //This code only supports integration testing scenarios

            var mergedFields = new FieldDescriptionsCM();
            var availableData = await GetAvailableData(activityDO, direction, availability, userId);

            mergedFields.Fields.AddRange(availableData.IncomingFields);

            return mergedFields;
        }

        public async Task<IncomingCratesDTO> GetAvailableData(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            var fields = await GetCratesByDirection<FieldDescriptionsCM>(activityDO, direction,  userId);
            var crates = await GetCratesByDirection<CrateDescriptionCM>(activityDO, direction, userId);
            var availableData = new IncomingCratesDTO();

            availableData.IncomingFields.AddRange(fields.SelectMany(x => x.Content.Fields).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0));
            availableData.IncomingFields.AddRange(crates.SelectMany(x => x.Content.CrateDescriptions).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0).SelectMany(x => x.Fields));
            availableData.IncomingCrates.AddRange(crates.SelectMany(x => x.Content.CrateDescriptions).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0));

            return availableData;
        }

        public async Task ApplyNewToken(Guid activityId, Guid authTokenId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityDTO> ConfigureActivity(ActivityDTO activityDTO, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityDO> SaveActivity(ActivityDO activityDO, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityDTO> CreateAndConfigureActivity(Guid templateId, string userId, string label = null, int? order = null, Guid? parentNodeId = default(Guid?), bool createPlan = false, Guid? authorizationTokenId = null)
        {
            throw new NotImplementedException();
        }

        public Task<ActivityDO> ConfigureActivity(ActivityDO activityDO, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDTO> CreatePlan(PlanEmptyDTO planDTO, string userId)
        {
            throw new NotImplementedException();
        }

        public Task RunPlan(Guid planId, List<CrateDTO> payload, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDO> ActivatePlan(PlanDO planDO, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PlanDTO>> GetPlansByName(string name, string userId, PlanVisibility visibility = PlanVisibility.Standard)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDTO> GetPlansByActivity(string activityId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan, string userId)
        {
            throw new NotImplementedException();
        }

        public Task NotifyUser(TerminalNotificationDTO notificationMessage, string userId)
        {
            return Task.FromResult(0);
        }

        public Task DeletePlan(Guid planId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FileDTO>> GetFiles(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadFile(int fileId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteExistingChildNodesFromActivity(Guid curActivityId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteActivity(Guid curActivityId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<CrateDTO>> GetStoredManifests(string currentFr8UserId, List<CrateDTO> cratesForMTRequest)
        {
            throw new NotImplementedException();
        }

        public Task<AuthorizationTokenDTO> GetAuthToken(string authTokenId, string curFr8UserId)
        {
            throw new NotImplementedException();
        }
    }
}
