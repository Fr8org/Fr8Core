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
    public class Archive_DocuSign_Template_v1 : BaseDocuSignAction
    {
        private class ActionUi : StandardConfigurationControlsCM
        {
            public ActionUi()
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
                        ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
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
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var docuSignTemplatesCrate = DocuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(docuSignTemplatesCrate);
            }

            return Task.FromResult(curActivityDO);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var confControls = GetConfigurationControls(curActivityDO);
            var selectedTemplateField = (DropDownList) GetControl(confControls, "Available_Templates", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(selectedTemplateField.Value))
            {
                return curActivityDO;
            }

            var destinationFileNameField = (TextBox)GetControl(confControls, "File_Name", ControlTypes.TextBox);
            if (string.IsNullOrEmpty(destinationFileNameField.Value))
            {
                return curActivityDO;
            }

            curActivityDO.ChildNodes = new List<RouteNodeDO>();
            var activityTemplates = await HubCommunicator.GetActivityTemplates(curActivityDO, CurrentFr8UserId);
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

        private async Task<ActivityDO> CreateConvertCratesActivity(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Convert Crates", CurrentFr8UserId, "Convert Crates", 2,parentAction.Id);
            return Mapper.Map<ActivityDO>(activity);
        }
        private async Task<ActivityDO> CreateStoreFileActivity(ActivityTemplateDTO template, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Store File", CurrentFr8UserId, "Store File", 3,parentAction.Id);
            return Mapper.Map<ActivityDO>(activity);
        }

        private async Task<ActivityDO> CreateGetDocuSignTemplateActivity(ActivityTemplateDTO template, AuthorizationTokenDO authTokenDO, ActivityDO parentAction)
        {
            var activity = await HubCommunicator.CreateAndConfigureActivity(template.Id, "Get Docusign Template", CurrentFr8UserId, "Get Docusign Template", 1, parentAction.Id, false, authTokenDO.Id);
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
            using (var updater = Crate.UpdateStorage(storeFileActivity))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var fileNameTextbox = (TextBox)GetControl(confControls, "File_Name", ControlTypes.TextBox);
                var fileCrateTextSource = (TextSource)GetControl(confControls, "File Crate label", ControlTypes.TextSource);


                fileNameTextbox.Value = fileName;
                fileCrateTextSource.ValueSource = "specific";
                fileCrateTextSource.TextValue = "From DocuSignTemplate To StandardFileDescription";
            }
        }

        private void SetFromConversion(ActivityDO convertCratesActivity)
        {
            using (var updater = Crate.UpdateStorage(convertCratesActivity))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var fromDropdown = (DropDownList)GetControl(confControls, "Available_From_Manifests", ControlTypes.DropDownList);
                
                fromDropdown.Value = ((int)MT.DocuSignTemplate).ToString(CultureInfo.InvariantCulture);
                fromDropdown.selectedKey = MT.DocuSignTemplate.GetEnumDisplayName();
            }
        }

        private void SetToConversion(ActivityDO convertCratesActivity)
        {
            using (var updater = Crate.UpdateStorage(convertCratesActivity))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var toDropdown = (DropDownList)GetControl(confControls, "Available_To_Manifests", ControlTypes.DropDownList);
                toDropdown.Value = ((int)MT.StandardFileHandle).ToString(CultureInfo.InvariantCulture);
                toDropdown.selectedKey = MT.StandardFileHandle.GetEnumDisplayName();
            }
        }

        private void SetSelectedTemplate(ActivityDO docuSignActivity, DropDownList selectedTemplateDd)
        {
            using (var updater = Crate.UpdateStorage(docuSignActivity))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var actionDdlb = (DropDownList)GetControl(confControls, "Available_Templates", ControlTypes.DropDownList);
                actionDdlb.selectedKey = selectedTemplateDd.selectedKey;
                actionDdlb.Value = selectedTemplateDd.Value;
            }
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActivityDO, containerId));
        }
    }
}