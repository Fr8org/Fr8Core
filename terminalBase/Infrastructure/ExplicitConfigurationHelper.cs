using System;
using System.Threading.Tasks;
using AutoMapper;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Hub.Managers.APIManagers.Transmitters.Restful;
using StructureMap;

namespace TerminalBase.Infrastructure
{
    public class ExplicitConfigurationHelper
    {
        public ICrateManager Crate { get; set; }
        public IRestfulServiceClient RestfulServiceClient { get; set; }

        public ExplicitConfigurationHelper()
        {
            RestfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            Crate = new CrateManager();
        }

        public string GetTerminalConfigureUrl(string endPoint)
        {
            var prefix = endPoint.ToUpper().StartsWith("HTTP://") ? "" : "http://";

            return prefix + endPoint + "/actions/configure";
        }

        private void AddHubCrate<T>(ActionDO actionDO,
            T crateManifest, string label, string innerLabel)
        {
            var crateStorage = Crate.GetStorage(actionDO.ExplicitData);

            var fullLabel = label;
            if (!string.IsNullOrEmpty(innerLabel))
            {
                fullLabel += "_" + innerLabel;
            }

            var crate = Crate<T>.FromContent(fullLabel, crateManifest);
            crateStorage.Add(crate);

            actionDO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddCrate<T>(ActionDO actionDO, T crateManifest, string label)
        {
            var crateStorage = Crate.GetStorage(actionDO.ExplicitData);

            var crate = Crate<T>.FromContent(label, crateManifest);
            crateStorage.Add(crate);

            actionDO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddUpstreamCrate<T>(ActionDO actionDO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(actionDO, crateManifest, "ExplicitData_UpstreamCrate", crateLabel);
        }

        public void AddDownstreamCrate<T>(ActionDO actionDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(actionDTO, crateManifest, "ExplicitData_DownstreamCrate", crateLabel);
        }

        public async Task<ActionDO> Configure(
            ActionDO actionDO,
            ActivityTemplateDTO activityTemplate,
            AuthorizationTokenDO authTokenDO = null)
        {
            var actionDTO = Mapper.Map<ActionDTO>(actionDO);
            actionDTO.IsExplicitData = true;
            actionDTO.ActivityTemplate = activityTemplate;

            if (authTokenDO != null)
            {
                actionDTO.AuthToken = new AuthorizationTokenDTO()
                {
                    Token = authTokenDO.Token,
                    AdditionalAttributes = authTokenDO.AdditionalAttributes
                };
            }

            var responseActionDTO = await RestfulServiceClient.PostAsync<ActionDTO, ActionDTO>(
                new Uri(GetTerminalConfigureUrl(activityTemplate.Terminal.Endpoint)),
                actionDTO
            );

            var responseActionDO = Mapper.Map<ActionDO>(responseActionDTO);
            return responseActionDO;
        }
    }
}
