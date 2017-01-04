using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using terminalZendesk.Interfaces;
using terminalZendesk.Models;

namespace terminalZendesk.Services
{
    public class Event : IEvent
    {
        public Task<Crate> Process(string externalEventPayload)
        {
            if (string.IsNullOrEmpty(externalEventPayload))
            {
                return null;
            }
            TicketCreatedEventPayload ticketPayload = null;
            try
            {
                ticketPayload = Newtonsoft.Json.JsonConvert.DeserializeObject<TicketCreatedEventPayload>(externalEventPayload);
            }
            catch
            {
                return null;
            }

            var zendeskEventCM = new ZendeskTicketCreatedEvent
            {
                TicketId = ticketPayload.Id,
                Assignee = ticketPayload.Assignee?.Name,
                Description = ticketPayload.Description,
                Title = ticketPayload.Title,
                Time = ticketPayload.CreatedAt,
                UserId = ticketPayload.CurrentUser.Id
            };
            // this is a simple hack for now
            // should find a more solid solution
            // we are using subdomains as external account id
            var subDomain = ticketPayload.Url.Split('.')[0];
            var eventReportContent = new EventReportCM
            {
                EventNames = "ticketCreated",
                EventPayload = new CrateStorage(Crate.FromContent("Zendesk ticket event", zendeskEventCM)),
                ExternalAccountId = subDomain,
                Manufacturer = "Zendesk"
            };
            var curEventReport = Crate.FromContent("Zendesk ticket event", eventReportContent);
            return Task.FromResult((Crate)curEventReport);
        }

        private ICrateStorage WrapPayloadDataCrate(List<KeyValueDTO> payloadFields)
        {
            return new CrateStorage(Crate.FromContent("Payload Data", new StandardPayloadDataCM(payloadFields)));
        }
    }
}