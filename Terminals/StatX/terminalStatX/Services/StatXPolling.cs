using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalStatX.Helpers;
using terminalStatX.Interfaces;

namespace terminalStatX.Services
{
    public class StatXPolling : IStatXPolling
    {
        private readonly IStatXIntegration _statXIntegration;
        private readonly IHubEventReporter _hubReporter;

        public StatXPolling(IStatXIntegration statXIntegration, IHubEventReporter hubReporter)
        {
            _statXIntegration = statXIntegration;
            _hubReporter = hubReporter;
        }

        public async Task SchedulePolling(IHubCommunicator hubCommunicator, string externalAccountId, bool triggerImmediately, string groupId, string statId)
        {
            string pollingInterval = CloudConfigurationManager.GetSetting("terminalStatX.PollingInterval");

            var additionalAttributes = JsonConvert.SerializeObject(
                new
                {
                    GroupId = groupId,
                    StatId = statId
                });

            await hubCommunicator.ScheduleEvent(externalAccountId, pollingInterval, triggerImmediately, additionalAttributes, statId.Substring(0, 18));
        }

        public async Task<PollingDataDTO> Poll(PollingDataDTO pollingData)
        {
            if (string.IsNullOrEmpty(pollingData.AdditionalConfigAttributes))
            {
                pollingData.Result = false;
                return pollingData;
            }

            var attributesObject = JObject.Parse(pollingData.AdditionalConfigAttributes);

            var groupId = attributesObject["GroupId"]?.ToString();
            var statId = attributesObject["StatId"]?.ToString();

            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(statId))
            {
                pollingData.Result = false;
                return pollingData;
            }

            if (string.IsNullOrEmpty(pollingData.Payload))
            {
                //polling is called for the first time
                var latestStatWithValues = await GetLatestStatItem(pollingData.AuthToken, groupId, statId);
                pollingData.Payload = JsonConvert.SerializeObject(latestStatWithValues);
            }
            else
            {
                var statXCM = JsonConvert.DeserializeObject<StatXItemCM>(pollingData.Payload);
                var latestStatWithValues = await GetLatestStatItem(pollingData.AuthToken, groupId, statId);

                //check value by value to see if a difference exist. 
                if (StatXUtilities.CompareStatsForValueChanges(statXCM, latestStatWithValues))
                {
                    var eventReportContent = new EventReportCM
                    {
                        EventNames = "StatXValueChange_" + statId.Substring(0, 18),
                        EventPayload = new CrateStorage(Crate.FromContent("StatXValueChange", latestStatWithValues)),
                        Manufacturer = "StatX",
                        ExternalAccountId = pollingData.ExternalAccountId
                    };

                    pollingData.Payload = JsonConvert.SerializeObject(latestStatWithValues);

                    await _hubReporter.Broadcast(Crate.FromContent("Standard Event Report", eventReportContent));
                }
            }

            pollingData.Result = true;
            return pollingData;
        }

        private async Task<StatXItemCM> GetLatestStatItem(string token, string groupId, string statId)
        {
            var latestStat = await _statXIntegration.GetStat(StatXUtilities.GetStatXAuthToken(token), groupId, statId);

            return StatXUtilities.MapToStatItemCrateManifest(latestStat);
        }
    }
}