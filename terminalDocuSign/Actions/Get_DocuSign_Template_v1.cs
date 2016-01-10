
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
    public class Get_DocuSign_Template_v1 : BaseTerminalAction
    {
        private readonly DocuSignManager _docuSignManager;

        public Get_DocuSign_Template_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public override Task<ActionDO> Activate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            //create DocuSign account if there is no existing connect profile
            var docuSignAccount = new DocuSignAccount();
            DocuSignAccount.CreateOrUpdateDefaultDocuSignConnectConfiguration(docuSignAccount, null);

            return Task.FromResult<ActionDO>(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO,
            Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(actionDO, containerId);
            
            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }
            //Get envlopeId
            var control = (DropDownList) FindControl(Crate.GetStorage(actionDO), "Available_Templates");
            string selectedDocusignTemplateId = control.Value;
            if (selectedDocusignTemplateId == null)
            {
                return Error(payloadCrates, "No Template was selected at design time", ActionErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
            //lets download specified template from user's docusign account
            var downloadedTemplate = _docuSignManager.DownloadDocuSignTemplate(docuSignAuthDTO, selectedDocusignTemplateId);
            //and add it to payload
            var templateCrate = CreateDocuSignTemplateCrateFromDto(downloadedTemplate);
            using (var updater = Crate.UpdateStorage(payloadCrates))
            {
                updater.CrateStorage.Add(templateCrate);
            }
            return Success(payloadCrates);
        }

        private Crate CreateDocuSignTemplateCrateFromDto(DocuSignTemplateDTO template)
        {
            var manifest = new DocuSignTemplateCM
            {
                Body = JsonConvert.SerializeObject(template),
                CreateDate = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                Name = template.Name,
                Status = template.EnvelopeData.status
            };

            return Data.Crates.Crate.FromContent("DocuSign Template", manifest);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        protected override async Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
            var docuSignTemplatesCrate = _docuSignManager.PackCrate_DocuSignTemplateNames(docuSignAuthDTO);
            var controls = CreateControlsCrate();

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(controls);
                updater.CrateStorage.Add(docuSignTemplatesCrate);

            }

            return curActionDO;
        }

        private Crate CreateControlsCrate()
        {
            
            var availableTemplates = new DropDownList
            {
                Label = "Get which template",
                Name = "Available_Templates",
                Value = null,
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };
            return PackControlsCrate(availableTemplates);
        }
    }
}