using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Data.Entities;
using Data.States;
using Utilities;
using terminalDocuSign.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Envelope_v1 : BaseDocuSignAction
    {
        private readonly DocuSignManager _docuSignManager;

        public Get_DocuSign_Envelope_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public override async Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActionDO, dto => ConfigurationEvaluator(dto), authTokenDO);
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
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
            var control = CreateSpecificOrUpstreamValueChooser(
               "EnvelopeId",
               "EnvelopeIdSelector",
               "Upstream Design-Time Fields"
            );

            control.Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") };

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(PackControlsCrate(control));

            }

            return await Task.FromResult<ActionDO>(curActionDO); ;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                var curUpstreamFields = (await GetDesignTimeFields(curActionDO.Id, CrateDirection.Upstream)).Fields.ToArray();
                var upstreamFieldsCrate = Crate.CreateDesignTimeFieldsCrate("Upstream Design-Time Fields", curUpstreamFields);
                updater.CrateStorage.ReplaceByLabel(upstreamFieldsCrate);

                var control = FindControl(Crate.GetStorage(curActionDO), "EnvelopeIdSelector");
                string envelopeId = GetEnvelopeID(control as TextSource, authTokenDO);
                int fieldsCount = _docuSignManager.UpdateUserDefinedFields(curActionDO, authTokenDO, updater, envelopeId);
            }
            return await Task.FromResult(curActionDO);
        }

        public async Task<PayloadDTO> Run(ActionDO actionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(actionDO, containerId);
            var payloadCrateStorage = Crate.GetStorage(payloadCrates);
            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            //Get envlopeId from configuration
            var control = (TextSource)FindControl(Crate.GetStorage(actionDO), "EnvelopeIdSelector");
            string envelopeId = GetEnvelopeID(control, authTokenDO);
            // if it's not valid, try to search upstream runtime values
            if (!envelopeId.IsGuid())
                envelopeId = control.GetValue(payloadCrateStorage);

            if (string.IsNullOrEmpty(envelopeId))
            {
                return Error(payloadCrates, "EnvelopeId", ActionErrorCode.PAYLOAD_DATA_MISSING);
            }

            using (var updater = Crate.UpdateStorage(() => payloadCrates.CrateStorage))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("DocuSign Envelope Data", _docuSignManager.CreateActionPayload(actionDO, authTokenDO, envelopeId)));
            }

            return Success(payloadCrates);
        }

        private string GetEnvelopeID(ControlDefinitionDTO control, AuthorizationTokenDO authTokenDo)
        {
            string envelopeId = null;
            var textSource = (TextSource)control;
            if (textSource.ValueSource == null)
                return null;

            envelopeId = textSource.ValueSource == "specific" ? textSource.TextValue : textSource.Value;
            return envelopeId;
        }
    }
}