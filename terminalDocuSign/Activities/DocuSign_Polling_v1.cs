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
using DocuSign.eSign.Client;
using Utilities.Configuration.Azure;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSign.Actions
{
    public class DocuSign_Polling_v1 : BaseDocuSignActivity
    {
        private IDocuSignManager _docusignManager;
        private IRestfulServiceClient _restfulServiceClient;

        public DocuSign_Polling_v1()
        {
            _docusignManager = ObjectFactory.GetInstance<IDocuSignManager>();
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

        protected override string ActivityUserFriendlyName => "DocuSign Polling";

        protected internal override async Task<PayloadDTO> RunInternal(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var config = _docusignManager.SetUp(authTokenDO);
            EnvelopesApi api = new EnvelopesApi((Configuration)config.Configuration);
            List<CrateDTO> changed_crates = new List<CrateDTO>();

            // 1. Poll changes
            string pollingInterval = CloudConfigurationManager.GetSetting("terminalDocuSign.PollingInterval");
            var changed_envelopes_info = api.ListStatusChanges(config.AccountId, new EnvelopesApi.ListStatusChangesOptions()
            { fromDate = DateTime.UtcNow.AddMinutes(-Convert.ToInt32(pollingInterval) - 1).ToString("o") });
            foreach (var envelope in changed_envelopes_info.Envelopes)
            {
                var envelopeCrate = Data.Crates.Crate.FromContent("ChangedEnvelope", new DocuSignEnvelopeCM_v2()
                {
                    EnvelopeId = envelope.EnvelopeId,
                    Status = envelope.Status,
                    StatusChangedDateTime = DateTime.Parse(envelope.StatusChangedDateTime),
                    ExternalAccountId = JToken.Parse(authTokenDO.Token)["Email"].ToString(),
                }, AvailabilityType.RunTime);
                changed_crates.Add(CrateManager.ToDto(envelopeCrate));
            }

            // 2. Check if we processed these envelopes before and if so - retrieve them
            var recorded_crates = await HubCommunicator.GetStoredManifests(CurrentFr8UserId, changed_crates);
            // 3. Fill polled envelopes with data 
            var changed_envelopes = FillChangedEnvelopesWithData(config, changed_crates);
            // 4. Exclude envelopes that came from a 1 minute overlap
            var envelopesToNotify = ExcludeCollisions(changed_envelopes, recorded_crates);
            // 5. Push envelopes to event controller
            await PushEnvelopesToTerminalEndpoint(envelopesToNotify);

            return Success(await GetPayload(curActivityDO, containerId));
        }

        private async Task PushEnvelopesToTerminalEndpoint(IEnumerable<DocuSignEnvelopeCM_v2> envelopesToNotify)
        {
            foreach (var envelope in envelopesToNotify)
            {
                var crate = CrateManager.ToDto(Data.Crates.Crate.FromContent("Polling Event", envelope));
                string publishUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint") + "/terminals/terminalDocuSign/events";
                var uri = new Uri(publishUrl, UriKind.Absolute);
                await _restfulServiceClient.PostAsync<CrateDTO>(new Uri(publishUrl, UriKind.Absolute), crate, null);
            }
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

                var authToken = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token);
                var docuSignUserCrate = Data.Crates.Crate.FromContent("DocuSignUserCrate", new StandardPayloadDataCM(new FieldDTO("DocuSignUserEmail", authToken.Email)));
                storage.Add(docuSignUserCrate);
            }
            return curActivityDO;
        }
    }
}