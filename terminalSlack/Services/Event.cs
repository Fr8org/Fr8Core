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
            //Currently Slack username is stored in ExternalAccountId property of AuthorizationToken (in order to display it in authentication dialog)
            //TODO: this should be changed. We should have ExternalAccountName and ExternalDomainName for displaying purposes
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
                ContainerDoId = "",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = userName, 
                //Now plans won't be run for entire team but rather for specific user again
                //ExternalDomainId = teamId,
                Manufacturer = "Slack",
            };
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
            return Task.FromResult(curEventReport);
        }

        private List<Guid> ParsePlansAffected(string plansAffectedString)
        {
            if (string.IsNullOrEmpty(plansAffectedString))
            {
                return null;
            }
            try
            {
                return plansAffectedString.Split(',').Select(x => Guid.Parse(x)).ToList();
            }
            catch
            {
                return null;
            }
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