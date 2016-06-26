using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Security;
using Newtonsoft.Json;
using StructureMap;

namespace Fr8.Testing.Integration
{
    public abstract class BaseTerminalIntegrationTest : BaseIntegrationTest
    {
        public IHMACService HMACService { get; set; }

        public BaseTerminalIntegrationTest()
        {
            ObjectFactory.Configure(Hub.StructureMap.StructureMapBootStrapper.LiveConfiguration);
            HMACService = new Fr8HMACService(ObjectFactory.GetInstance<MediaTypeFormatter>());
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
            AddHubCrate(dataDTO, new KeyValueListCM(new KeyValueDTO("ActivityTemplate", JsonConvert.SerializeObject(activityTemplate))), 
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

        public async Task<TResponse> HttpPostAsync<TRequest, TResponse>(string url, TRequest request)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.PostAsync<TRequest, TResponse>(uri, request);
        }

        public async Task<TResponse> HttpGetAsync<TResponse>(string url)
        {
            var uri = new Uri(url);
            return await RestfulServiceClient.GetAsync<TResponse>(uri);
        }
    }
}
