using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalGoogle.Infrastructure;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using terminalGoogle.Interfaces;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Fr8.TerminalBase.Interfaces;

namespace terminalGoogle.Services
{
    public class Event : IEvent
    {
        public async Task<Crate> Process(IContainer container, string externalEventPayload)
        {
            if (string.IsNullOrEmpty(externalEventPayload))
            {
                return null;
            }

            var payloadFields = ParseGoogleFormPayloadData(externalEventPayload);
            if(payloadFields.Count == 0)
            {
                var jo = (JObject)JsonConvert.DeserializeObject(externalEventPayload);
                var curFr8UserId = jo["fr8_user_id"].Value<string>();
                if (!string.IsNullOrEmpty(curFr8UserId))
                {
                    var hub = container.GetInstance<IHubCommunicator>();
                    var plan = new GoogleMTSFPlan(curFr8UserId,hub, "alexed","general");
                    await plan.CreateAndActivateNewMTSFPlan();
                }
            }

            var externalAccountId = payloadFields.FirstOrDefault(x => x.Key == "user_id");
            if (externalAccountId == null || string.IsNullOrEmpty(externalAccountId.Value))
            {
                return null;
            }

            var eventReportContent = new EventReportCM
            {
                EventNames = "Google Form Response",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = externalAccountId.Value,
                Manufacturer = "Google"
            };

            //prepare the event report
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);

            return curEventReport;
        }

        private List<KeyValueDTO> ParseGoogleFormPayloadData(string message)
        {
            var tokens = message.Split(
                new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            var payloadFields = new List<KeyValueDTO>();
            foreach (var token in tokens)
            {
                var nameValue = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValue.Length < 2)
                {
                    continue;
                }

                var name = HttpUtility.UrlDecode(nameValue[0]);
                var value = HttpUtility.UrlDecode(nameValue[1]);

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