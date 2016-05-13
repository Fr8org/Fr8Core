using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Data.Entities;
using Data.States;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.States;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalDocuSign.Actions
{
    public class Get_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        private const string AllFieldsCrateName = "DocuSign Envelope Fields";

        public override async Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (CheckAuthentication(curActivityDO, authTokenDO))
            {
                return curActivityDO;
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
                List<FieldDTO> allFields = new List<FieldDTO>();

                var curUpstreamFields = (await GetDesignTimeFields(curActivityDO, CrateDirection.Upstream)).Fields.ToArray();
                var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Design-Time Fields", curUpstreamFields);
                crateStorage.ReplaceByLabel(upstreamFieldsCrate);

                var control = FindControl(CrateManager.GetStorage(curActivityDO), "EnvelopeIdSelector");
                string envelopeId = GetEnvelopeId(control as TextSource);
                allFields.AddRange(GetTemplateUserDefinedFields(authTokenDO, envelopeId, null));

                // Update all fields crate
                crateStorage.RemoveByLabel(AllFieldsCrateName);
                crateStorage.Add(Crate.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));
            }
            return await Task.FromResult(curActivityDO);
        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Envelope";

        public override Task ValidateActivity(ActivityDO curActivityDO, ICrateStorage crateStorage, ValidationManager validationManager)
        {
            var control = (TextSource)FindControl(crateStorage, "EnvelopeIdSelector");
            var envelopeId = GetEnvelopeId(control);

            if (string.IsNullOrEmpty(envelopeId))
            {
                validationManager.SetError("Envelope Id is not set", control);
            }

            return Task.FromResult(0);
        }

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO activityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
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
                List<FieldDTO> allFields = new List<FieldDTO>();

                // This has to be re-thinked. TemplateId is neccessary to retrieve fields but is unknown atm
                // Perhaps it can be received by EnvelopeId
                allFields.AddRange(GetEnvelopeData(authTokenDO, envelopeId, null));

                // Update all fields crate
                crateStorage.Add(Crate.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));
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