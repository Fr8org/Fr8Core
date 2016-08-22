using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using terminalDocuSign.Services.New_Api;
using terminalDocuSign.Infrastructure;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;

namespace terminalDocuSign.Services
{
    public class DocuSignPolling
    {
        private readonly IDocuSignManager _docuSignManager;
        private readonly IHubEventReporter _reporter;

        public DocuSignPolling(IDocuSignManager docuSignManager, IHubEventReporter reporter)
        {
            _docuSignManager = docuSignManager;
            _reporter = reporter;
        }

        public void SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId)
        {
            string pollingInterval = CloudConfigurationManager.GetSetting("terminalDocuSign.PollingInterval");
            hubCommunicator.ScheduleEvent(externalAccountId, pollingInterval);
        }

        public async Task<PollingDataDTO> Poll(PollingDataDTO pollingData)
        {
            var config = _docuSignManager.SetUp(pollingData.AuthToken);
            EnvelopesApi api = new EnvelopesApi((Configuration)config.Configuration);
            List<DocuSignEnvelopeCM_v2> changed_envelopes = new List<DocuSignEnvelopeCM_v2>();

            // 1. Poll changes

            var changed_envelopes_info = api.ListStatusChanges(config.AccountId, new EnvelopesApi.ListStatusChangesOptions()
            { fromDate = DateTime.UtcNow.AddMinutes(-Convert.ToInt32(pollingData.PollingIntervalInMinutes)).ToString("o") });
            foreach (var envelope in changed_envelopes_info.Envelopes)
            {
                var envelopeCM = new DocuSignEnvelopeCM_v2()
                {
                    EnvelopeId = envelope.EnvelopeId,
                    Status = envelope.Status,
                    StatusChangedDateTime = DateTime.Parse(envelope.StatusChangedDateTime),
                    ExternalAccountId = JToken.Parse(pollingData.AuthToken)["Email"].ToString(),
                };

                changed_envelopes.Add(envelopeCM);
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

            pollingData.Result = true;
            return pollingData;
        }

        private async Task PushEnvelopesToTerminalEndpoint(IEnumerable<DocuSignEnvelopeCM_v2> envelopesToNotify)
        {
            foreach (var envelope in envelopesToNotify)
            {
                var eventReportContent = new EventReportCM
                {
                    EventNames = DocuSignEventParser.GetEventNames(envelope),
                    EventPayload = new CrateStorage(Crate.FromContent("DocuSign Connect Event", envelope)),
                    Manufacturer = "DocuSign",
                    ExternalAccountId = envelope.ExternalAccountId
                };

                await _reporter.Broadcast(Crate.FromContent("Standard Event Report", eventReportContent));
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

            foreach (var envelope in changed_envelopes)
            {
                var filled_envelope = _docuSignManager.GetEnvelope(config, envelope.EnvelopeId);

                result.Add(filled_envelope);
            }

            return result;
        }

    }
}