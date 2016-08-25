using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public class Extract_Data_From_Envelopes_v1 : BaseDocuSignActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("9676dd67-519d-4492-ad25-b5f55f9b4804"),
            Name = "Extract_Data_From_Envelopes",
            Label = "Extract Data From Envelopes",
            Version = "1",
            MinPaneWidth = 380,
            NeedsAuthentication = true,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[] { ActivityCategories.Solution }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


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

        public Extract_Data_From_Envelopes_v1(ICrateManager crateManager, IDocuSignManager docuSignManager)
            : base(crateManager, docuSignManager)
        {
        }

        public override async Task Initialize()
        {
            Storage.Clear();

            AddControls(new ActivityUi().Controls);

            await FillFinalActionsListSource("FinalActionsList");
        }

        protected async Task<ActivityTemplateDTO> GetActivityTemplateByName(string activityTemplateName)
        {
            var allActivityTemplates = await HubCommunicator.GetActivityTemplates();
            var foundActivity = allActivityTemplates.FirstOrDefault(a => a.Name == activityTemplateName);

            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. ActivitiyTemplateName: {activityTemplateName}");
            }

            return foundActivity;
        }

        public override async Task FollowUp()
        {
            var actionUi = new ActivityUi();
            actionUi.ClonePropertiesFrom(ConfigurationControls);

            //don't add child actions until a selection is made
            if (string.IsNullOrEmpty(actionUi.FinalActionsList.Value))
            {
                return;
            }

            //Removing children activities when configuring solution after returning to Solution Introduction
            if (ActivityPayload.ChildrenActivities.Count() > 0)
            {
                ActivityPayload.ChildrenActivities.RemoveAll(a => true);
            }

            // Always use default template for solution
            const string firstTemplateName = "Monitor_DocuSign_Envelope_Activity";
            var monitorDocusignTemplate = await HubCommunicator.GetActivityTemplate("terminalDocuSign", firstTemplateName);
            Guid secondActivityTemplateGuid;
            ActivityTemplateDTO secondActivityTemplate;
            if (Guid.TryParse(actionUi.FinalActionsList.Value, out secondActivityTemplateGuid))
            {
                secondActivityTemplate = await HubCommunicator.GetActivityTemplate(Guid.Parse(actionUi.FinalActionsList.Value));
            }
            else
            {
                secondActivityTemplate = await GetActivityTemplateByName(actionUi.FinalActionsList.Value);
            }

            var firstActivity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, monitorDocusignTemplate);
            var secondActivity = await HubCommunicator.AddAndConfigureChildActivity(ActivityPayload, secondActivityTemplate, "Final activity");

            await HubCommunicator.ApplyNewToken(firstActivity.Id, Guid.Parse(AuthorizationToken.Id));
            firstActivity = await HubCommunicator.ConfigureActivity(firstActivity);
            ActivityPayload.ChildrenActivities[0] = firstActivity;

            return;
        }

        protected override string ActivityUserFriendlyName => SolutionName;

        public override async Task Run()
        {
            Success();
            await Task.Yield();
        }

        /// <summary>
        /// This method provides documentation in two forms:
        /// SolutionPageDTO for general information and 
        /// ActivityResponseDTO for specific Help on minicon
        /// </summary>
        /// <param name="activityDO"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        protected override Task<DocumentationResponseDTO> GetDocumentation(string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainExtractData"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution work with DocuSign envelops"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(new DocumentationResponseDTO("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    new DocumentationResponseDTO("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }

        #region Private Methods
        private async Task FillFinalActionsListSource(string controlName)
        {
            var control = ConfigurationControls.FindByNameNested<DropDownList>(controlName);

            if (control != null)
            {
                control.ListItems = await GetFinalActionListItems();
            }
        }

        private async Task<List<ListItem>> GetFinalActionListItems()
        {
            var templates = await HubCommunicator.GetActivityTemplates(ActivityCategories.ForwardId, true);
            return templates.OrderBy(x => x.Label).Select(x => new ListItem { Key = x.Label, Value = x.Id.ToString() }).ToList();
        }
        #endregion


    }
}