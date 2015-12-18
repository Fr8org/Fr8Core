using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using terminalAtlassian.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalAtlassian.Actions
{
    public class Get_Jira_Issue_v1 : BaseTerminalAction
    {
        private readonly AtlassianService _atlassianService;
        protected ICrateManager _crateManager;

        public Get_Jira_Issue_v1()
        {
            _atlassianService = ObjectFactory.GetInstance<AtlassianService>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();

        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            base.CheckAuthentication(authTokenDO);

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            base.CheckAuthentication(authTokenDO);

            var processPayload = await GetProcessPayload(curActionDO, containerId);

            string jiraKey = ExtractJiraKey(curActionDO);
            var jiraIssue = _atlassianService.GetJiraIssue(jiraKey, authTokenDO);

            using (var updater = _crateManager.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(PackCrate_JiraIssueDetails(jiraIssue));
            }

            return processPayload;
        }

        private string ExtractJiraKey(ActionDO curActionDO)
        {
            var controls = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First().Controls;
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

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(CreateControlsCrate());
            }

            return await Task.FromResult<ActionDO>(curActionDO);
        }

        private Crate CreateControlsCrate()
        {
            var control = new TextBox()
            {
                Label = "Jira Key",
                Name = "jira_key",
                Required = true,
                Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") }
            };

            return PackControlsCrate(control);
        }
    }
}