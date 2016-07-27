using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using terminalSlack.Interfaces;

namespace terminalSlack.Services
{
    public class Event : IEvent
    {
        public Task<Crate> Process(string externalEventPayload)
        {
            if (string.IsNullOrEmpty(externalEventPayload))
            {
                return null;
            }
            externalEventPayload = externalEventPayload.Trim('\"');
            var payloadFields = ParseSlackPayloadData(externalEventPayload);
            //This is for backwards compatibility. Messages received from Slack RTM mechanism will contain the owner of subscription whereas messegas received from WebHooks not
            var userName = payloadFields.FirstOrDefault(x => x.Key == "owner_name")?.Value ?? payloadFields.FirstOrDefault(x => x.Key == "user_name")?.Value;
            var teamId = payloadFields.FirstOrDefault(x => x.Key == "team_id")?.Value;
            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(teamId))
            {
                return null;
            }
            var eventReportContent = new EventReportCM
            {
                EventNames = "Slack Outgoing Message",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = userName, 
                //Now plans won't be run for entire team but rather for specific user again
                //ExternalDomainId = teamId,
                Manufacturer = "Slack",
            };
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
            return Task.FromResult((Crate)curEventReport);
        }

        private List<KeyValueDTO> ParseSlackPayloadData(string message)
        {
            var tokens = message.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            var payloadFields = new List<KeyValueDTO>();
            foreach (var token in tokens)
            {
                var nameValue = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValue.Length < 2)
                {
                    continue;
                }

                var name = HttpUtility.UrlDecode(nameValue[0]).Trim('\"');
                var value = HttpUtility.UrlDecode(nameValue[1]).Trim('\"');

                payloadFields.Add(new KeyValueDTO()
                {
                    Key = name,
                    Value = value
                });
            }

            return payloadFields;
        }

        private ICrateStorage WrapPayloadDataCrate(List<KeyValueDTO> payloadFields)
        {
            return new CrateStorage(Crate.FromContent("Payload Data", new StandardPayloadDataCM(payloadFields)));
        }
    }
}