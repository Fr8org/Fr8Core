using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using terminalDocuSign.Services.New_Api;

namespace terminalDocuSign.Activities
{
    public class Archive_DocuSign_Template_v1 : BaseDocuSignActivity
    {
        protected override ActivityTemplateDTO MyTemplate { get; }


        private const string SolutionName = "Archive DocuSign Template";
        private const double SolutionVersion = 1.0;
        private const string TerminalName = "DocuSign";
        private const string SolutionBody = @"<p>This is Archive DocuSign Template solution action</p>";

        private class ActivityUi : StandardConfigurationControlsCM
        {
            public ActivityUi()
            {
                Controls = new List<ControlDefinitionDTO>();
                Controls.Add(new DropDownList
                {
                    Label = "Archive which template",
                    Name = "Available_Templates",
                    Value = null,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                });

                Controls.Add(new TextBox
                {
                    Label = "Destination File Name",
                    Name = "File_Name",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                });
            }
        }

        public Archive_DocuSign_Template_v1(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }

        public override Task Initialize()
        {
            Storage.Clear();

            AddControls(new ActivityUi().Controls);
            FillDocuSignTemplateSource("Available_Templates");
           
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            var selectedTemplateField = GetControl<DropDownList>("Available_Templates");
            if (string.IsNullOrEmpty(selectedTemplateField.Value))
            {
                return;
            }

            var destinationFileNameField = GetControl<TextBox>("File_Name");
            if (string.IsNullOrEmpty(destinationFileNameField.Value))
            {
                return;
            }

            var getDocusignTemplate = await HubCommunicator.GetActivityTemplate("terminalDocusign", "Get_DocuSign_Template");
            var convertCratesTemplate = await HubCommunicator.GetActivityTemplate("terminalFr8Core", "Convert_Crates");
            var storeFileTemplate = await HubCommunicator.GetActivityTemplate("terminalFr8Core", "Store_File");

            var getDocuSignTemplateActivity = await CreateGetDocuSignTemplateActivity(getDocusignTemplate, ActivityPayload);
            var convertCratesActivity = await CreateConvertCratesActivity(convertCratesTemplate, ActivityPayload);
            var storeFileActivity = await CreateStoreFileActivity(storeFileTemplate, ActivityPayload);

            SetSelectedTemplate(getDocuSignTemplateActivity, selectedTemplateField);
            SetFromConversion(convertCratesActivity);
            convertCratesActivity = await HubCommunicator.ConfigureActivity(convertCratesActivity);
            SetToConversion(convertCratesActivity);
            SetFileDetails(storeFileActivity, destinationFileNameField.Value);
            //add child nodes here
            ActivityPayload.ChildrenActivities.Add(getDocuSignTemplateActivity);
            ActivityPayload.ChildrenActivities.Add(convertCratesActivity);
            ActivityPayload.ChildrenActivities.Add(storeFileActivity);
        }

        private async Task<ActivityPayload> CreateGetDocuSignTemplateActivity(ActivityTemplateDTO template, ActivityPayload parentAction)
        {
            var authTokenId = Guid.Parse(AuthorizationToken.Id);
            return await HubCommunicator.CreateAndConfigureActivity(template.Id, "Get Docusign Template", 1, parentAction.Id, false, authTokenId);
        }
        private async Task<ActivityPayload> CreateConvertCratesActivity(ActivityTemplateDTO template, ActivityPayload parentAction)
        {
            return await HubCommunicator.CreateAndConfigureActivity(template.Id,  "Convert Crates", 2, parentAction.Id);
        }
        private async Task<ActivityPayload> CreateStoreFileActivity(ActivityTemplateDTO template, ActivityPayload parentAction)
        {
            return await HubCommunicator.CreateAndConfigureActivity(template.Id,  "Store File", 3, parentAction.Id);
        }

        private void SetFileDetails(ActivityPayload storeFileActivity, string fileName)
        {
            var fileNameTextbox = ActivityConfigurator.GetControl<TextBox>(storeFileActivity, "File_Name", ControlTypes.TextBox);
            var fileCrateTextSource = ActivityConfigurator.GetControl<TextSource>(storeFileActivity, "File Crate label", ControlTypes.TextSource);
            fileNameTextbox.Value = fileName;
            fileCrateTextSource.ValueSource = "specific";
            fileCrateTextSource.TextValue = "From DocuSignTemplate To StandardFileDescription";
        }

        private void SetFromConversion(ActivityPayload convertCratesActivity)
        {
            var fromDropdown = ActivityConfigurator.GetControl<DropDownList>(convertCratesActivity, "Available_From_Manifests", ControlTypes.DropDownList);
            fromDropdown.Value = ((int)MT.DocuSignTemplate).ToString(CultureInfo.InvariantCulture);
            fromDropdown.selectedKey = MT.DocuSignTemplate.GetEnumDisplayName();
        }

        private void SetToConversion(ActivityPayload convertCratesActivity)
        {
            var toDropdown = ActivityConfigurator.GetControl<DropDownList>(convertCratesActivity, "Available_To_Manifests", ControlTypes.DropDownList);
            toDropdown.Value = ((int)MT.StandardFileHandle).ToString(CultureInfo.InvariantCulture);
            toDropdown.selectedKey = MT.StandardFileHandle.GetEnumDisplayName();
        }

        private void SetSelectedTemplate(ActivityPayload docuSignActivity, DropDownList selectedTemplateDd)
        {
            var actionDdlb = ActivityConfigurator.GetControl<DropDownList>(docuSignActivity, "Available_Templates", ControlTypes.DropDownList);
            actionDdlb.selectedKey = selectedTemplateDd.selectedKey;
            actionDdlb.Value = selectedTemplateDd.Value;
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
        /// <param name="activityPayload"></param>
        /// <param name="curDocumentation"></param>
        /// <returns></returns>
        public dynamic Documentation(ActivityPayload activityPayload, string curDocumentation)
        {
            if (curDocumentation.Contains("MainPage"))
            {
                var curSolutionPage = new DocumentationResponseDTO(SolutionName, SolutionVersion, TerminalName, SolutionBody);
                return Task.FromResult(curSolutionPage);
            }
            if (curDocumentation.Contains("HelpMenu"))
            {
                if (curDocumentation.Contains("ExplainArchiveTemplate"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution work with DocuSign templates"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(new DocumentationResponseDTO(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(new DocumentationResponseDTO("Unknown contentPath"));
            }
            return
                Task.FromResult(new DocumentationResponseDTO("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }

        
    }
}