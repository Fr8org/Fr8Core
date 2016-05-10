using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Template_v1 : BaseDocuSignActivity
    {


        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Template";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);
            //Get template Id
            var control = (DropDownList)FindControl(CrateManager.GetStorage(activityDO), "Available_Templates");
            string selectedDocusignTemplateId = control.Value;
            if (selectedDocusignTemplateId == null)
            {
                return Error(payloadCrates, "No Template was selected at design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            var config = DocuSignManager.SetUp(authTokenDO);
            //lets download specified template from user's docusign account
            var downloadedTemplate = DocuSignManager.DownloadDocuSignTemplate(config, selectedDocusignTemplateId);
            //and add it to payload
            var templateCrate = CreateDocuSignTemplateCrateFromDto(downloadedTemplate);
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(templateCrate);
            }
            return Success(payloadCrates);
        }

        private Crate CreateDocuSignTemplateCrateFromDto(JObject template)
        {
            var manifest = new DocuSignTemplateCM
            {
                Body = JsonConvert.SerializeObject(template),
                CreateDate = DateTime.UtcNow,
                Name = template["Name"].ToString(),
                Status = template.Property("Name").SelectToken("status").Value<string>()
            };

            return Data.Crates.Crate.FromContent("DocuSign Template", manifest);
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
            var configurationCrate = CreateControlsCrate();
            FillDocuSignTemplateSource(configurationCrate, "Available_Templates", authTokenDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(configurationCrate);
            }
            return await Task.FromResult(curActivityDO);
        }

        public override Task ValidateActivity(ActivityDO curActivityDO, ICrateStorage crateStorage, ValidationManager validationManager)
        {
            var configControls = GetConfigurationControls(crateStorage);
            var templateList = configControls?.Controls?.OfType<DropDownList>().FirstOrDefault();

            if (!validationManager.ValidateControlExistance(templateList))
            {
                return Task.FromResult(0);
            }

            validationManager.ValidateTemplateList(templateList);

            return Task.FromResult(0);
        }

        private Crate CreateControlsCrate()
        {
            var availableTemplates = new DropDownList
            {
                Label = "Get which template",
                Name = "Available_Templates",
                Value = null,
                Source = null,
                Events = new List<ControlEvent> { ControlEvent.RequestConfig },
            };
            return PackControlsCrate(availableTemplates);
        }
    }
}