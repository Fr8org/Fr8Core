using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using Newtonsoft.Json;

namespace terminalDocuSign.Activities
{
    public class Extract_Data_From_Envelopes_v1 : BaseDocuSignActivity
    {

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Extract_Data_From_Envelopes",
            Label = "Extract Data From Envelopes",
            Version = "1",
            Category = ActivityCategory.Solution,
            MinPaneWidth = 380,
            NeedsAuthentication = true,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
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

        protected override async Task InitializeDS()
        {
            var configurationCrate = PackControls(new ActivityUi());
            await FillFinalActionsListSource(configurationCrate, "FinalActionsList");
            Storage.Clear();
            Storage.Add(configurationCrate);              
        }

        protected async Task<ActivityTemplateDTO> GetActivityTemplateByName(string activityTemplateName)
        {
            var allActivityTemplates = _activityTemplateCache ?? (_activityTemplateCache = await HubCommunicator.GetActivityTemplates(CurrentUserId));
            var foundActivity = allActivityTemplates.FirstOrDefault(a => a.Name == activityTemplateName);

            if (foundActivity == null)
            {
                throw new Exception($"ActivityTemplate was not found. ActivitiyTemplateName: {activityTemplateName}");
            }

            return foundActivity;
        }

        protected override async Task FollowUpDS()
        {
            var actionUi = new ActivityUi();
            actionUi.ClonePropertiesFrom(ConfigurationControls);

            //don't add child actions until a selection is made
            if (string.IsNullOrEmpty(actionUi.FinalActionsList.Value))
            {
                return;
            }

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

            var firstActivity = await AddAndConfigureChildActivity(ActivityPayload, monitorDocusignTemplate);
            var secondActivity = await AddAndConfigureChildActivity(ActivityPayload, secondActivityTemplate, "Final activity");

            return;
        }

        protected override string ActivityUserFriendlyName => SolutionName;

        protected override async Task RunDS()
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
        protected override Task<SolutionPageDTO> GetDocumentation(string curDocumentation)
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
                    return Task.FromResult(GenerateDocumentationResponse(@"This solution work with DocuSign envelops"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationResponse(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorResponse("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorResponse("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
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
            var templates = await HubCommunicator.GetActivityTemplates(ActivityCategory.Forwarders, CurrentUserId);
            return templates.Select(x => new ListItem() { Key = x.Label, Value = x.Id.ToString() }).ToList();
        }
        #endregion

        
    }
}