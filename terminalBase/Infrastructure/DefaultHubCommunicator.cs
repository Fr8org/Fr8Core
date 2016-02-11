using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Hub.Managers;
using StructureMap;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Utilities.Configuration.Azure;
using Data.Constants;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoMapper;

namespace TerminalBase.Infrastructure
{
    public class DefaultHubCommunicator : IHubCommunicator
    {
        private readonly IRouteNode _routeNode;
        private readonly IRestfulServiceClient _restfulServiceClient;
        protected string TerminalSecret { get; set; }
        protected string TerminalId { get; set; }

        private readonly IHMACService _hmacService;
        private readonly ICrateManager _crate;
        private bool _configured;

        public DefaultHubCommunicator()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();

            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
        }

        public Task Configure(string terminalName)
        {
            if (string.IsNullOrEmpty(terminalName))
                throw new ArgumentNullException("terminalName");

            TerminalSecret = CloudConfigurationManager.GetSetting(terminalName + "_TerminalSecret");
            TerminalId = CloudConfigurationManager.GetSetting(terminalName + "_TerminalId");
            _configured = true;
            return Task.FromResult<object>(null);
        }

        #region HMAC

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, string userId)
        {
            if (!_configured)
                throw new InvalidOperationException("Please call Configure() before using the class.");

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, string userId, T content)
        {
            if (!_configured)
                throw new InvalidOperationException("Please call Configure() before using the class.");

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, string userId, HttpContent content)
        {
            if (!_configured)
                throw new InvalidOperationException("Please call Configure() before using the class.");

            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }

        #endregion

        public async Task<PayloadDTO> GetPayload(ActivityDO activityDO, Guid containerId, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/containers/payload?id="
                + containerId.ToString("D");
            var uri = new Uri(url, UriKind.Absolute);
            var payloadDTOTask = await _restfulServiceClient.GetAsync<PayloadDTO>(new Uri(url, UriKind.Absolute), containerId.ToString(), await GetHMACHeader(uri, userId));

            return payloadDTOTask;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActivityDO activityDO, CrateDirection direction, string userId)
        {
            var directionSuffix = (direction == CrateDirection.Upstream)
                ? "upstream_actions/"
                : "downstream_actions/";

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/"
                + directionSuffix
                + "?id=" + activityDO.Id;
            var uri = new Uri(url, UriKind.Absolute);
            
            var curActions = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null, await GetHMACHeader(uri, userId));
            var curCrates = new List<Crate<TManifest>>();

            foreach (var curAction in curActions)
        {
                var storage = _crate.FromDto(curAction.CrateStorage);

                curCrates.AddRange(storage.CratesOfType<TManifest>());
        }

            return curCrates;
        }

        public async Task<List<Crate>> GetCratesByDirection(ActivityDO activityDO, CrateDirection direction, string userId)
        {
            var directionSuffix = (direction == CrateDirection.Upstream)
                ? "upstream_actions/"
                : "downstream_actions/";

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/"
                + directionSuffix
                + "?id=" + activityDO.Id;

            var uri = new Uri(url, UriKind.Absolute);
            var curActions = await _restfulServiceClient.GetAsync<List<ActivityDTO>>(uri, null, await GetHMACHeader(uri, userId));
            var curCrates = new List<Crate>();

            foreach (var curAction in curActions)
            {
                var storage = _crate.FromDto(curAction.CrateStorage);
                curCrates.AddRange(storage);
            }

            return curCrates;
        }

        public async Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/designtime_fields_dir"
                + "?id=" + activityId
                + "&direction=" + (int)direction
                + "&availability=" + (int)availability;
            var uri = new Uri(url, UriKind.Absolute);
            var curFields = await _restfulServiceClient.GetAsync<StandardDesignTimeFieldsCM>(uri, null, await GetHMACHeader(uri, userId));
            return curFields;
        }

        public async Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(ActivityDO activityDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            return await GetDesignTimeFieldsByDirection(activityDO.Id, direction, availability, userId);
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO, string userId)
        {
            var hubAlarmsUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/alarms";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, alarmDTO, null, await GetHMACHeader(uri, userId, alarmDTO));
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityDO activityDO, string userId)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl") 
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/available";

            var uri = new Uri(hubUrl);
            var allCategories = await _restfulServiceClient.GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(uri, null, await GetHMACHeader(uri, userId));

            var templates = allCategories.SelectMany(x => x.Activities);
            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityDO activityDO, ActivityCategory category, string userId)
        {
            var allTemplates = await GetActivityTemplates(activityDO, userId);
            var templates = allTemplates.Where(x => x.Category == category);

            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActivityDO activityDO, string tag, string userId)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/available?tag=";

            if (string.IsNullOrEmpty(tag))
            {
                hubUrl += "[all]";
            }
            else
            {
                hubUrl += tag;
            }

            var uri = new Uri(hubUrl);
            var templates = await _restfulServiceClient.GetAsync<List<ActivityTemplateDTO>>(uri, null, await GetHMACHeader(uri, userId));

            return templates;
        }

        public async Task<List<FieldValidationResult>> ValidateFields(List<FieldValidationDTO> fields, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/field/exists";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<List<FieldValidationDTO>, List<FieldValidationResult>>(uri, fields, null, await GetHMACHeader(uri, userId, fields));
        }

        public async Task<ActivityDTO> ConfigureActivity(ActivityDTO activityDTO, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/actions/configure";
            var uri = new Uri(url);
            return await _restfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(uri, activityDTO, null, await GetHMACHeader(uri, userId, activityDTO));
        }

        public async Task<ActivityDTO> CreateAndConfigureActivity(int templateId, string userId, string label = null, int? order = null, Guid? parentNodeId = null, bool createRoute = false, Guid? authorizationTokenId = null)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/actions/create";
            
            
            var postUrl = "?actionTemplateId={0}&createRoute={2}";
            var formattedPostUrl = string.Format(postUrl, templateId, createRoute ? "true" : "false");
            
            if (label != null)
            {
                formattedPostUrl += "&label=" + label;
            }
            if (parentNodeId != null)
            {
                formattedPostUrl += "&parentNodeId=" + parentNodeId;
            }
            if (authorizationTokenId != null)
            {
                formattedPostUrl += "&authorizationTokenId=" + authorizationTokenId;
            }
            if (order != null)
            {
                formattedPostUrl += "&order=" + order;
            }
            
            var uri = new Uri(url + formattedPostUrl);
            return await _restfulServiceClient.PostAsync<ActivityDTO>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<ActivityDO> ConfigureActivity(ActivityDO activityDO, string userId)
        {
            var activityDTO = Mapper.Map<ActivityDTO>(activityDO);
            return Mapper.Map<ActivityDO>(await ConfigureActivity(activityDTO, userId));
        }

        public async Task<RouteFullDTO> CreatePlan(RouteEmptyDTO routeDTO, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routes";
            var uri = new Uri(url);

            return await _restfulServiceClient.PostAsync<RouteEmptyDTO, RouteFullDTO>(uri, routeDTO, null, await GetHMACHeader(uri, userId, routeDTO));
        }

        public async Task<PlanDO> ActivatePlan(PlanDO planDO, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routes/activate?planId=" + planDO.Id;
            var uri = new Uri(url);

            return await _restfulServiceClient.PostAsync<PlanDO>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<IEnumerable<RouteFullDTO>> GetPlansByName(string name, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                      + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routes/getbyname?name=" + name;
            var uri = new Uri(url);

            return await _restfulServiceClient.GetAsync<IEnumerable<RouteFullDTO>>(uri, null, await GetHMACHeader(uri, userId));
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
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/files/files";
            var multiPartData = new MultipartFormDataContent();
            var byteData = ReadFully(stream);
            multiPartData.Add(new ByteArrayContent(byteData), name, name);
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.PostAsync<FileDO>(uri, multiPartData, null, await GetHMACHeader(uri, userId, (HttpContent)multiPartData));
        }

        public async Task<IEnumerable<FileDTO>> GetFiles(string userId)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/files";
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.GetAsync<IEnumerable<FileDTO>>(uri, null, await GetHMACHeader(uri, userId));
        }

        public async Task<Stream> DownloadFile(int fileId, string userId)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/files/download?id="+fileId;
            var uri = new Uri(hubUrl);
            return await _restfulServiceClient.DownloadAsync(uri, null, await GetHMACHeader(uri, userId));
        }
    }
}
