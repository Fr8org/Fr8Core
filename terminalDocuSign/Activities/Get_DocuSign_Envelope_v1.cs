using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Utilities;
using terminalDocuSign.Services.New_Api;

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
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string AllFieldsCrateName = "DocuSign Envelope Fields";


        public Get_DocuSign_Envelope_v1(ICrateManager crateManager, IDocuSignManager docuSignManager) 
            : base(crateManager, docuSignManager)
        {
        }

        public override Task Initialize()
        {
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

        public override async Task FollowUp()
        {
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            string envelopeId = GetEnvelopeId(control);

            CrateSignaller.MarkAvailableAtRuntime<StandardPayloadDataCM>(AllFieldsCrateName, true).AddFields(GetTemplateUserDefinedFields(envelopeId));
        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Envelope";

        protected override Task Validate()
        {
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            var envelopeId = GetEnvelopeId(control);

            if (string.IsNullOrEmpty(envelopeId))
            {
                ValidationManager.SetError("Envelope Id is not set", control);
            }

            return Task.FromResult(0);
        }

        public override async Task Run()
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

            List<KeyValueDTO> allFields = new List<KeyValueDTO>();
            // This has to be re-thinked. TemplateId is neccessary to retrieve fields but is unknown atm
            // Perhaps it can be received by EnvelopeId
            allFields.AddRange(GetEnvelopeData(envelopeId, null));
            // Update all fields crate
            Payload.Add(AllFieldsCrateName, new StandardPayloadDataCM(allFields));

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