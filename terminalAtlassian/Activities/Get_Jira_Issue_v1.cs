using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;
using terminalAtlassian.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalAtlassian.Actions
{
    public class Get_Jira_Issue_v1 : BaseTerminalActivity
    {
        private readonly AtlassianService _atlassianService;
        protected ICrateManager _crateManager;

        public Get_Jira_Issue_v1()
        {
            _atlassianService = ObjectFactory.GetInstance<AtlassianService>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(CreateControlsCrate());
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            string jiraKey = ExtractJiraKey(curActivityDO);
            var jiraIssue = _atlassianService.GetJiraIssue(jiraKey, authTokenDO);

            using (var crateStorage = _crateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(PackCrate_JiraIssueDetails(jiraIssue));
            }

            return Success(payloadCrates);
        }

        private string ExtractJiraKey(ActivityDO curActivityDO)
        {
            var controls = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First().Controls;
            var templateTextBox = controls.SingleOrDefault(x => x.Name == "jira_key");

            if (templateTextBox == null)
            {
                throw new ApplicationException("Could not find jira_key TextBox control.");
            }

            return templateTextBox.Value;
        }

        private Crate PackCrate_JiraIssueDetails(List<FieldDTO> curJiraIssue)
        {
            return Data.Crates.Crate.FromContent("Jira Issue Details", new StandardPayloadDataCM(curJiraIssue));
        }

        private Crate CreateControlsCrate()
        {
            var control = new TextBox()
            {
                Label = "Jira Key",
                Name = "jira_key",
                Required = true
            };

            return PackControlsCrate(control);
        }
    }
}