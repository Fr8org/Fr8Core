using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hub.Managers;
using StructureMap;
using Data.Entities;
using Data.States;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Utilities.Configuration.Azure;
using Newtonsoft.Json;
using AutoMapper;
using System.Configuration;
using Fr8Data.Constants;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace TerminalBase.Infrastructure
{
    public class DefaultHubCommunicator : IHubCommunicator
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        protected string TerminalSecret { get; set; }
        protected string TerminalId { get; set; }

        private readonly IHMACService _hmacService;
        private readonly ICrateManager _crate;
        public bool IsConfigured { get; set; }

        public DefaultHubCommunicator()
        {
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
        }

        public Task Configure(string terminalName)
        {
            if (string.IsNullOrEmpty(terminalName))
                throw new ArgumentNullException("terminalName");

            TerminalSecret = CloudConfigurationManager.GetSetting("TerminalSecret");
            TerminalId = CloudConfigurationManager.GetSetting("TerminalId");

            //we might be on integration test currently
            if (TerminalSecret == null || TerminalId == null)
            {
                TerminalSecret = ConfigurationManager.AppSettings[terminalName + "TerminalSecret"];
                TerminalId = ConfigurationManager.AppSettings[terminalName + "TerminalId"];
            }

            IsConfigured = true;
            return Task.FromResult<object>(null);
        }

        #region HMAC

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, string userId)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Please call Configure() before using the class.");

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, string userId, T content)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Please call Configure() before using the class.");

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, string userId, HttpContent content)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Please call Configure() before using the class.");

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }

        #endregion

        public async Task<PayloadDTO> GetPayload(ActivityDO activityDO, Guid containerId, string userId)
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/containers/payload?id={containerId.ToString("D")}", UriKind.Absolute);
            var payloadDTOTask = await _restfulServiceClient.GetAsync<PayloadDTO>(uri, containerId.ToString(), await GetHMACHeader(uri, userId));
            return payloadDTOTask;
        }

        public async Task<UserDTO> GetCurrentUser(ActivityDO activityDO, Guid containerId, string userId)
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/users/userdata?id={userId}", UriKind.Absolute);
            var curUser = await _restfulServiceClient.GetAsync<UserDTO>(uri, containerId.ToString(), await GetHMACHeader(uri, userId));
            return curUser;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction, string userId)
        {
            var directionSuffix = direction == CrateDirection.Upstream
                ? "upstream"
                : "downstream";
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/plan_nodes?id={activityDO.Id}&direction={directionSuffix}", UriKind.Absolute);
            var curActivities = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null, await GetHMACHeader(uri, userId));
            var curCrates = new List<Crate<TManifest>>();

            foreach (var curAction in curActivities)
            {
                var storage = _crate.FromDto(curAction.CrateStorage);

                curCrates.AddRange(storage.CratesOfType<TManifest>());
            }

            return curCrates;
        }

        public async Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction, string userId)
        {
            var directionSuffix = direction == CrateDirection.Upstream
                ? "upstream"
                : "downstream";

            var uri = new Uri($"{GetHubUrlWithApiVersion()}/plan_nodes?id={activityDO.Id}&direction={directionSuffix}", UriKind.Absolute);
            var curActivities = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null, await GetHMACHeader(uri, userId));
            var curCrates = new List<Crate>();

            foreach (var curAction in curActivities)
            {
                var storage = _crate.FromDto(curAction.CrateStorage);
                curCrates.AddRange(storage);
            }

            return curCrates;
        }

        public async Task<IncomingCratesDTO> GetAvailableData(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plan_nodes/signals?id={activityDO.Id}&direction={(int)direction}&availability={(int)availability}";
            var uri = new Uri(url, UriKind.Absolute);
            var availableData = await _restfulServiceClient.GetAsync<IncomingCratesDTO>(uri, null, await GetHMACHeader(uri, userId));
            return availableData;
        }

        public async Task<FieldDescriptionsCM> GetDesignTimeFieldsByDirection(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            var mergedFields = new FieldDescriptionsCM();
            var availableData = await GetAvailableData(activityDO, direction, availability, userId);

            mergedFields.Fields.AddRange(availableData.AvailableCrates.SelectMany(x => x.Fields));

            return mergedFields;
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO, string userId)
        {
            var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/alarms";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, alarmDTO, null, await GetHMACHeader(uri, userId, alarmDTO));
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(string userId, bool getLatestsVersionsOnly = false)
        {
            var hubUri = new Uri($"{GetHubUrlWithApiVersion()}/activity_templates");
            var allCategories = await _restfulServiceClient.GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(hubUri, null, await GetHMACHeader(hubUri, userId));
            var templates = allCategories.SelectMany(x => x.Activities);
            return getLatestsVersionsOnly ? GetLatestsVersionsOnly(templates) : templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, string userId, bool getLatestsVersionsOnly = false)
        {
            var allTemplates = await GetActivityTemplates(userId, getLatestsVersionsOnly);
            var templates = allTemplates.Where(x => x.Category == category);
            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, string userId, bool getLatestsVersionsOnly = false)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/activity_templates?tag={(string.IsNullOrEmpty(tag) ? "[all]" : tag)}";
            var uri = new Uri(hubUrl);
            var templates = await _restfulServiceClient.GetAsync<List<ActivityTemplateDTO>>(uri, null, await GetHMACHeader(uri, userId));
            return getLatestsVersionsOnly ? GetLatestsVersionsOnly(templates) : templates;
        }

        private List<ActivityTemplateDTO> GetLatestsVersionsOnly(IEnumerable<ActivityTemplateDTO> templates)
        {
            if (templates == null)
            {
                return new List<ActivityTemplateDTO>(0);
            }
            var result = templates.GroupBy(x => new { x.Name, TerminalName = x.Terminal.Name })
                                  .Select(x => x.OrderByDescending(y => int.Parse(y.Version)).First())
                                  .ToList();
            return result;
        } 

        public async Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/field/exists";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<List<FieldValidationDTO>, List<FieldValidationResult>>(uri, fields, null, await GetHMACHeader(uri, userId, fields));
        }

        public async Task ApplyNewToken(Guid activityId, Guid authTokenId, string userId)
        {
            var applyToken = new ManageAuthToken_Apply()
            {
                ActivityId = activityId,
                AuthTokenId = authTokenId,
                IsMain = false
            };

            var token = new[] { applyToken };

            var url = $"{GetHubUrlWithApiVersion()}/ManageAuthToken/apply";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, token, null, await GetHMACHeader(uri, userId, token));
        }

        public async Task<ActivityDTO> ConfigureActivity(ActivityDTO activityDTO, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/configure";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO, null, await GetHMACHeader(uri, userId, activityDTO));
        }

        public async Task<ActivityDTO> SaveActivity(ActivityDTO activityDTO, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/save";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO, null, await GetHMACHeader(uri, userId, activityDTO));
        }

        public async Task<ActivityDO> SaveActivity(ActivityDO activityDO, string userId)
        {
            var activityDTO = Mapper.Map<ActivityDTO>(activityDO);
            return Mapper.Map<ActivityDO>(await SaveActivity(activityDTO, userId));
        }

        public async Task<ActivityDTO> CreateAndConfigureActivity(Guid templateId, string userId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/create";
            var postUrl = $"?activityTemplateId={templateId}&createPlan={createPlan}";
            if (name != null)
            {
                postUrl += "&name=" + name;
            }
            if (parentNodeId != null)
            {
                postUrl += "&parentNodeId=" + parentNodeId;
            }
            if (authorizationTokenId != null)
            {
                postUrl += "&authorizationTokenId=" + authorizationTokenId;
            }
            if (order != null)
            {
                postUrl += "&order=" + order;
            }

            var uri = new Uri(url + postUrl);
            return await _restfulServiceClient.PostAsync<ActivityDTO>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<ActivityDO> ConfigureActivity(ActivityDO activityDO, string userId)
        {
            var activityDTO = Mapper.Map<ActivityDTO>(activityDO);
            return Mapper.Map<ActivityDO>(await ConfigureActivity(activityDTO, userId));
        }

        public async Task<PlanDTO> CreatePlan(PlanEmptyDTO planDTO, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<PlanEmptyDTO, PlanDTO>(uri, planDTO, null, await GetHMACHeader(uri, userId, planDTO));
        }

        public async Task RunPlan(Guid planId, List<CrateDTO> payload, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans/runwithpayload?planId=" + planId;
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync<List<CrateDTO>>(uri, payload, null, await GetHMACHeader(uri, userId, payload));
        }
        
        public async Task<IEnumerable<PlanDTO>> GetPlansByName(string name, string userId, PlanVisibility visibility = PlanVisibility.Standard)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans/getbyname?name={name}&visibility={visibility}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<IEnumerable<PlanDTO>>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<PlanDTO> GetPlansByActivity(string activityId, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans/getByActivity?id={activityId}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<PlanDTO>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan, string userId)
        {
            var jsonObject = JsonConvert.SerializeObject(plan);
            HttpContent jsonContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");

            var hubUrl = $"{GetHubUrlWithApiVersion()}/plans/";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<PlanDTO>(uri, jsonContent, null, await GetHMACHeader(uri, userId, jsonContent));
        }

        public async Task NotifyUser(TerminalNotificationDTO notificationMessage, string userId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/notifications";
            var uri = new Uri(hubUrl);
            await _restfulServiceClient.PostAsync(uri, notificationMessage, null, await GetHMACHeader(uri, userId, notificationMessage));
        }

        public async Task DeletePlan(Guid planId, string userId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/plans?id={planId}";
            var uri = new Uri(hubUrl);
            await _restfulServiceClient.DeleteAsync(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task DeleteExistingChildNodesFromActivity(Guid curActivityId, string userId)
        {
            var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/activities/deletechildnodes?activityId={curActivityId}";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.DeleteAsync(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task DeleteActivity(Guid curActivityId, string userId)
        {
            var hubDeleteUrl = $"{GetHubUrlWithApiVersion()}/activities/deleteactivity?id={curActivityId}";
            var uri = new Uri(hubDeleteUrl);
            var headers = await GetHMACHeader(uri, userId);
            await _restfulServiceClient.DeleteAsync(uri, null, headers);
        }

        public static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task<FileDO> SaveFile(string name, Stream stream, string userId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files";
            var multiPartData = new MultipartFormDataContent();
            var byteData = ReadFully(stream);
            multiPartData.Add(new ByteArrayContent(byteData), name, name);
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<FileDO>(uri, multiPartData, null, await GetHMACHeader(uri, userId, (HttpContent)multiPartData));
        }

        public async Task<IEnumerable<FileDTO>> GetFiles(string userId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.GetAsync<IEnumerable<FileDTO>>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<Stream> DownloadFile(int fileId, string userId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files/{fileId}";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.DownloadAsync(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<List<CrateDTO>> GetStoredManifests(string currentFr8UserId, List<CrateDTO> cratesForMTRequest)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/warehouse?userId={currentFr8UserId}";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<List<CrateDTO>, List<CrateDTO>>(uri, cratesForMTRequest, null, await GetHMACHeader(uri, currentFr8UserId, cratesForMTRequest));
        }

        public async Task<AuthorizationTokenDTO> GetAuthToken(string externalAccountId, string curFr8UserId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/GetAuthToken?curFr8UserId={curFr8UserId}&externalAccountId={externalAccountId}&terminalId={TerminalId}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<AuthorizationTokenDTO>(uri, null, await GetHMACHeader(uri, curFr8UserId));
        }

        public async Task RenewToken(string id, string externalAccountId, string token, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/renewtoken?id={id}&externalAccountId={externalAccountId}&token={token}";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task RenewToken(AuthorizationTokenDTO token, string userId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/renewtoken";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, token, null, await GetHMACHeader(uri, userId, token));
        }

        public async Task ScheduleEvent(string externalAccountId, string curFr8UserId, string minutes)
        {
                      var hubAlarmsUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
               + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion")
               + string.Format("/alarms/polling?job_id={0}&fr8_account_id={1}&minutes={2}&terminal_id={3}",
               externalAccountId, curFr8UserId, minutes, TerminalId);
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, null, await GetHMACHeader(uri, curFr8UserId));
        }

        private static string GetHubUrlWithApiVersion()
        {
            return $"{CloudConfigurationManager.GetSetting("CoreWebServerUrl")}api/{CloudConfigurationManager.GetSetting("HubApiVersion")}";
        }
    }
}
