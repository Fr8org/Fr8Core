using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.WebSockets;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalStatX.DataTransferObjects;
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

            await hubCommunicator.ScheduleEvent(externalAccountId, pollingInterval, triggerImmediately, additionalAttributes);
        }

        public async Task<PollingDataDTO> Poll(IHubCommunicator hubCommunicator, PollingDataDTO pollingData)
        {
            var token = await hubCommunicator.GetAuthToken(pollingData.ExternalAccountId);

            //find a way to get statId and groupId as a way for main monitor of stat changes
            var groupId = "";
            var statId = "";

            if (token == null)
            {
                pollingData.Result = false;
                return pollingData;
            }

            if (string.IsNullOrEmpty(pollingData.Payload))
            {
                //polling is called for the first time
                var latestStatWithValues = await GetLatestStatItem(token, groupId, statId);
                pollingData.Payload = JsonConvert.SerializeObject(latestStatWithValues);
            }
            else
            {
                var statXCM = JsonConvert.DeserializeObject<StatXItemCM>(pollingData.Payload);
                var latestStatWithValues = await GetLatestStatItem(token, groupId, statId);

                if (DateTime.Parse(statXCM.LastUpdatedDateTime) < DateTime.Parse(latestStatWithValues.LastUpdatedDateTime))
                {
                    //check value by value to see difference. 

                    var eventReportContent = new EventReportCM
                    {
                        EventNames = "StatXValueChange",
                        ContainerDoId = "",
                        EventPayload = new CrateStorage(Crate.FromContent("StatXValueChange", latestStatWithValues)),
                        Manufacturer = "StatX",
                        ExternalAccountId = token.ExternalAccountId
                    };

                    pollingData.Payload = JsonConvert.SerializeObject(latestStatWithValues);

                    await _hubReporter.Broadcast(Crate.FromContent("Standard Event Report", eventReportContent));
                }
            }
            
            pollingData.Result = true;
            return pollingData;
        }

        private async Task<StatXItemCM> GetLatestStatItem(AuthorizationToken token, string groupId, string statId)
        {
            var latestStat = await _statXIntegration.GetStat(StatXUtilities.GetStatXAuthToken(token), groupId, statId);

            return StatXUtilities.MapToStatItemCrateManifest(latestStat);
        }
    }
}