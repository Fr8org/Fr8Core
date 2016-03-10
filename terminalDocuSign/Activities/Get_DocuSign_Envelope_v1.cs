using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
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
    public class Get_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        private readonly DocuSignManager _docuSignManager;

        public Get_DocuSign_Envelope_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            return await ProcessConfigurationRequest(curActivityDO, dto => ConfigurationEvaluator(dto), authTokenDO);
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
            var control = CreateSpecificOrUpstreamValueChooser(
               "EnvelopeId",
               "EnvelopeIdSelector",
               "Upstream Design-Time Fields"
            );

            control.Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") };

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControlsCrate(control));

            }

            return await Task.FromResult<ActivityDO>(curActivityDO); ;
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                var curUpstreamFields = (await  GetDesignTimeFields(curActivityDO.Id, CrateDirection.Upstream)).Fields.ToArray();
                var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Design-Time Fields", curUpstreamFields);
                crateStorage.ReplaceByLabel(upstreamFieldsCrate);

                var control = FindControl(CrateManager.GetStorage(curActivityDO), "EnvelopeIdSelector");
                var envelopeId = GetEnvelopeId(control as TextSource);
                var fieldsCount = _docuSignManager.UpdateUserDefinedFields(curActivityDO, authTokenDO, crateStorage, envelopeId);
            }
            return await Task.FromResult(curActivityDO);
        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Envelope";

        protected override bool ActivityIsValid(ActivityDO curActivityDO, out string errorMessage)
        {
            errorMessage = string.Empty;
            var control = FindControl(CrateManager.GetStorage(curActivityDO), "EnvelopeIdSelector");
            var envelopeId = GetEnvelopeId(control as TextSource);
            errorMessage = string.IsNullOrEmpty(envelopeId) ? "Envelope Id is not set" : string.Empty;
            return string.IsNullOrEmpty(errorMessage);
        }

        protected override async Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(activityDO, containerId);
            var payloadCrateStorage = CrateManager.GetStorage(payloadCrates);
            //Get envlopeId from configuration
            var control = (TextSource)FindControl(CrateManager.GetStorage(activityDO), "EnvelopeIdSelector");
            string envelopeId = GetEnvelopeId(control);
            // if it's not valid, try to search upstream runtime values
            if (!envelopeId.IsGuid())
                envelopeId = control.GetValue(payloadCrateStorage);

            if (string.IsNullOrEmpty(envelopeId))
            {
                return Error(payloadCrates, "EnvelopeId is not set", ActivityErrorCode.PAYLOAD_DATA_MISSING);
            }

            using (var crateStorage = CrateManager.UpdateStorage(() => payloadCrates.CrateStorage))
            {

                // This has to be re-thinked. TemplateId is neccessary to retrieve fields but is unknown atm
                // Perhaps it can be received by EnvelopeId
                crateStorage.Add(Crate.FromContent("DocuSign Envelope Data", _docuSignManager.CreateActivityPayload(activityDO, authTokenDO, envelopeId, null)));
            }

            return Success(payloadCrates);
        }

        private string GetEnvelopeId(ControlDefinitionDTO control)
        {
            var textSource = (TextSource)control;
            if (textSource.ValueSource == null)
            {
                return null;
            }
            return textSource.ValueSource == "specific" ? textSource.TextValue : textSource.Value;
        }
    }
}