using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Services
{
    class DelayedHubCommunicator : IHubCommunicator
    {
        private IHubCommunicator _underlyingHubCommunicator;
        private readonly object _sync = new object();
        private readonly AsyncMultiLock _lock = new AsyncMultiLock();
        private readonly Task<IHubCommunicator> _resolveHubCommunicatorTask;
        private string _userId;

        public string UserId => _userId;

        public DelayedHubCommunicator(Task<IHubCommunicator> resolveHubCommunicatorTask)
        {
            _resolveHubCommunicatorTask = resolveHubCommunicatorTask;
        }

        public void Authorize(string userId)
        {
            lock (_sync)
            {
                _userId = userId;
                _underlyingHubCommunicator?.Authorize(userId);
            }
        }

        public async Task<PlanEmptyDTO> LoadPlan(JToken planContents)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.LoadPlan(planContents);
        }

        public async Task<PayloadDTO> GetPayload(Guid containerId)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetPayload(containerId);
        }

        public async Task<UserDTO> GetCurrentUser()
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetCurrentUser();
        } 

        public async Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetAvailableData(activityId, direction, availability);
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetCratesByDirection<TManifest>(activityId, direction);
        }

        public async Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetCratesByDirection(activityId, direction);
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.CreateAlarm(alarmDTO);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(bool getLatestsVersionsOnly = false)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetActivityTemplates(getLatestsVersionsOnly);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, bool getLatestsVersionsOnly = false)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetActivityTemplates(category, getLatestsVersionsOnly);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, bool getLatestsVersionsOnly = false)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetActivityTemplates(tag, getLatestsVersionsOnly);
        }

        //public async Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields)
        //{
        //    await InitializeUnderlyingCommunicator();
        //    return await _underlyingHubCommunicator.ValidateFields(fields);
        //}

        public async Task ScheduleEvent(string externalAccountId, string minutes, bool triggerImmediately = false, string additionalConfigAttributes = null)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.ScheduleEvent(externalAccountId, minutes, triggerImmediately, additionalConfigAttributes);
        }

        public async Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload, bool force)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.ConfigureActivity(activityPayload, force);
        }

        public async Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload, bool force)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.SaveActivity(activityPayload, force);
        }

        public async Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.CreateAndConfigureActivity(templateId, name, order, parentNodeId, createPlan, authorizationTokenId);
        }

        public async Task<PlanDTO> CreatePlan(PlanEmptyDTO planDTO)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.CreatePlan(planDTO);
        }

        public async Task RunPlan(Guid planId, List<CrateDTO> payload)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.RunPlan(planId, payload);
        }

        public async Task<List<CrateDTO>> GetStoredManifests(List<CrateDTO> cratesForMTRequest)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetStoredManifests(cratesForMTRequest);
        }

        public async Task<IEnumerable<PlanDTO>> GetPlansByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetPlansByName(name, visibility);
        }

        public async Task<FileDTO> SaveFile(string name, Stream stream)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.SaveFile(name, stream);
        }

        public async Task<Stream> DownloadFile(int fileId)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.DownloadFile(fileId);
        }

        public async Task<IEnumerable<FileDTO>> GetFiles()
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetFiles();
        }

        public async Task ApplyNewToken(Guid activityId, Guid authTokenId)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.ApplyNewToken(activityId, authTokenId);
        }

        public async Task DeletePlan(Guid planId)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.DeletePlan(planId);
        }

        public async Task DeleteActivity(Guid curActivityId)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.DeleteActivity(curActivityId);
        }

        public async Task DeleteExistingChildNodesFromActivity(Guid curActivityId)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.DeleteExistingChildNodesFromActivity(curActivityId);
        }

        public async Task<PlanDTO> GetPlansByActivity(string activityId)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetPlansByActivity(activityId);
        }

        public async Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.UpdatePlan(plan);
        }

        public async Task NotifyUser(TerminalNotificationDTO notificationMessage)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.NotifyUser(notificationMessage);
        }

        public async Task RenewToken(AuthorizationTokenDTO token)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.RenewToken(token);
        }

        public async Task SendEvent(Crate eventPayload)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.SendEvent(eventPayload);
        }

        public async Task<List<TManifest>> QueryWarehouse<TManifest>(List<FilterConditionDTO> query)
            where TManifest : Manifest
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.QueryWarehouse<TManifest>(query);
        }

        public async Task AddOrUpdateWarehouse(params Manifest[] manifests)
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.AddOrUpdateWarehouse(manifests);
        }

        public async Task DeleteFromWarehouse<TManifest>(List<FilterConditionDTO> query)
            where TManifest : Manifest
        {
            await InitializeUnderlyingCommunicator();
            await _underlyingHubCommunicator.DeleteFromWarehouse<TManifest>(query);
        }

        private async Task InitializeUnderlyingCommunicator()
        {
            using (await _lock.Lock(_sync))
            {
                if (_underlyingHubCommunicator == null)
                {
                    var result = await _resolveHubCommunicatorTask;

                    lock (_sync)
                    {
                        _underlyingHubCommunicator = result;
                        _underlyingHubCommunicator.Authorize(UserId);
                    }
                }
            }
        }

        public async Task<List<AuthenticationTokenTerminalDTO>> GetTokens()
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetTokens();
        }

        public async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri)
        {
            await InitializeUnderlyingCommunicator();
            return await _underlyingHubCommunicator.GetHMACHeader(requestUri);
        }
    }
}
