
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.Crates;
using Data.States;
using Utilities;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Template_v1 : BaseDocuSignActivity
    {
        private readonly DocuSignManager _docuSignManager;

        public Get_DocuSign_Template_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActivityDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActivityDO activityDO,
            Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }
            //Get envlopeId
            var control = (DropDownList)FindControl(CrateManager.GetStorage(activityDO), "Available_Templates");
            string selectedDocusignTemplateId = control.Value;
            if (selectedDocusignTemplateId == null)
            {
                return Error(payloadCrates, "No Template was selected at design time", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

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
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var configurationCrate = CreateControlsCrate(docuSignAuthDTO);
            _docuSignManager.FillDocuSignTemplateSource(configurationCrate, "Available_Templates", docuSignAuthDTO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(configurationCrate);
            }
            return curActivityDO;
        }

        private Crate CreateControlsCrate(DocuSignAuthTokenDTO authToken)
        {
            var availableTemplates = new DropDownList
            {
                Label = "Get which template",
                Name = "Available_Templates",
                Value = null,
                Source = null
            };
            return PackControlsCrate(availableTemplates);
        }
    }
}