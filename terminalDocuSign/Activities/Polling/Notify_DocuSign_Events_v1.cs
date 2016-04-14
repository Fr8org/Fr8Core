using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using TerminalBase.BaseClasses;
using terminalDocuSign.Infrastructure;
using Hub.Managers;
using Data.States;
using terminalDocuSign.Services.New_Api;
using StructureMap;
using TerminalBase.Infrastructure;
using terminalDocuSign.Actions;
using DocuSign.eSign.Api;
using terminalDocuSign.Services.NewApi;
using Newtonsoft.Json.Linq;
using terminalDocuSign.DataTransferObjects;
using DocuSign.eSign.Model;

namespace terminalDocuSign.Actions
{
    public class Notify_DocuSign_Events_v1 : BaseDocuSignActivity
    {
        private IDocuSignManager _docusignManager;

        protected override string ActivityUserFriendlyName => "Notify DocuSign Events";

        public Notify_DocuSign_Events_v1()
        {
            _docusignManager = ObjectFactory.GetInstance<IDocuSignManager>();
        }

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetPayload(curActivityDO, containerId);

            var config = _docusignManager.SetUp(authTokenDO);

            var changed_crates = payload.CrateStorage.Crates.Where(a => a.ManifestId == (int)MT.DocuSignEnvelope_v2 && a.Label == "ChangedEnvelope");
            var recorded_crates = payload.CrateStorage.Crates.Where(a => a.ManifestId == (int)MT.DocuSignEnvelope_v2 && a.Label == "RecordedEnvelope");

            var changed_envelopes = FillChangedEnvelopesWithData(config, changed_crates);

            var envelopesToNotify = ExcludeCollisions(changed_envelopes, recorded_crates);

            //notify

            return Success(payload);
        }

        private IEnumerable<DocuSignEnvelopeCM_v2> ExcludeCollisions(IEnumerable<DocuSignEnvelopeCM_v2> changed_crates, IEnumerable<CrateDTO> recorded_crates)
        {
            List<DocuSignEnvelopeCM_v2> result = new List<DocuSignEnvelopeCM_v2>();
            List<DocuSignEnvelopeCM_v2> recorded_envelopes = new List<DocuSignEnvelopeCM_v2>();

            foreach (var recorded_crate in recorded_crates)
            {
                recorded_envelopes.Add(Crate.FromDto(recorded_crate).Get<DocuSignEnvelopeCM_v2>());
            }

            foreach (var changed_envelope in changed_crates)
            {
                var same_recorded_envelopes = recorded_envelopes.Where(a => a.EnvelopeId == changed_envelope.EnvelopeId);

                //we dont have a record for this envelope
                if (same_recorded_envelopes.Count() == 0)
                {
                    result.Add(changed_envelope);
                    continue;
                }

                //check envelope status
                if (!same_recorded_envelopes.Any(b => b.Status == changed_envelope.Status))
                {
                    result.Add(changed_envelope);
                    continue;
                }

                //check if recipient status has changed
                var recorded_recipients = same_recorded_envelopes.SelectMany(a => a.Recipients);
                foreach (var received_recepient in changed_envelope.Recipients)
                {
                    if (!recorded_recipients.Where(a => a.RecipientId == received_recepient.RecipientId).Any(b => b.Status == received_recepient.Status))
                    {
                        //if none of recorded recipients has the same status
                        if (!result.Contains(changed_envelope))
                            result.Add(changed_envelope);

                        continue;
                    }
                }
            }

            return result;
        }

        private List<DocuSignEnvelopeCM_v2> FillChangedEnvelopesWithData(DocuSignApiConfiguration config, IEnumerable<CrateDTO> changed_crates)
        {
            var changed_envelopes = new List<DocuSignEnvelopeCM_v2>();
            EnvelopesApi api = new EnvelopesApi(config.Configuration);

            foreach (var crate in changed_crates)
            {
                var envelope = Crate.FromDto(crate).Get<DocuSignEnvelopeCM_v2>();

                //Templates
                var templates = api.ListTemplates(config.AccountId, envelope.EnvelopeId);
                var recipients = api.ListRecipients(config.AccountId, envelope.EnvelopeId);

                envelope = DocuSignEventParser.ParseAPIresponsesIntoCM(envelope, templates, recipients);
                changed_envelopes.Add(envelope);
            }

            return changed_envelopes;
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return CrateManager.IsStorageEmpty(curActivityDO) ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        protected async override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var storage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                storage.Add(PackControls(new StandardConfigurationControlsCM()
                {
                    Controls = new List<ControlDefinitionDTO>()
                { new TextBlock { Value = "This activity doesn't require any configuration" } }
                }));
            }
            return curActivityDO;
        }
    }
}