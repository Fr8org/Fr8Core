
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Crates;

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Template_v1 : BaseDocuSignActivity
    {
        private readonly IDocuSignManager _docuSignManager;

        public Get_DocuSign_Template_v1(IDocuSignManager docuSignManager)
        {
            _docuSignManager = docuSignManager ?? new DocuSignManager();
        }
        //Left for compatibility
        public Get_DocuSign_Template_v1() : this(null)
        {
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Template";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);
            //Get template Id
            var control = (DropDownList)FindControl(CrateManager.GetStorage(activityDO), "Available_Templates");
            var selectedDocusignTemplateId = control.Value;
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            //lets download specified template from user's docusign account
            var downloadedTemplate = _docuSignManager.DownloadDocuSignTemplate(docuSignAuthDTO, selectedDocusignTemplateId);
            //and add it to payload
            var templateCrate = CreateDocuSignTemplateCrateFromDto(downloadedTemplate);
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(templateCrate);
            }
            return Success(payloadCrates);
        }

        private Crate CreateDocuSignTemplateCrateFromDto(DocuSignTemplateDTO template)
        {
            var manifest = new DocuSignTemplateCM
            {
                Body = JsonConvert.SerializeObject(template),
                CreateDate = DateTime.UtcNow,
                Name = template.Name,
                Status = template.EnvelopeData.status
            };

            return Crate.FromContent("DocuSign Template", manifest);
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
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var configurationCrate = CreateControlsCrate();
            _docuSignManager.FillDocuSignTemplateSource(configurationCrate, "Available_Templates", docuSignAuthDTO);
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(configurationCrate);
            }
            return await Task.FromResult(curActivityDO);
        }

        protected internal override bool ActivityIsValid(ActivityDO curActivityDO, out string errorMessage)
        {
            errorMessage = string.Empty;
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var configControls = GetConfigurationControls(crateStorage);
                if (configControls == null)
                {
                    errorMessage = "Controls are not configured properly";
                    return false;
                }
                var templateList = configControls.Controls.OfType<DropDownList>().First();
                if (templateList?.ListItems.Count == 0)
                {
                    errorMessage = "Please link at least one template to your DocuSign account";
                    return false;
                }
                if (string.IsNullOrEmpty(templateList?.selectedKey))
                {
                    errorMessage = "Template is not selected";
                    return false;
                }
                return true;
            }
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