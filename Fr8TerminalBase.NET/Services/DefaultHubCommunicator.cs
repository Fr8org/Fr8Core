using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.TerminalBase.Services
{
    public class DefaultHubCommunicator : IHubCommunicator
    {
        protected readonly IRestfulServiceClient _restfulServiceClient;
        private readonly string _apiUrl;
        private string _userId;
        protected string TerminalToken { get; set; }
        public string UserId => _userId;

        public DefaultHubCommunicator(IRestfulServiceClient restfulServiceClient, string apiUrl, string token, string userId)
        {
            TerminalToken = token;
            _restfulServiceClient = restfulServiceClient;
            _apiUrl = apiUrl?.TrimEnd('/', '\\');
            _userId = userId;
            _restfulServiceClient.AddRequestSignature(new HubAuthenticationHeaderSignature(TerminalToken, userId));
        }

        public async Task<PlanEmptyDTO> LoadPlan(JToken planContents)
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/plans/load");
            return await _restfulServiceClient.PostAsync<JToken, PlanEmptyDTO>(uri, planContents);
        }

        public async Task<PayloadDTO> GetPayload(Guid containerId)
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/containers/payload?id={containerId.ToString("D")}", UriKind.Absolute);
            var payloadDTOTask = await _restfulServiceClient.GetAsync<PayloadDTO>(uri, containerId.ToString());
            return payloadDTOTask;
        }

        public async Task<UserDTO> GetCurrentUser()
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/users/userdata?id={_userId}", UriKind.Absolute);
            var curUser = await _restfulServiceClient.GetAsync<UserDTO>(uri, null);
            return curUser;
        }

        public async Task<List<AuthenticationTokenTerminalDTO>> GetTokens()
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/authentication/tokens", UriKind.Absolute);
            var tokens = await _restfulServiceClient.GetAsync<List<AuthenticationTokenTerminalDTO>>(uri, null);
            return tokens;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction)
        {
            var directionSuffix = direction == CrateDirection.Upstream
                ? "upstream"
                : "downstream";
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/plan_nodes?id={activityId}&direction={directionSuffix}", UriKind.Absolute);
            var curActivities = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null);
            var curCrates = new List<Crate<TManifest>>();

            foreach (var curAction in curActivities)
            {
                var storage = CrateStorageSerializer.Default.ConvertFromDto(curAction.CrateStorage);

                curCrates.AddRange(storage.CratesOfType<TManifest>());
            }

            return curCrates;
        }

        public async Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction)
        {
            var directionSuffix = direction == CrateDirection.Upstream
                ? "upstream"
                : "downstream";

            var uri = new Uri($"{GetHubUrlWithApiVersion()}/plan_nodes?id={activityId}&direction={directionSuffix}", UriKind.Absolute);
            var curActivities = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null);
            var curCrates = new List<Crate>();

            foreach (var curAction in curActivities)
            {
                var storage = CrateStorageSerializer.Default.ConvertFromDto(curAction.CrateStorage);
                curCrates.AddRange(storage);
            }

            return curCrates;
        }

        public async Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plan_nodes/signals?id={activityId}&direction={(int)direction}&availability={(int)availability}";
            var uri = new Uri(url, UriKind.Absolute);
            var availableData = await _restfulServiceClient.GetAsync<IncomingCratesDTO>(uri, null);
            return availableData;
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO)
        {
            var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/alarms";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, alarmDTO, null);
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(bool getLatestsVersionsOnly = false)
        {
            var hubUri = new Uri($"{GetHubUrlWithApiVersion()}/activity_templates");
            var allCategories = await _restfulServiceClient.GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(hubUri);
            var templates = allCategories.SelectMany(x => x.Activities);
            return getLatestsVersionsOnly ? GetLatestsVersionsOnly(templates) : templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityCategory category, bool getLatestsVersionsOnly = false)
        {
            var allTemplates = await GetActivityTemplates(getLatestsVersionsOnly);
            var templates = allTemplates.Where(x => x.Category == category);
            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(string tag, bool getLatestsVersionsOnly = false)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/activity_templates?tag={(string.IsNullOrEmpty(tag) ? "[all]" : tag)}";
            var uri = new Uri(hubUrl);
            var templates = await _restfulServiceClient.GetAsync<List<ActivityTemplateDTO>>(uri);
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

        public async Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields)
        {
            var url = $"{GetHubUrlWithApiVersion()}/field/exists";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<List<FieldValidationDTO>, List<FieldValidationResult>>(uri, fields);
        }

        public async Task ApplyNewToken(Guid activityId, Guid authTokenId)
        {
            var applyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = activityId,
                AuthTokenId = authTokenId,
                IsMain = false
            };

            var token = new[] { applyToken };

            var url = $"{GetHubUrlWithApiVersion()}/authentication/granttokens";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, token);
        }

        public async Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload, bool force)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/configure?force=" + force;
            var uri = new Uri(url);
            var activityDTO = Mapper.Map<ActivityDTO>(activityPayload);
            var resultActivityDTO = await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO);
            return Mapper.Map<ActivityPayload>(resultActivityDTO);
        }

        public async Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload, bool force)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/save?force=" + force;
            var uri = new Uri(url);
            var activityDTO = Mapper.Map<ActivityDTO>(activityPayload);
            var resultActivityDTO = await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO);
            return Mapper.Map<ActivityPayload>(resultActivityDTO);
        }

        public async Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/create";
            var postUrl = $"?activityTemplateId={templateId}&createPlan={createPlan}";
            if (name != null)
            {
                postUrl += "&name=" + HttpUtility.UrlEncode(name);
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
            var resultActivityDTO = await _restfulServiceClient.PostAsync<ActivityDTO>(uri);
            return Mapper.Map<ActivityPayload>(resultActivityDTO);
        }

        public async Task<PlanDTO> CreatePlan(PlanEmptyDTO planDTO)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<PlanEmptyDTO, PlanDTO>(uri, planDTO);
        }

        public async Task RunPlan(Guid planId, List<CrateDTO> payload)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans/run?planId=" + planId;
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync<List<CrateDTO>>(uri, payload);
        }

        public async Task<IEnumerable<PlanDTO>> GetPlansByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans?name={name}&visibility={visibility}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<IEnumerable<PlanDTO>>(uri);
        }

        public async Task<PlanDTO> GetPlansByActivity(string activityId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans?id={activityId}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<PlanDTO>(uri);
        }

        public async Task<PlanDTO> UpdatePlan(PlanEmptyDTO plan)
        {
            var jsonObject = JsonConvert.SerializeObject(plan);
            HttpContent jsonContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");

            var hubUrl = $"{GetHubUrlWithApiVersion()}/plans/";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<PlanDTO>(uri, jsonContent);
        }

        public async Task NotifyUser(TerminalNotificationDTO notificationMessage)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/notifications";
            var uri = new Uri(hubUrl);
            await _restfulServiceClient.PostAsync(uri, notificationMessage);
        }

        public async Task DeletePlan(Guid planId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/plans?id={planId}";
            var uri = new Uri(hubUrl);
            await _restfulServiceClient.DeleteAsync(uri);
        }

        public async Task DeleteExistingChildNodesFromActivity(Guid curActivityId)
        {
            var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/activities?id={curActivityId}&delete_child_nodes=true";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.DeleteAsync(uri);
        }

        public async Task DeleteActivity(Guid curActivityId)
        {
            var hubDeleteUrl = $"{GetHubUrlWithApiVersion()}/activities?id={curActivityId}";
            var uri = new Uri(hubDeleteUrl);
            await _restfulServiceClient.DeleteAsync(uri);
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

        public async Task<FileDTO> SaveFile(string name, Stream stream)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files";
            var multiPartData = new MultipartFormDataContent();
            var byteData = ReadFully(stream);
            multiPartData.Add(new ByteArrayContent(byteData), name, name);
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<FileDTO>(uri, multiPartData);
        }

        public async Task<IEnumerable<FileDTO>> GetFiles()
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.GetAsync<IEnumerable<FileDTO>>(uri);
        }

        public async Task<Stream> DownloadFile(int fileId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files/{fileId}";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.DownloadAsync(uri);
        }

        public async Task<List<CrateDTO>> GetStoredManifests(List<CrateDTO> cratesForMTRequest)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/warehouse?userId={_userId}";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<List<CrateDTO>, List<CrateDTO>>(uri, cratesForMTRequest);
        }

        public async Task<AuthorizationToken> GetAuthToken(string externalAccountId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/GetAuthToken?curFr8UserId={_userId}&externalAccountId={HttpUtility.UrlEncode(externalAccountId)}";
            var uri = new Uri(url);
            var authTokenDTO = await _restfulServiceClient.GetAsync<AuthorizationTokenDTO>(uri);
            return Mapper.Map<AuthorizationToken>(authTokenDTO);
        }

        public async Task RenewToken(AuthorizationTokenDTO token)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/renewtoken";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, token);
        }

        public async Task SendEvent(Crate eventPayload)
        {
            var eventReportCrateDTO = CrateStorageSerializer.Default.ConvertToDto(eventPayload);

            if (eventReportCrateDTO != null)
            {
                var url = $"{GetHubUrlWithApiVersion()}/events";
                var uri = new Uri(url);
                await _restfulServiceClient.PostAsync(uri, eventReportCrateDTO);
            }
        }

        public async Task ScheduleEvent(string externalAccountId, string minutes, bool triggerImmediately = false, string additionalConfigAttributes = null, string additionToJobId = null)
        {
            var hubAlarmsUrl = GetHubUrlWithApiVersion() + $"/alarms/polling?terminalToken={TerminalToken}";
            var uri = new Uri(hubAlarmsUrl);
            var data = new PollingDataDTO() { Fr8AccountId = _userId, ExternalAccountId = externalAccountId, PollingIntervalInMinutes = minutes, TriggerImmediately = triggerImmediately, AdditionalConfigAttributes = additionalConfigAttributes, AdditionToJobId  = additionToJobId};

            await _restfulServiceClient.PostAsync<PollingDataDTO>(uri, data);
        }

        public async Task<List<TManifest>> QueryWarehouse<TManifest>(List<FilterConditionDTO> query)
            where TManifest : Manifest
        {
            var url = $"{GetHubUrlWithApiVersion()}/warehouse/query";
            var uri = new Uri(url);

            var payload = new QueryDTO(ManifestDiscovery.Default.GetManifestType<TManifest>().Type, query);

            return await _restfulServiceClient.PostAsync<QueryDTO, List<TManifest>>(uri, payload);
        }

        public async Task AddOrUpdateWarehouse(params Manifest[] manifests)
        {
            var url = $"{GetHubUrlWithApiVersion()}/warehouse";
            var uri = new Uri(url);

            var crateStorage = new CrateStorage(manifests.Select(x => Crate.FromContent(null, x)));
            var payload = CrateStorageSerializer.Default.ConvertToDto(crateStorage);

            await _restfulServiceClient.PostAsync(uri, payload);
        }

        public async Task DeleteFromWarehouse<TManifest>(List<FilterConditionDTO> query)
            where TManifest : Manifest
        {
            var url = $"{GetHubUrlWithApiVersion()}/warehouse/delete";
            var uri = new Uri(url);
            var payload = new QueryDTO(ManifestDiscovery.Default.GetManifestType<TManifest>().Type, query);

            await _restfulServiceClient.PostAsync(uri, payload);
        }

        private string GetHubUrlWithApiVersion()
        {
            return _apiUrl;
        }
    }
}