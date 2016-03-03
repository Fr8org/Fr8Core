using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Constants;
using Utilities;

namespace terminalDocuSign.Actions
{
    public class Archive_DocuSign_Template_v1 : BaseDocuSignActivity
    {
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
                    Source = new FieldSourceDTO
                    {
                        Label = "Available Templates",
                        ManifestType = MT.FieldDescription.GetEnumDisplayName()
                    },
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

        private readonly DocuSignManager DocuSignManager;


        public Archive_DocuSign_Template_v1()
        {
            DocuSignManager = new DocuSignManager();
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var docuSignTemplatesCrate = DocuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(docuSignTemplatesCrate);
            }

            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var confControls = GetConfigurationControls(curActivityDO);
            var selectedTemplateField = (DropDownList)GetControl(confControls, "Available_Templates", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(selectedTemplateField.Value))
            {
                return curActivityDO;
            }

            var destinationFileNameField = (TextBox)GetControl(confControls, "File_Name", ControlTypes.TextBox);
            if (string.IsNullOrEmpty(destinationFileNameField.Value))
            {
                return curActivityDO;
            }

            curActivityDO.ChildNodes = new List<PlanNodeDO>();
            var activityTemplates = await HubCommunicator.GetActivityTemplates(CurrentFr8UserId);
            var getDocusignTemplate = GetActivityTemplate(activityTemplates, "Get_DocuSign_Template");
            var convertCratesTemplate = GetActivityTemplate(activityTemplates, "ConvertCrates");
            var storeFileTemplate = GetActivityTemplate(activityTemplates, "StoreFile");

            var getDocuSignTemplateActivity = await CreateGetDocuSignTemplateActivity(getDocusignTemplate, authTokenDO, curActivityDO);
            var convertCratesActivity = await CreateConvertCratesActivity(convertCratesTemplate, curActivityDO);
            var storeFileActivity = await CreateStoreFileActivity(storeFileTemplate, curActivityDO);

            SetSelectedTemplate(getDocuSignTemplateActivity, selectedTemplateField);
            SetFromConversion(convertCratesActivity);
            convertCratesActivity = await HubCommunicator.ConfigureActivity(convertCratesActivity, CurrentFr8UserId);
            SetToConversion(convertCratesActivity);
            SetFileDetails(storeFileActivity, destinationFileNameField.Value);
            //add child nodes here
            curActivityDO.ChildNodes.Add(getDocuSignTemplateActivity);
            curActivityDO.ChildNodes.Add(convertCratesActivity);
            curActivityDO.ChildNodes.Add(storeFileActivity);

            return curActivityDO;
        }

        private async Task<ActivityDO> CreateGetDocuSignTemplateActivity(ActivityTemplateDTO template, AuthorizationTokenDO authTokenDO, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, CurrentFr8UserId, "Get Docusign Template", 1, parentAction.Id, false, authTokenDO.Id);
            return Mapper.Map<ActivityDO>(activity);
        }
        private async Task<ActivityDO> CreateConvertCratesActivity(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, CurrentFr8UserId, "Convert Crates", 2, parentAction.Id);
            return Mapper.Map<ActivityDO>(activity);
        }
        private async Task<ActivityDO> CreateStoreFileActivity(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, CurrentFr8UserId, "Store File", 3, parentAction.Id);
            return Mapper.Map<ActivityDO>(activity);
        }

        private ActivityTemplateDTO GetActivityTemplate(IEnumerable<ActivityTemplateDTO> activityList, string activityTemplateName)
        {
            var template = activityList.FirstOrDefault(x => x.Name == activityTemplateName);
            if (template == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            return template;
        }

        private void SetFileDetails(ActivityDO storeFileActivity, string fileName)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(storeFileActivity))
            {
                var confControls = GetConfigurationControls(crateStorage);
                var fileNameTextbox = (TextBox)GetControl(confControls, "File_Name", ControlTypes.TextBox);
                var fileCrateTextSource = (TextSource)GetControl(confControls, "File Crate label", ControlTypes.TextSource);


                fileNameTextbox.Value = fileName;
                fileCrateTextSource.ValueSource = "specific";
                fileCrateTextSource.TextValue = "From DocuSignTemplate To StandardFileDescription";
            }
        }

        private void SetFromConversion(ActivityDO convertCratesActivity)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(convertCratesActivity))
            {
                var confControls = GetConfigurationControls(crateStorage);
                var fromDropdown = (DropDownList)GetControl(confControls, "Available_From_Manifests", ControlTypes.DropDownList);

                fromDropdown.Value = ((int)MT.DocuSignTemplate).ToString(CultureInfo.InvariantCulture);
                fromDropdown.selectedKey = MT.DocuSignTemplate.GetEnumDisplayName();
            }
        }

        private void SetToConversion(ActivityDO convertCratesActivity)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(convertCratesActivity))
            {
                var confControls = GetConfigurationControls(crateStorage);
                var toDropdown = (DropDownList)GetControl(confControls, "Available_To_Manifests", ControlTypes.DropDownList);
                toDropdown.Value = ((int)MT.StandardFileHandle).ToString(CultureInfo.InvariantCulture);
                toDropdown.selectedKey = MT.StandardFileHandle.GetEnumDisplayName();
            }
        }

        private void SetSelectedTemplate(ActivityDO docuSignActivity, DropDownList selectedTemplateDd)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(docuSignActivity))
            {
                var confControls = GetConfigurationControls(crateStorage);
                var actionDdlb = (DropDownList)GetControl(confControls, "Available_Templates", ControlTypes.DropDownList);
                actionDdlb.selectedKey = selectedTemplateDd.selectedKey;
                actionDdlb.Value = selectedTemplateDd.Value;
            }
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
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
                if (curDocumentation.Contains("ExplainArchiveTemplate"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution work with DocuSign templates"));
                }
                if (curDocumentation.Contains("ExplainService"))
                {
                    return Task.FromResult(GenerateDocumentationRepsonce(@"This solution works and DocuSign service and uses Fr8 infrastructure"));
                }
                return Task.FromResult(GenerateErrorRepsonce("Unknown contentPath"));
            }
            return
                Task.FromResult(
                    GenerateErrorRepsonce("Unknown displayMechanism: we currently support MainPage and HelpMenu cases"));
        }
    }
}