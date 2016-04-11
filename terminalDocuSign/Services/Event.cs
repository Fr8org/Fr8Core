using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data.Crates;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Interfaces;
using Utilities.Configuration.Azure;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Infrastructure.DocuSignParserModels;

namespace terminalDocuSign.Services
{
    public class Event : terminalDocuSign.Interfaces.IEvent
    {
        private readonly EventReporter _alertReporter;
        private readonly IDocuSignPlan _docuSignPlan;

        public Event()
        {
            _alertReporter = ObjectFactory.GetInstance<EventReporter>();
            _docuSignPlan = ObjectFactory.GetInstance<IDocuSignPlan>();
        }

        public async Task<Crate> Process(string curExternalEventPayload)
        {
            //if the event payload is Fr8 User ID, it is DocuSign Authentication Completed event
            //The Monitor All DocuSign Events plan should be creaed in this case.
            if (curExternalEventPayload.Contains("fr8_user_id"))
            {
                try
                {
                    return await ConfirmAuthenticationAndCreateMADSE(curExternalEventPayload);
                }
                catch
                {
                    //create polling
                    ///////////
                }
            }

            //If this is a connect event
            if (curExternalEventPayload.Contains("DocuSignEnvelopeInformation"))
            {
                // Connect events come only for a single envelope
                var curDocuSignEnvelopeInfo = DocuSignConnectParser.GetEnvelopeInformation(curExternalEventPayload);
                // transform XML structure into DocuSignEnvelopeCM_v2
                var curDocuSingEnvelopCM = DocuSignConnectParser.ParseXMLintoCM(curDocuSignEnvelopeInfo);
                var eventReportContent = new EventReportCM
                {
                    EventNames = DocuSignConnectParser.GetEventNames(curDocuSignEnvelopeInfo),
                    ContainerDoId = "",
                    EventPayload = new CrateStorage(Crate.FromContent("DocuSign Connect Event", curDocuSingEnvelopCM)),
                    Manufacturer = "DocuSign",
                    ExternalAccountId = curDocuSignEnvelopeInfo.EnvelopeStatus.Email
                };

                ////prepare the event report
                var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
                return curEventReport;
            }
            
            return null;
        }

        private async Task<Crate> ConfirmAuthenticationAndCreateMADSE(string curExternalEventPayload)
        {
            var jo = (JObject)JsonConvert.DeserializeObject(curExternalEventPayload);
            var curFr8UserId = jo["fr8_user_id"].Value<string>();
            var authToken = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(jo["auth_token"].ToString());

            if (authToken == null)
            {
                throw new ArgumentException("Authorization Token required");
            }

            if (string.IsNullOrEmpty(curFr8UserId))
            {
                throw new ArgumentException("Fr8 User ID is not in the correct format.");
            }

            // create MonitorAllDocuSignEvents plan
            await _docuSignPlan.CreatePlan_MonitorAllDocuSignEvents(curFr8UserId, authToken);
            return null;
        }
    }
}