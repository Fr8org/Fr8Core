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

        private void AddHubCrate<T>(ActivityDTO activityDO,
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

        public void AddCrate<T>(ActivityDTO activityDO, T crateManifest, string label)
        {
            var crateStorage = Crate.GetStorage(activityDO.ExplicitData);

            var crate = Crate<T>.FromContent(label, crateManifest);
            crateStorage.Add(crate);

            activityDO.ExplicitData = Crate.CrateStorageAsStr(crateStorage);
        }

        public void AddUpstreamCrate<T>(ActivityDTO activityDO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(activityDO, crateManifest, "ExplicitData_UpstreamCrate", crateLabel);
        }

        public void AddDownstreamCrate<T>(ActivityDTO activityDTO, T crateManifest, string crateLabel = "")
        {
            AddHubCrate(activityDTO, crateManifest, "ExplicitData_DownstreamCrate", crateLabel);
        }

        public async Task<ActivityDTO> Configure(ActivityDTO activityDO,ActivityTemplateDTO activityTemplate,AuthorizationTokenDTO authTokenDTO = null)
        {
            var activityDTO = Mapper.Map<ActivityDTO>(activityDO);
            activityDTO.IsExplicitData = true;
            activityDTO.ActivityTemplate = activityTemplate;

            if (authTokenDTO != null)
            {
                activityDTO.AuthToken = new AuthorizationTokenDTO()
                {
                    Token = authTokenDTO.Token,
                    AdditionalAttributes = authTokenDTO.AdditionalAttributes
                };
            }

            var responseActionDTO = await RestfulServiceClient.PostAsync<ActivityDTO, ActivityDTO>(new Uri(GetTerminalConfigureUrl(activityTemplate.Terminal.Endpoint)),
                activityDTO
            );

            var responseActionDO = Mapper.Map<ActivityDTO>(responseActionDTO);
            return responseActionDO;
        }
    }*/
}
