namespace TerminalBase.Infrastructure
{
    /*
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

            return prefix + endPoint + "/activities/configure";
        }

        private void AddHubCrate<T>(ActivityDO activityDO,
            T crateManifest, string label, string innerLabel)
        {
            var crateStorage = Crate.GetStorage(activityDO.ExplicitData);

            var fullLabel = label;
            if (!string.IsNullOrEmpty(innerLabel))
            {
                fullLabel += "_" + innerLabel;
            }

            var crate = Crate<T>.FromContent(fullLabel, crateManifest);
            crateStorage.Add(crate);

            activityDO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddCrate<T>(ActivityDO activityDO, T crateManifest, string label)
        {
            var crateStorage = Crate.GetStorage(activityDO.ExplicitData);

            var crate = Crate<T>.FromContent(label, crateManifest);
            crateStorage.Add(crate);

            activityDO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddUpstreamCrate<T>(ActivityDO activityDO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(activityDO, crateManifest, "ExplicitData_UpstreamCrate", crateLabel);
        }

        public void AddDownstreamCrate<T>(ActivityDO activityDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(activityDTO, crateManifest, "ExplicitData_DownstreamCrate", crateLabel);
        }

        public async Task<ActivityDO> Configure(ActivityDO activityDO,ActivityTemplateDTO activityTemplate,AuthorizationTokenDO authTokenDO = null)
        {
            var activityDTO = Mapper.Map<ActivityDTO>(activityDO);
            activityDTO.IsExplicitData = true;
            activityDTO.ActivityTemplate = activityTemplate;

            if (authTokenDO != null)
            {
                activityDTO.AuthToken = new AuthorizationTokenDTO()
                {
                    Token = authTokenDO.Token,
                    AdditionalAttributes = authTokenDO.AdditionalAttributes
                };
            }

            var responseActionDTO = await RestfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(new Uri(GetTerminalConfigureUrl(activityTemplate.Terminal.Endpoint)),
                activityDTO
            );

            var responseActionDO = Mapper.Map<ActivityDO>(responseActionDTO);
            return responseActionDO;
        }
    }*/
}
