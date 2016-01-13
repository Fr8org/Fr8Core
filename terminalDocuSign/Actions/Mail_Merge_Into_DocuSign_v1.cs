using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Actions
{
    public class Mail_Merge_Into_DocuSign_v1 : BaseTerminalAction
    {
        readonly DocuSignManager _docuSignManager;
        string _dataSourceValue;
        string _docuSignTemplateValue;

        public Mail_Merge_Into_DocuSign_v1() : base()
        {
            _docuSignManager = new DocuSignManager();
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return Success(await GetPayload(curActionDO, containerId));
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private async Task<Crate> CreateConfigurationControlsCrate(ActionDO actionDO)
        {
            var controlList = new List<ControlDefinitionDTO>();

            controlList.Add(new DropDownList()
            {
                Label = "1. Where is your Source Data?",
                Name = "DataSource",
                ListItems = await GetDataSourceListItems(actionDO, "Table Data Generator")
            });

            controlList.Add(DocuSignManager.CreateDocuSignTemplatePicker(false, "DocuSignTemplate", "2. Use which DocuSign Template?"));
            controlList.Add(new Button()
            {
                Label = "Continue",
                Name = "Continue",
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onClick", "requestConfig")
                }
            });

            return PackControlsCrate(controlList.ToArray());
        }

        private async Task<List<ListItem>> GetDataSourceListItems(ActionDO actionDO, string tag)
        {
            var curActivityTempaltes = await HubCommunicator.GetActivityTemplates(actionDO, tag);
            return curActivityTempaltes.Select(at => new ListItem() { Key = at.Label, Value = at.Name }).ToList();
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActionDO> InitialConfigurationResponse(
            ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    if (authTokenDO == null || authTokenDO.Token == null)
                    {
                        updater.CrateStorage = new CrateStorage(await CreateNoAuthCrate());
                    }
                    else
                    {
                        var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);

                        //build a controls crate to render the pane
                        var configurationControlsCrate = await CreateConfigurationControlsCrate(curActionDO);
                        var templatesFieldCrate = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);

                        updater.CrateStorage = new CrateStorage(templatesFieldCrate, configurationControlsCrate);
                    }
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }

            //validate if any DocuSignTemplates has been linked to the Account
            var validationError = await ValidateDocuSignAtLeastOneTemplate(curActionDO);
            if (validationError == null) return curActionDO;

            var crateStorageTemp = Crate.GetStorage(curActionDO);
            crateStorageTemp.Add(Crate.FromDto(validationError));
            curActionDO.CrateStorage = Crate.CrateStorageAsStr(crateStorageTemp);

            return curActionDO;
        }

        private Task<Crate> CreateNoAuthCrate()
        {
            var controlList = new List<ControlDefinitionDTO>();

            controlList.Add(new TextBlock()
            {
                Value = "This action requires authentication. Please authenticate."
            });
            return Task.FromResult((Crate)PackControlsCrate(controlList.ToArray()));
        }

        private T GetStdConfigurationControl<T>(CrateStorage storage, string name)
            where T : ControlDefinitionDTO
        {
            var controls = storage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
            if (controls == null)
            {
                return null;
            }

            var control = (T)controls.FindByName(name);
            return control;
        }

        /// <summary>
        /// All validation scenarios for Mail_Merge_Into_DocuSign action
        /// </summary>
        /// <param name="curActionDO"></param>
        /// <returns></returns>
        protected override async Task<CrateStorage> ValidateAction(ActionDO curActionDO)
        {
            //first validate if any DocuSign Template has been linked to the account
            var result = await ValidateDocuSignAtLeastOneTemplate(curActionDO);
            if (result != null)
            {
                var crateStorageTemp = Crate.GetStorage(curActionDO);
                crateStorageTemp.Add(Crate.FromDto(result));
                return await Task.FromResult(crateStorageTemp);
            }
             
            //Validate if the current user selected any template to his account DocuSign account
            var validateTemplatesResult = await ValidateDocuSignTemplateSelectValue(curActionDO);
            if (validateTemplatesResult != null)
            {
                var crateStorageTemp = Crate.GetStorage(curActionDO);
                crateStorageTemp.Add(Crate.FromDto(validateTemplatesResult));
                return await Task.FromResult(crateStorageTemp);
            }

            return await Task.FromResult<CrateStorage>(null);
        }

        private async Task<CrateDTO> ValidateDocuSignTemplateSelectValue(ActionDO curActionDO)
        {
            //validate DocuSignTemplate for present selected template 
            DropDownList docuSignTemplate = GetStdConfigurationControl<DropDownList>(Crate.GetStorage(curActionDO), "DocuSignTemplate");
            if (docuSignTemplate.Value != null) return await Task.FromResult<CrateDTO>(null);

            var validationErrorCrate = Crate.CreateValidationErrorOverviewCrate("validation_error_summary", "Please select some DocuSign template from the dropdown list.", "Mail Merge Into DocuSign");
            return await Task.FromResult(Crate.ToDto(validationErrorCrate));
        }

        private async Task<CrateDTO> ValidateDocuSignAtLeastOneTemplate(ActionDO curActionDO)
        {
            //validate DocuSignTemplate for present selected template 
            var crateStorage = Crate.GetStorage(curActionDO);
            var docuSignTemplate = crateStorage.CrateContentsOfType<StandardDesignTimeFieldsCM>(x=>x.Label == "Available Templates").FirstOrDefault();

            if (docuSignTemplate != null && docuSignTemplate.Fields != null && docuSignTemplate.Fields.Count != 0) return  await Task.FromResult<CrateDTO>(null);

            var validationErrorCrate = Crate.CreateValidationErrorOverviewCrate("validation_error_summary", "Please link some templates to your DocuSign account.", "Mail Merge Into DocuSign");
            return await Task.FromResult(Crate.ToDto(validationErrorCrate));
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            // Do not tarsnfer to follow up when child actions are already present 
            if (curActionDO.ChildNodes.Count() > 0) return ConfigurationRequestType.Initial;

            var storage = Crate.GetStorage(curActionDO);
            if (storage == null || storage.Count() == 0)
            {
                return ConfigurationRequestType.Initial;
            }

            // "Follow up" phase is when Continue button is clicked 
            Button button = GetStdConfigurationControl<Button>(storage, "Continue");
            if (button == null) return ConfigurationRequestType.Initial;
            if (button.Clicked == false) return ConfigurationRequestType.Initial;

            // If no values selected in textboxes, remain on initial phase
            DropDownList dataSource = GetStdConfigurationControl<DropDownList>(storage, "DataSource");
            if (dataSource.Value == null) return ConfigurationRequestType.Initial;
            _dataSourceValue = dataSource.Value;

            DropDownList docuSignTemplate = GetStdConfigurationControl<DropDownList>(storage, "DocuSignTemplate");
            if (docuSignTemplate.Value == null) return ConfigurationRequestType.Initial;
            _docuSignTemplateValue = docuSignTemplate.Value;

            return ConfigurationRequestType.Followup;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActionDO> FollowupConfigurationResponse(
            ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            _docuSignManager.ExtractFieldsAndAddToCrate(_docuSignTemplateValue, docuSignAuthDTO, curActionDO);
            var curActivityTemplates = (await HubCommunicator.GetActivityTemplates(curActionDO, null))
                .Select(x => Mapper.Map<ActivityTemplateDO>(x))
                .ToList();

            try
            {
                ActivityTemplateDO dataSourceActTempl = curActivityTemplates.FirstOrDefault(at => at.Name == _dataSourceValue);
                if (dataSourceActTempl == null) return curActionDO;
                curActionDO.ChildNodes.Add(new ActionDO()
                {
                    ActivityTemplateId = dataSourceActTempl.Id,
                    IsTempId = true,
                    Name = dataSourceActTempl.Name,
                    Label = dataSourceActTempl.Label,
                    CrateStorage = Crate.EmptyStorageAsStr(),
                    ParentRouteNode = curActionDO,
                    Ordering = 1
                });

                ActivityTemplateDO mapFieldActTempl = curActivityTemplates.FirstOrDefault(at => at.Name == "MapFields");
                if (mapFieldActTempl == null) return curActionDO;

                curActionDO.ChildNodes.Add(new ActionDO()
                {
                    ActivityTemplateId = mapFieldActTempl.Id,
                    IsTempId = true,
                    Name = mapFieldActTempl.Name,
                    Label = mapFieldActTempl.Label,
                    CrateStorage = Crate.EmptyStorageAsStr(),
                    ParentRouteNode = curActionDO,
                    Ordering = 2
                });

                ActivityTemplateDO sendDocuSignEnvActTempl = curActivityTemplates.FirstOrDefault(at => at.Name == "Send_DocuSign_Envelope");
                if (sendDocuSignEnvActTempl == null) return curActionDO;
                curActionDO.ChildNodes.Add(new ActionDO()
                {
                    ActivityTemplateId = sendDocuSignEnvActTempl.Id,
                    IsTempId = true,
                    Name = sendDocuSignEnvActTempl.Name,
                    CrateStorage = Crate.EmptyStorageAsStr(),
                    Label = sendDocuSignEnvActTempl.Label,
                    ParentRouteNode = curActionDO,
                    Ordering = 3
                });

                //uow.ActionRepository.Add(curActionDO);
                //uow.Db.Entry<ActionDO>(curActionDO).State = System.Data.Entity.EntityState.Modified;
                //uow.SaveChanges();
            }
            catch (Exception)
            {
                return null;
            }


            return await Task.FromResult(curActionDO);
        }
    }
}