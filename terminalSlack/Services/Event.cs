using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
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
            var userId = payloadFields.FirstOrDefault(x => x.Key == "user_id")?.Value;
            var teamId = payloadFields.FirstOrDefault(x => x.Key == "team_id")?.Value;
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(teamId))
            {
                return null;
            }
            var eventReportContent = new EventReportCM
            {
                EventNames = "Slack Outgoing Message",
                ContainerDoId = "",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = userId,
                ExternalDomainId = teamId,
                Manufacturer = "Slack"
            };
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
            return Task.FromResult(curEventReport);
        }

        private List<FieldDTO> ParseSlackPayloadData(string message)
        {
            var tokens = message.Split(
                new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            var payloadFields = new List<FieldDTO>();
            foreach (var token in tokens)
            {
                var nameValue = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValue.Length < 2)
                {
                    continue;
                }

                var name = HttpUtility.UrlDecode(nameValue[0]).Trim('\"');
                var value = HttpUtility.UrlDecode(nameValue[1]).Trim('\"');

                payloadFields.Add(new FieldDTO()
                {
                    Key = name,
                    Value = value
                });
            }

            return payloadFields;
        }

        private ICrateStorage WrapPayloadDataCrate(List<FieldDTO> payloadFields)
        {
            return new CrateStorage(Crate.FromContent("Payload Data", new StandardPayloadDataCM(payloadFields)));
        }
    }
}