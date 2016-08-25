using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Services;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Services
{
    public class GoogleGDrivePolling : IGoogleGDrivePolling
    {
        private static readonly Dictionary<GDrivePollingType, string> _eventNames
            = new Dictionary<GDrivePollingType, string>()
            {
                { GDrivePollingType.Spreadsheets, "GoogleSpreadsheet" }
            };

        private static readonly Dictionary<GDrivePollingType, string> _fileTypes
            = new Dictionary<GDrivePollingType, string>()
            {
                { GDrivePollingType.Spreadsheets, "application/vnd.google-apps.spreadsheet".ToUpper() }
            };

        private readonly IGoogleDrive _googleDrive;
        private readonly IHubEventReporter _reporter;
        private static readonly ILog Logger = LogManager.GetLogger("terminalGoogle");

        public GoogleGDrivePolling(IGoogleDrive googleDrive, IHubEventReporter reporter)
        {
            _googleDrive = googleDrive;
            _reporter = reporter;
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

        public async Task<PollingDataDTO> Poll(PollingDataDTO pollingData, GDrivePollingType pollingType)
        {
            var googleAuthToken = JsonConvert.DeserializeObject<GoogleAuthDTO>(pollingData.AuthToken);
            var driveService = await _googleDrive.CreateDriveService(googleAuthToken);

            string startPageToken;
            if (string.IsNullOrEmpty(pollingData.Payload))
            {
                var response = driveService.Changes.GetStartPageToken().Execute();
                startPageToken = response.StartPageTokenValue;
            }
            else
            {
                startPageToken = pollingData.Payload;
            }

            var changedFiles = new Dictionary<string, string>();

            var pageToken = startPageToken;
            while (pageToken != null)
            {
                var request = driveService.Changes.List(pageToken);
                request.Fields = "changes,kind,newStartPageToken,nextPageToken";
                request.Spaces = "drive";

                var changes = request.Execute();
                foreach (var change in changes.Changes)
                {
                    if (!string.IsNullOrEmpty(change.File.MimeType)
                        && change.File.MimeType.ToUpper() == _fileTypes[pollingType])
                    {
                        if (!changedFiles.ContainsKey(change.FileId))
                        {
                            changedFiles.Add(change.FileId, change.File.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(changes.NewStartPageToken))
                {
                    startPageToken = changes.NewStartPageToken;
                }

                pageToken = changes.NextPageToken;
            }

            if (changedFiles.Count > 0)
            {
                var changedFilesCM = new KeyValueListCM();
                foreach (var pair in changedFiles)
                {
                    changedFilesCM.Values.Add(new KeyValueDTO(pair.Key, pair.Value));
                }

                var eventReportContent = new EventReportCM
                {
                    EventNames = _eventNames[pollingType],
                    EventPayload = new CrateStorage(Crate.FromContent("ChangedFiles", changedFilesCM)),
                    Manufacturer = "Google",
                    ExternalAccountId = pollingData.ExternalAccountId
                };

                Logger.Info("Polling for Google Drive: changed files of type \"" + pollingType.ToString() + "\"");
                await _reporter.Broadcast(Crate.FromContent("Standard Event Report", eventReportContent));
            }

            pollingData.Payload = startPageToken;
            return pollingData;
        }
    }
}