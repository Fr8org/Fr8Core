using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.IO;
using System.Net.Http;

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

        public DefaultHubCommunicator()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            TerminalSecret = CloudConfigurationManager.GetSetting("TerminalSecret");
            TerminalId = CloudConfigurationManager.GetSetting("TerminalId");
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _hmacService = ObjectFactory.GetInstance<IHMACService>();
        }

        #region HMAC

        private async Task<Dictionary<string, string>> GetHMACHeader(Uri requestUri, string userId)
        {
            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId);
        }

        private async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, string userId, T content)
        {
            return await _hmacService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }

        #endregion

        public async Task<PayloadDTO> GetPayload(ActionDO actionDO, Guid containerId, string userId)
        {
            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/containers?id="
                + containerId.ToString("D");
            var uri = new Uri(url, UriKind.Absolute);
            var payloadDTOTask = await _restfulServiceClient.GetAsync<PayloadDTO>(new Uri(url, UriKind.Absolute), containerId.ToString(), await GetHMACHeader(uri ,userId));

            return payloadDTOTask;
        }

        public async Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(ActionDO actionDO, CrateDirection direction, string userId)
        {
            var directionSuffix = (direction == CrateDirection.Upstream)
                ? "upstream_actions/"
                : "downstream_actions/";

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                +"api/"+ CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/"
                + directionSuffix
                + "?id=" + actionDO.Id;
            var uri = new Uri(url, UriKind.Absolute);
            
            var curActions = await _restfulServiceClient.GetAsync<List<ActionDTO>>(uri, null, await GetHMACHeader(uri, userId));
            var curCrates = new List<Crate<TManifest>>();

            foreach (var curAction in curActions)
        {
                var storage = _crate.FromDto(curAction.CrateStorage);

                curCrates.AddRange(storage.CratesOfType<TManifest>());
        }

            return curCrates;
        }

        public async Task<List<Crate>> GetCratesByDirection(ActionDO actionDO, CrateDirection direction, string userId)
        {
            var directionSuffix = (direction == CrateDirection.Upstream)
                ? "upstream_actions/"
                : "downstream_actions/";

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/"
                + directionSuffix
                + "?id=" + actionDO.Id;

            var uri = new Uri(url, UriKind.Absolute);
            var curActions = await _restfulServiceClient.GetAsync<List<ActionDTO>>(uri, null, await GetHMACHeader(uri, userId));
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

        public async Task<StandardDesignTimeFieldsCM> GetDesignTimeFieldsByDirection(ActionDO actionDO, CrateDirection direction, AvailabilityType availability, string userId)
        {
            return await GetDesignTimeFieldsByDirection(actionDO.Id, direction, availability, userId);
        }

        public async Task CreateAlarm(AlarmDTO alarmDTO, string userId)
        {
            var hubAlarmsUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/alarms";
            var uri = new Uri(hubAlarmsUrl);
            await _restfulServiceClient.PostAsync(uri, alarmDTO, null, await GetHMACHeader(uri, userId));
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string userId)
        {
            var hubUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl") 
                + "api/" + CloudConfigurationManager.GetSetting("HubApiVersion") + "/routenodes/available";

            var uri = new Uri(hubUrl);
            var allCategories = await _restfulServiceClient.GetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(uri, null, await GetHMACHeader(uri, userId));

            var templates = allCategories.SelectMany(x => x.Activities);
            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, ActivityCategory category, string userId)
        {
            var allTemplates = await GetActivityTemplates(actionDO, userId);
            var templates = allTemplates.Where(x => x.Category == category);

            return templates.ToList();
        }

        public async Task<List<ActivityTemplateDTO>> GetActivityTemplates(ActionDO actionDO, string tag, string userId)
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

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
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
            return await _restfulServiceClient.PostAsync<FileDO>(uri, multiPartData, null, await GetHMACHeader(uri, userId, multiPartData));
        }
    }
}
