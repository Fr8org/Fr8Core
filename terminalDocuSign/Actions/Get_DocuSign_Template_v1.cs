
using System;
using System.Collections.Generic;
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

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Template_v1 : BaseTerminalAction
    {
        private readonly DocuSignManager _docuSignManager;

        public Get_DocuSign_Template_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, ConfigurationEvaluator, authTokenDO);
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO,
            Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var processPayload = await GetProcessPayload(actionDO, containerId);
            /*
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            

            //Get envlopeId
            var control = FindControl(Crate.GetStorage(actionDO), "EnvelopeIdSelector");
            string envelopeId = GetEnvelopeID(control, authTokenDO);
            if (envelopeId == null)
            {
                throw new TerminalCodedException(TerminalErrorCode.PAYLOAD_DATA_MISSING, "EnvelopeId");
            }

            using (var updater = Crate.UpdateStorage(() => processPayload.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", _docuSignManager.CreateActionPayload(actionDO, authTokenDO, envelopeId)));
            }
            */
            return Success(processPayload);
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
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                Source = new FieldSourceDTO
                {
                    Label = "Available Templates",
                    ManifestType = MT.StandardDesignTimeFields.GetEnumDisplayName()
                }
            };
            return PackControlsCrate(availableTemplates);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            return curActionDO;
        }

    }
}