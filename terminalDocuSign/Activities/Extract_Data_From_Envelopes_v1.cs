using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Activities
{
    public class Extract_Data_From_Envelopes_v1 : BaseDocuSignActivity
    {
        private const string SolutionName = "Extract Data From Envelopes";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>This powerful report generator extends the capabilities of the standard DocuSign reporting tools. 
                                                Search by Recipient or Template and build powerful queries with a few mouse clicks</p>";


        private class ActivityUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public DropDownList FinalActionsList { get; set; }

            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<img height=\"30px\" src=\"/Content/icons/web_services/DocuSign-Logo.png\">" +
                            "<p>You will be asked to select a DocuSign Template.</p>" +
                            "<p>Each time a related DocuSign Envelope is completed, we'll extract the data for you.</p>"

                });

                Controls.Add((FinalActionsList = new DropDownList
                {
                    Name = "FinalActionsList",
                    Required = true,
                    Label = "What would you like us to do with the data?",                   
                    Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") }
                }));
            }
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
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

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivtyDO, AuthorizationTokenDO authTokenDO)
        {
            var configurationCrate = PackControls(new ActivityUi());
            await FillFinalActionsListSource(configurationCrate, "FinalActionsList");
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivtyDO))
            {
                crateStorage.Clear();
                crateStorage.Add(configurationCrate);              
            }
            return curActivtyDO;
        }      

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var actionUi = new ActivityUi();

            actionUi.ClonePropertiesFrom(CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().First());

            //don't add child actions until a selection is made
            if (string.IsNullOrEmpty(actionUi.FinalActionsList.Value))
            {
                return curActivityDO;
            }

            curActivityDO.ChildNodes = new List<PlanNodeDO>();

            // Always use default template for solution
            const string firstTemplateName = "Monitor_DocuSign_Envelope_Activity";
            var monitorDocusignTemplate = await GetActivityTemplate("terminalDocuSign", firstTemplateName);
            Guid secondActivityTemplateGuid;
            ActivityTemplateDTO secondActivityTemplate;
            if (Guid.TryParse(actionUi.FinalActionsList.Value, out secondActivityTemplateGuid))
            {
                secondActivityTemplate = await GetActivityTemplate(Guid.Parse(actionUi.FinalActionsList.Value));
            }
            else
            {
                secondActivityTemplate = await GetActivityTemplateByName(actionUi.FinalActionsList.Value);
            }

            var firstActivity = await AddAndConfigureChildActivity(curActivityDO, monitorDocusignTemplate);
            var secondActivity = await AddAndConfigureChildActivity(curActivityDO, secondActivityTemplate, "Final activity");

            return curActivityDO;
        }

        protected override string ActivityUserFriendlyName => SolutionName;

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(activityDO, containerId));
        }
    
        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityDO activityDO, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = GetDefaultDocumentation(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainExtractData"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonse(@"This solution work with DocuSign envelops"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonse(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonse("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonse("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }

        #region Private Methods
        private async Task FillFinalActionsListSource(Crate configurationCrate, string controlName)
        {
            var configurationControl = configurationCrate.Get<StandardConfigurationControlsCM>();
            var control = configurationControl.FindByNameNested<DropDownList>(controlName);
            if (control != null)
            {
                control.ListItems = await GetFinalActionListItems();
            }
        }

        private async Task<List<ListItem>> GetFinalActionListItems()
        {
            var templates = await HubCommunicator.GetActivityTemplates(ActivityCategory.Forwarders, CurrentFr8UserId);
            return templates.Select(x => new ListItem() { Key = x.Label, Value = x.Id.ToString() }).ToList();
        }
        #endregion
    }
}