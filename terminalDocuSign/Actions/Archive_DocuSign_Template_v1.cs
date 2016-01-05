using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class Archive_DocuSign_Template_v1 : BaseTerminalAction
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


        public ExplicitConfigurationHelper ExplicitConfigurationHelper { get; set; }
        private readonly DocuSignManager DocuSignManager;
        

        public Archive_DocuSign_Template_v1()
        {
            DocuSignManager = new DocuSignManager();
            ExplicitConfigurationHelper = new ExplicitConfigurationHelper();
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
            var docuSignTemplatesCrate = DocuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            using (var updater = Crate.UpdateStorage(actionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.Add(docuSignTemplatesCrate);
            }

            return Task.FromResult(actionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            var confControls = GetConfigurationControls(actionDO);
            var selectedTemplateField = (DropDownList) GetControl(confControls, "Available_Templates", ControlTypes.DropDownList);
            if (string.IsNullOrEmpty(selectedTemplateField.Value))
            {
                return actionDO;
            }

            var destinationFileNameField = (TextBox)GetControl(confControls, "File_Name", ControlTypes.TextBox);
            if (string.IsNullOrEmpty(destinationFileNameField.Value))
            {
                return actionDO;
            }

            actionDO.ChildNodes = new List<RouteNodeDO>();

            var getDocuSignTemplateAction = await CreateGetDocuSignTemplateAction(actionDO, authTokenDO);
            SetSelectedTemplate(getDocuSignTemplateAction, selectedTemplateField);

            var convertCratesAction = await CreateConvertCratesAction(actionDO, authTokenDO);
            SetFromConversion(convertCratesAction);

            var storeFileAction = await CreateStoreFileAction(actionDO, authTokenDO);
            SetFileDetails(storeFileAction, destinationFileNameField.Value);

            //add child nodes here
            actionDO.ChildNodes.Add(getDocuSignTemplateAction);
            actionDO.ChildNodes.Add(convertCratesAction);
            actionDO.ChildNodes.Add(storeFileAction);

            return actionDO;
        }

        private void SetFileDetails(ActionDO storeFileAction, string fileName)
        {
            using (var updater = Crate.UpdateStorage(storeFileAction))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var fileNameTextbox = (TextBox)GetControl(confControls, "File_Name", ControlTypes.TextBox);
                var fileCrateTextSource = (TextSource)GetControl(confControls, "File Crate label", ControlTypes.TextSource);


                fileNameTextbox.Value = fileName;
                fileCrateTextSource.ValueSource = "specific";
                fileCrateTextSource.TextValue = "From DocuSignTemplate To StandardFileDescription";
            }
        }

        private void SetFromConversion(ActionDO convertCratesAction)
        {
            using (var updater = Crate.UpdateStorage(convertCratesAction))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var fromDropdown = (DropDownList)GetControl(confControls, "Available_From_Manifests", ControlTypes.DropDownList);
                
                fromDropdown.Value = ((int)MT.DocuSignTemplate).ToString();
            }
        }

        private void SetToConversion(ActionDO convertCratesAction)
        {
            using (var updater = Crate.UpdateStorage(convertCratesAction))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var toDropdown = (DropDownList)GetControl(confControls, "Available_To_Manifests", ControlTypes.DropDownList);
                toDropdown.Value = ((int)MT.StandardFileHandle).ToString();
            }
        }

        private void SetSelectedTemplate(ActionDO docuSignAction, DropDownList selectedTemplateDd)
        {
            using (var updater = Crate.UpdateStorage(docuSignAction))
            {
                var confControls = GetConfigurationControls(updater.CrateStorage);
                var actionDdlb = (DropDownList)GetControl(confControls, "Available_Templates", ControlTypes.DropDownList);
                actionDdlb.selectedKey = selectedTemplateDd.selectedKey;
                actionDdlb.Value = selectedTemplateDd.Value;
            }
        }


        private async Task<ActionDO> CreateGetDocuSignTemplateAction(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            return await CreateAction(actionDO, authTokenDO, "Get_DocuSign_Template", "Get DocuSign Template", "Get DocuSign Template", 1);
        }

        private async Task<ActionDO> CreateConvertCratesAction(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            return await CreateAction(actionDO, authTokenDO, "ConvertCrates", "Convert Crates", "Convert Crates", 2);
        }

        private async Task<ActionDO> CreateStoreFileAction(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            return await CreateAction(actionDO, authTokenDO, "StoreFile", "Store File", "Store File", 3);
        }

        private async Task<ActionDO> CreateAction(ActionDO actionDO, AuthorizationTokenDO authTokenDO, string activityTemplateName, string name, string label, int ordering)
        {
            
            var activityTemplate = (await HubCommunicator.GetActivityTemplates(actionDO)).FirstOrDefault(x => x.Name == activityTemplateName);

            if (activityTemplate == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            var action = new ActionDO
            {
                IsTempId = true,
                ActivityTemplateId = activityTemplate.Id,
                CrateStorage = Crate.EmptyStorageAsStr(),
                CreateDate = DateTime.Now,
                Ordering = ordering,
                Name = name,
                Label = label
            };

            action = await ExplicitConfigurationHelper.Configure(
                action,
                activityTemplate,
                authTokenDO
            );

            return action;
        }


        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }
    }
}