using System.Threading.Tasks;
using Newtonsoft.Json;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Interfaces;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Services
{
    public class GoogleGDrivePolling : IGoogleGDrivePolling
    {
        private readonly IGoogleDrive _googleDrive;

        public GoogleGDrivePolling(IGoogleDrive googleDrive)
        {
            _googleDrive = googleDrive;
        }

        public async Task SchedulePolling(
            IHubCommunicator hubCommunicator,
            string externalAccountId,
            GDrivePollingType pollingType,
            bool triggerImmediatly)
        {
            var pollingInterval = "1";
            await hubCommunicator.ScheduleEvent(
                externalAccountId,
                pollingInterval,
                triggerImmediatly,
                additionToJobId: pollingType.ToString()
            );
        }

        public async Task<PollingDataDTO> Poll(PollingDataDTO pollingData)
        {
            var googleAuthToken = JsonConvert.DeserializeObject<GoogleAuthDTO>(pollingData.AuthToken);
            var googleDriveService = await _googleDrive.CreateDriveService(googleAuthToken);

            string startPageToken;
            if (string.IsNullOrEmpty(pollingData.Payload))
            {
                // var response = googleDriveService.Changes
            }
            else
            {
                startPageToken = pollingData.Payload;
            }

            return pollingData;
        }
    }
}