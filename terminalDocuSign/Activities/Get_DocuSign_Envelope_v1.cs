using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
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

namespace terminalDocuSign.Activities
{
    public class Get_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Version = "1",
            Name = "Get_DocuSign_Envelope",
            Label = "Get DocuSign Envelope",
            Category = ActivityCategory.Receivers,
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            WebService = TerminalData.WebServiceDTO,
            Terminal = TerminalData.TerminalDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string AllFieldsCrateName = "DocuSign Envelope Fields";

        protected override Task InitializeDS()
        {
            var docuSignAuthDTO = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(AuthorizationToken.Token);
            var control = ControlHelper.CreateSpecificOrUpstreamValueChooser(
               "EnvelopeId",
               "EnvelopeIdSelector",
               "Upstream Design-Time Fields"
            );

            control.Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") };
            Storage.Clear();
            Storage.Add(PackControlsCrate(control));
            return Task.FromResult(0);
        }

        protected override async Task FollowUpDS()
        {
            List<FieldDTO> allFields = new List<FieldDTO>();
            var curUpstreamFields = (await GetDesignTimeFields(CrateDirection.Upstream)).Fields.ToArray();
            var upstreamFieldsCrate = CrateManager.CreateDesignTimeFieldsCrate("Upstream Design-Time Fields", curUpstreamFields);
            Storage.ReplaceByLabel(upstreamFieldsCrate);
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            string envelopeId = GetEnvelopeId(control);
            allFields.AddRange(GetTemplateUserDefinedFields(envelopeId, null));

            // Update all fields crate
            Storage.RemoveByLabel(AllFieldsCrateName);
            Storage.Add(CrateManager.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));
        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Envelope";

        protected override Task<bool> Validate()
        {
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            var envelopeId = GetEnvelopeId(control);

            if (string.IsNullOrEmpty(envelopeId))
            {
                ValidationManager.SetError("Envelope Id is not set", control);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        protected override async Task RunDS()
        {
            //Get envlopeId from configuration
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            string envelopeId = GetEnvelopeId(control);
            // if it's not valid, try to search upstream runtime values
            if (!envelopeId.IsGuid())
                envelopeId = control.GetValue(Payload);

            if (string.IsNullOrEmpty(envelopeId))
            {
                RaiseError("EnvelopeId is not set", ActivityErrorCode.PAYLOAD_DATA_MISSING);
                return;
            }

            List<FieldDTO> allFields = new List<FieldDTO>();
            // This has to be re-thinked. TemplateId is neccessary to retrieve fields but is unknown atm
            // Perhaps it can be received by EnvelopeId
            allFields.AddRange(GetEnvelopeData(envelopeId, null));
            // Update all fields crate
            Payload.Add(CrateManager.CreateDesignTimeFieldsCrate(AllFieldsCrateName, AvailabilityType.RunTime, allFields.ToArray()));

            Success();
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