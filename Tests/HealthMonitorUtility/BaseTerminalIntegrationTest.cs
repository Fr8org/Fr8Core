using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Hub.Interfaces;
using Hub.Security;
using Newtonsoft.Json;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using System.Linq;
using NUnit.Framework;
using Data.Constants;
using Data.Interfaces.DataTransferObjects.Helpers;
using StructureMap;

namespace HealthMonitor.Utility
{
    public abstract class BaseTerminalIntegrationTest : BaseIntegrationTest
    {
        public IHMACService HMACService { get; set; }

        public BaseTerminalIntegrationTest()
        {
            ObjectFactory.Initialize();
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            HMACService = new Fr8HMACService();
        }

        public void AddCrate<T>(Fr8DataDTO dataDTO, T crateManifest, string label)
        {
            var crateStorage = Crate.GetStorage(dataDTO.ExplicitData);

            var crate = Crate<T>.FromContent(label, crateManifest);
            crateStorage.Add(crate);

            dataDTO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddActivityTemplate(Fr8DataDTO dataDTO, ActivityTemplateDTO activityTemplate)
        {
            AddHubCrate(dataDTO, new FieldDescriptionsCM(new FieldDTO("ActivityTemplate", JsonConvert.SerializeObject(activityTemplate))),
                "HealthMonitor_ActivityTemplate",
                ""
            );
        }

        public void AddUpstreamCrate<T>(Fr8DataDTO dataDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(dataDTO, crateManifest, "HealthMonitor_UpstreamCrate", crateLabel);
        }

        public void AddDownstreamCrate<T>(Fr8DataDTO dataDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(dataDTO, crateManifest, "HealthMonitor_DownstreamCrate", crateLabel);
        }

        protected async Task<Dictionary<string, string>> GetHMACHeader<T>(Uri requestUri, string userId, T content)
        {
            return await HMACService.GenerateHMACHeader(requestUri, TerminalId, TerminalSecret, userId, content);
        }

        public async Task<TResponse> HttpPostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.PostAsync<TRequest, TResponse>(uri, request, null, await GetHMACHeader(uri, "testUser", request));
        }

        public async Task<TResponse> HttpGetAsync<TResponse>(string url)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.GetAsync<TResponse>(uri);
        }
    }
}
