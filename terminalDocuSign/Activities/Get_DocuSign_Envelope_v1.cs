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
using System;
using Hub.Interfaces;

namespace terminalDocuSign.Activities
{
    public class Get_DocuSign_Envelope_v1 : BaseDocuSignActivity
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("0DE0F1FC-EBD3-48A6-9DF4-06F396E9F8C3"),
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
        private IManifest _manifest;

        public Get_DocuSign_Envelope_v1(ICrateManager crateManager, IDocuSignManager docuSignManager, IManifest _manifest)
            : base(crateManager, docuSignManager)
        {
            this._manifest = _manifest;
        }

        public override Task Initialize()
        {
            var control = UiBuilder.CreateSpecificOrUpstreamValueChooser(
               "EnvelopeId",
               "EnvelopeIdSelector",
               "Upstream Design-Time Fields"
            );

            control.Events = new List<ControlEvent>() { new ControlEvent("onChange", "requestConfig") };

            var infobox = UiBuilder.GenerateTextBlock("This activity will try to get the envelope with the specified Envelope Id. If an envelope id is known at design-time then this activity will signal envelope tabs", "", "");
            Storage.Clear();
            Storage.Add(PackControlsCrate(control, infobox));
            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            var control = GetControl<TextSource>("EnvelopeIdSelector");

            if (control.HasValue)
            {
                var value = control.GetValue(Storage);
                Guid envelopeId;

                if (Guid.TryParse(value, out envelopeId))

                {
                    var properties = ReflectionHelper.GetProperties(typeof(DocuSignEnvelopeCM_v2));
                    var fields = _manifest.ConvertPropertyToFields(properties);
                    CrateSignaller.MarkAvailableAtRuntime<DocuSignEnvelopeCM_v2>(AllFieldsCrateName, true).AddFields(fields);
                    CrateSignaller.MarkAvailableAtRuntime<DocuSignEnvelopeCM_v2>("test");
                }
            }

        }

        protected override string ActivityUserFriendlyName => "Get DocuSign Envelope";

        protected override Task Validate()
        {
            var control = GetControl<TextSource>("EnvelopeIdSelector");

            if (!control.HasValue)
            {
                ValidationManager.SetError("Envelope Id is not set", control);
            }

            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            //Get envlopeId from configuration
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            ////    string envelopeId = GetEnvelopeId(control);
            //    // if it's not valid, try to search upstream runtime values
            //    if (!envelopeId.IsGuid())
            //        envelopeId = control.GetValue(Payload);

            //    if (string.IsNullOrEmpty(envelopeId))
            //    {
            //        RaiseError("EnvelopeId is not set", ActivityErrorCode.PAYLOAD_DATA_MISSING);
            //        return;
            //    }

            //    List<KeyValueDTO> allFields = new List<KeyValueDTO>();
            //    // This has to be re-thinked. TemplateId is neccessary to retrieve fields but is unknown atm
            //    // Perhaps it can be received by EnvelopeId
            //    allFields.AddRange(GetEnvelopeData(envelopeId, null));
            //    // Update all fields crate
            //    Payload.Add(AllFieldsCrateName, new StandardPayloadDataCM(allFields));

            Success();
        }
    }
}