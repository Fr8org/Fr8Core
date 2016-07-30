using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
    public class DefaultHubCommunicator : IHubCommunicator
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IHMACService _hmacService;
        private readonly ICrateManager _crate;
        private string _userId;
        private ActivityContext _activityContext;

        protected string TerminalSecret { get; set; }
        protected string TerminalId { get; set; }

        public string UserId => _userId;

        public DefaultHubCommunicator()
        {
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
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
        }

        #region HMAC

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri)
        {
            /*if (!IsConfigured)
                throw new InvalidOperationException("Please call Configure() before using the class.");*/

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, _userId);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, T content)
        {
            /*if (!IsConfigured)
                throw new InvalidOperationException("Please call Configure() before using the class.");*/

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, _userId, content);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, HttpContent content)
        {
            /*if (!IsConfigured)
                throw new InvalidOperationException("Please call Configure() before using the class.");*/

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, _userId, content);
        }

        #endregion

        public async Task<PayloadDTO> GetPayload(Guid containerId)
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/containers/payload?id={containerId.ToString("D")}", UriKind.Absolute);
            var payloadDTOTask = await _restfulServiceClient.GetAsync<PayloadDTO>(uri, containerId.ToString(), await GetHMACHeader(uri));
            return payloadDTOTask;
        }

        public async Task<UserDTO> GetCurrentUser()
        {
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/users/userdata?id={_userId}", UriKind.Absolute);
            var curUser = await _restfulServiceClient.GetAsync<UserDTO>(uri, null, await GetHMACHeader(uri));
            return curUser;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction)
        {
            var directionSuffix = direction == CrateDirection.Upstream
                ? "upstream"
                : "downstream";
            var uri = new Uri($"{GetHubUrlWithApiVersion()}/plan_nodes?id={activityId}&direction={directionSuffix}", UriKind.Absolute);
            var curActivities = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null, await GetHMACHeader(uri));
            var curCrates = new List<Crate<TManifest>>();

            foreach (var curAction in curActivities)
            {
                var storage = _crate.FromDto(curAction.CrateStorage);

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
            var curActivities = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null, await GetHMACHeader(uri));
            var curCrates = new List<Crate>();

            foreach (var curAction in curActivities)
            {
                var storage = _crate.FromDto(curAction.CrateStorage);
                curCrates.AddRange(storage);
            }

            return curCrates;
        }

        public async Task<IncomingCratesDTO> GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plan_nodes/signals?id={activityId}&direction={(int)direction}&availability={(int)availability}";
            var uri = new Uri(url, UriKind.Absolute);
            var availableData = await _restfulServiceClient.GetAsync<IncomingCratesDTO>(uri, null, await GetHMACHeader(uri));
            return availableData;
        }
        
        public async Task CreateAlarm(AlarmDTO alarmDTO)
        {
            var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/alarms";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, alarmDTO, null, await GetHMACHeader(uri, alarmDTO));
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates( bool getLatestsVersionsOnly = false)
        {
            var hubUri = new Uri($"{GetHubUrlWithApiVersion()}/activity_templates");
            var allCategories = await _restfulServiceClient.GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(hubUri, null, await GetHMACHeader(hubUri));
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
            var templates = await _restfulServiceClient.GetAsync<List<ActivityTemplateDTO>>(uri, null, await GetHMACHeader(uri));
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
            return await _restfulServiceClient.PostAsync<List<FieldValidationDTO>, List<FieldValidationResult>>(uri, fields, null, await GetHMACHeader(uri, fields));
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

            var url = $"{GetHubUrlWithApiVersion()}/authentication/tokens/grant";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, token, null, await GetHMACHeader(uri, token));
        }

        public async Task<ActivityPayload> ConfigureActivity(ActivityPayload activityPayload)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/configure";
            var uri = new Uri(url);
            var activityDTO = Mapper.Map<ActivityDTO>(activityPayload);
            var resultActivityDTO = await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO, null, await GetHMACHeader(uri, activityDTO));
            return Mapper.Map<ActivityPayload>(resultActivityDTO);
        }

        public async Task<ActivityPayload> SaveActivity(ActivityPayload activityPayload)
        {
            var url = $"{GetHubUrlWithApiVersion()}/activities/save";
            var uri = new Uri(url);
            var activityDTO = Mapper.Map<ActivityDTO>(activityPayload);
            var resultActivityDTO = await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO, null, await GetHMACHeader(uri, activityDTO));
            return Mapper.Map<ActivityPayload>(resultActivityDTO);
        }

        public async Task<ActivityPayload> CreateAndConfigureActivity(Guid templateId, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
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
            var resultActivityDTO = await _restfulServiceClient.PostAsync<ActivityDTO>(uri, null, await GetHMACHeader(uri));
            return Mapper.Map<ActivityPayload>(resultActivityDTO);
        }

        public async Task<PlanDTO> CreatePlan(PlanNoChildrenDTO planDTO)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<PlanNoChildrenDTO, PlanDTO>(uri, planDTO, null, await GetHMACHeader(uri, planDTO));
        }

        public async Task RunPlan(Guid planId, List<CrateDTO> payload)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans/run?planId=" + planId;
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync<List<CrateDTO>>(uri, payload, null, await GetHMACHeader(uri, payload));
        }
        
        public async Task<IEnumerable<PlanDTO>> GetPlansByName(string name, PlanVisibility visibility = PlanVisibility.Standard)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans?name={name}&visibility={visibility}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<IEnumerable<PlanDTO>>(uri, null, await GetHMACHeader(uri));
        }

        public async Task<PlanDTO> GetPlansByActivity(string activityId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/plans?activity_id={activityId}";
            var uri = new Uri(url);
            return await _restfulServiceClient.GetAsync<PlanDTO>(uri, null, await GetHMACHeader(uri));
        }

        public async Task<PlanDTO> UpdatePlan(PlanNoChildrenDTO plan)
        {
            var jsonObject = JsonConvert.SerializeObject(plan);
            HttpContent jsonContent = new StringContent(jsonObject, Encoding.UTF8, "application/json");

            var hubUrl = $"{GetHubUrlWithApiVersion()}/plans/";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<PlanDTO>(uri, jsonContent, null, await GetHMACHeader(uri, jsonContent));
        }

        public async Task NotifyUser(TerminalNotificationDTO notificationMessage)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/notifications";
            var uri = new Uri(hubUrl);
            await _restfulServiceClient.PostAsync(uri, notificationMessage, null, await GetHMACHeader(uri, notificationMessage));
        }

        public async Task DeletePlan(Guid planId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/plans?id={planId}";
            var uri = new Uri(hubUrl);
            await _restfulServiceClient.DeleteAsync(uri, null, await GetHMACHeader(uri));
        }

        public async Task DeleteExistingChildNodesFromActivity(Guid curActivityId)
        {
            //var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/activities/deletechildnodes?activityId={curActivityId}";
            var hubAlarmsUrl = $"{GetHubUrlWithApiVersion()}/activities?id={curActivityId}&delete_child_nodes=true";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.DeleteAsync(uri, null, await GetHMACHeader(uri));
        }

        public async Task DeleteActivity(Guid curActivityId)
        {
            //var hubDeleteUrl = $"{GetHubUrlWithApiVersion()}/activities/deleteactivity?id={curActivityId}";
            var hubDeleteUrl = $"{GetHubUrlWithApiVersion()}/activities?id={curActivityId}";
            var uri = new Uri(hubDeleteUrl);
            var headers = await GetHMACHeader(uri);
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

        public async Task<FileDTO> SaveFile(string name, Stream stream)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files";
            var multiPartData = new MultipartFormDataContent();
            var byteData = ReadFully(stream);
            multiPartData.Add(new ByteArrayContent(byteData), name, name);
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<FileDTO>(uri, multiPartData, null, await GetHMACHeader(uri, (HttpContent)multiPartData));
        }

        public async Task<IEnumerable<FileDTO>> GetFiles()
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.GetAsync<IEnumerable<FileDTO>>(uri, null, await GetHMACHeader(uri));
        }

        public async Task<Stream> DownloadFile(int fileId)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/files/{fileId}";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.DownloadAsync(uri, null, await GetHMACHeader(uri));
        }

        public async Task<List<CrateDTO>> GetStoredManifests(List<CrateDTO> cratesForMTRequest)
        {
            var hubUrl = $"{GetHubUrlWithApiVersion()}/warehouse?userId={_userId}";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<List<CrateDTO>, List<CrateDTO>>(uri, cratesForMTRequest, null, await GetHMACHeader(uri, cratesForMTRequest));
        }

        public async Task<AuthorizationToken> GetAuthToken(string externalAccountId)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/GetAuthToken?externalAccountId={externalAccountId}&terminalId={TerminalId}";
            var uri = new Uri(url);
            var authTokenDTO = await _restfulServiceClient.GetAsync<AuthorizationTokenDTO>(uri, null, await GetHMACHeader(uri));
            return Mapper.Map<AuthorizationToken>(authTokenDTO);
        }

        public async Task RenewToken(string id, string externalAccountId, string token)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/renewtoken?id={id}&externalAccountId={externalAccountId}&token={token}";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, null, await GetHMACHeader(uri));
        }

        public async Task RenewToken(AuthorizationTokenDTO token)
        {
            var url = $"{GetHubUrlWithApiVersion()}/authentication/renewtoken";
            var uri = new Uri(url);
            await _restfulServiceClient.PostAsync(uri, token, null, await GetHMACHeader(uri, token));
        }

        public async Task ScheduleEvent(string externalAccountId, string minutes)
        {
            var hubAlarmsUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
               + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion")
               + string.Format("/alarms/polling?job_id={0}&fr8_account_id={1}&minutes={2}&terminal_id={3}",
               externalAccountId, _userId, minutes, TerminalId);
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, null, await GetHMACHeader(uri));
        }

        private static string GetHubUrlWithApiVersion()
        {
            return $"{CloudConfigurationManager.GetSetting("CoreWebServerUrl")}api/{CloudConfigurationManager.GetSetting("HubApiVersion")}";
        }
    }
}
