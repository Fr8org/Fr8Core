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
using System.Linq;

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
            NeedsAuthentication = true,
            MinPaneWidth = 330,
            Terminal = TerminalData.TerminalDTO,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        private const string AllFieldsCrateName = "DocuSign Envelope Fields";
        private IManifest _manifest;
        private IDocuSignManager _docusihManager;

        public Get_DocuSign_Envelope_v1(ICrateManager crateManager, IDocuSignManager docuSignManager, IManifest _manifest, IDocuSignManager _docusignManager)
            : base(crateManager, docuSignManager)
        {
            this._manifest = _manifest;
            this._docusihManager = _docusignManager;
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

            AddControls(infobox, control);

            return Task.FromResult(0);
        }

        public override async Task FollowUp()
        {
            var control = GetControl<TextSource>("EnvelopeIdSelector");
            var envelope_crate = CrateSignaller.MarkAvailableAtRuntime<DocuSignEnvelopeCM_v2>(AllFieldsCrateName, false);

            var availlable_crates = await HubCommunicator.GetCratesByDirection(this.ActivityId, CrateDirection.Upstream);

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
            string envelopeId = control.TextValue;

            if (envelopeId.IsGuid())
            {
                try
            {
                    var conf = _docusihManager.SetUp(AuthorizationToken.Token);
                    var envelope = _docusihManager.GetEnvelope(conf, envelopeId);
                    envelope.ExternalAccountId = AuthorizationToken.ExternalAccountId;
                    Payload.Add<DocuSignEnvelopeCM_v2>(AllFieldsCrateName, envelope);
                }
                catch { RaiseError($"Couldn't find an envelope with id={envelopeId}"); return; }
            }
            else
            { RaiseError("EnvelopeId is invalid"); return; }

            Success();
        }
    }
}