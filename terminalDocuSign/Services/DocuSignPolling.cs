using Data.Control;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Validations;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Services.NewApi;
using StructureMap;
using terminalDocuSign.Services.New_Api;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using terminalDocuSign.Infrastructure;
using Hub.Managers.APIManagers.Transmitters.Restful;
using System.Threading.Tasks;
using Hub.Managers;
using Hangfire;
using System.Web.Http.Results;
using Nito.AsyncEx;
using Hangfire.States;
using Hangfire.Common;
using Hangfire.Server;

namespace terminalDocuSign.Services
{
    public class DocuSignPolling
    {
        private IDocuSignManager _docuSignManager;
        private IHubCommunicator _hubCommunicator;
        private IRestfulServiceClient _restfulServiceClient;
        private ICrateManager _crateManager;

        public DocuSignPolling()
        {
            _hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
            _docuSignManager = ObjectFactory.GetInstance<IDocuSignManager>();
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
            _hubCommunicator.Configure("terminalDocuSign");
        }

        public void SchedulePolling(string externalAccountId, string curFr8UserId)
        {
            string pollingInterval = CloudConfigurationManager.GetSetting("terminalDocuSign.PollingInterval");
            var jobId = GetSchedulledJobId(externalAccountId);
            RecurringJob.AddOrUpdate(jobId, () => LaunchScheduledActivity(externalAccountId, curFr8UserId, pollingInterval), "*/" + pollingInterval + " * * * *", null, "terminal_docusign");
        }

        [Queue("terminal_docusign")]
        public void LaunchScheduledActivity(string externalAccountId, string curFr8UserId, string pollingInterval)
        {

            //http://stackoverflow.com/questions/9343594/how-to-call-asynchronous-method-from-synchronous-method-in-c
            var success = AsyncContext.Run(() => Poll(externalAccountId, curFr8UserId, pollingInterval));
            if (!success)
            {
                RecurringJob.RemoveIfExists(GetSchedulledJobId(externalAccountId));
            }
        }

        public string GetSchedulledJobId(string externalAccountId)
        {
            return externalAccountId.GetHashCode().ToString();
        }

        public async Task<bool> Poll(string externalAccountId, string curFr8UserId, string pollingInterval)
        {
            AuthorizationTokenDTO authtoken = await _hubCommunicator.GetAuthToken(externalAccountId, curFr8UserId);
            if (authtoken == null) return false;

            var authTokenDO = new AuthorizationTokenDO() { Token = authtoken.Token };
            var config = _docuSignManager.SetUp(authTokenDO);
            EnvelopesApi api = new EnvelopesApi((Configuration)config.Configuration);
            List<DocuSignEnvelopeCM_v2> changed_envelopes = new List<DocuSignEnvelopeCM_v2>();

            // 1. Poll changes

            var changed_envelopes_info = api.ListStatusChanges(config.AccountId, new EnvelopesApi.ListStatusChangesOptions()
            { fromDate = DateTime.UtcNow.AddMinutes(-Convert.ToInt32(pollingInterval)).ToString("o") });
            foreach (var envelope in changed_envelopes_info.Envelopes)
            {
                var envelopeCM = new DocuSignEnvelopeCM_v2()
                {
                    EnvelopeId = envelope.EnvelopeId,
                    Status = envelope.Status,
                    StatusChangedDateTime = DateTime.Parse(envelope.StatusChangedDateTime),
                    ExternalAccountId = JToken.Parse(authTokenDO.Token)["Email"].ToString(),
                };
            }

            // 2. Check if we processed these envelopes before and if so - retrieve them

            //TODO: add overlapping minute handling 
            var recorded_crates = new List<DocuSignEnvelopeCM_v2>();
            //    var recorded_crates = await _hubCommunicator.GetStoredManifests(curFr8UserId, changed_envelopes);
            // 3. Fill polled envelopes with data 
            changed_envelopes = FillChangedEnvelopesWithData(config, changed_envelopes);
            // 4. Exclude envelopes that came from a 1 minute overlap
            var envelopesToNotify = ExcludeCollisions(changed_envelopes, recorded_crates);
            // 5. Push envelopes to event controller
            await PushEnvelopesToTerminalEndpoint(envelopesToNotify);

            return true;
        }


        private async Task PushEnvelopesToTerminalEndpoint(IEnumerable<DocuSignEnvelopeCM_v2> envelopesToNotify)
        {
            foreach (var envelope in envelopesToNotify)
            {
                var crate = _crateManager.ToDto(Data.Crates.Crate.FromContent("Polling Event", envelope));
                string publishUrl = CloudConfigurationManager.GetSetting("terminalDocuSign.TerminalEndpoint") + "/terminals/terminalDocuSign/events";
                var uri = new Uri(publishUrl, UriKind.Absolute);
                await _restfulServiceClient.PostAsync<CrateDTO>(new Uri(publishUrl, UriKind.Absolute), crate, null);
            }
        }

        private IEnumerable<DocuSignEnvelopeCM_v2> ExcludeCollisions(IEnumerable<DocuSignEnvelopeCM_v2> changed_envelopes, IEnumerable<DocuSignEnvelopeCM_v2> recorded_envelopes)
        {
            List<DocuSignEnvelopeCM_v2> result = new List<DocuSignEnvelopeCM_v2>();

            foreach (var changed_envelope in recorded_envelopes)
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

        private List<DocuSignEnvelopeCM_v2> FillChangedEnvelopesWithData(DocuSignApiConfiguration config, IEnumerable<DocuSignEnvelopeCM_v2> changed_envelopes)
        {
            var result = new List<DocuSignEnvelopeCM_v2>();
            EnvelopesApi api = new EnvelopesApi(config.Configuration);
            foreach (var envelope in changed_envelopes)
            {
                //Templates
                var templates = api.ListTemplates(config.AccountId, envelope.EnvelopeId);
                var recipients = api.ListRecipients(config.AccountId, envelope.EnvelopeId);

                var filled_envelope = DocuSignEventParser.ParseAPIresponsesIntoCM(envelope, templates, recipients);

                var envelopestatus = api.GetEnvelope(config.AccountId, envelope.EnvelopeId);
                filled_envelope.CreateDate = DateTime.Parse(envelopestatus.CreatedDateTime);
                filled_envelope.SentDate = DateTime.Parse(envelopestatus.SentDateTime);

                result.Add(filled_envelope);
            }

            return result;
        }

    }
}