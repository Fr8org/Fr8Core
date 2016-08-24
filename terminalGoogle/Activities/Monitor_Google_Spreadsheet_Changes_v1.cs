using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities.Logging;
using terminalGoogle.Actions;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Activities
{
    public class Monitor_Google_Spreadsheet_Changes_v1
        : BaseGoogleTerminalActivity<Monitor_Google_Spreadsheet_Changes_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO()
        {
            Id = new Guid("1DD23540-10A3-4672-AC44-C647C5B235AD"),
            Name = "Monitor_Google_Spreadsheet_Changes",
            Label = "Monitor Google Spreadsheet Changes",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 400,
            Tags = string.Empty,
            Categories = new[]
            {
                ActivityCategories.Monitor,
                TerminalData.GooogleActivityCategoryDTO
            }
        };

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SpreadsheetList { get; set; }

            public RadioButtonGroup SpreadsheetSelectionGroup { get; set; }

            public RadioButtonOption AllSpreadsheetsOption { get; set; }

            public RadioButtonOption SpecificSpreadsheetOption { get; set; }

            public ActivityUi()
            {
                SpreadsheetList = new DropDownList()
                {
                    Name = "SpreadsheetList",
                    Required = true,
                    Events = new List<ControlEvent>() { ControlEvent.RequestConfig }
                };

                AllSpreadsheetsOption = new RadioButtonOption()
                {
                    Selected = true,
                    Name = nameof(AllSpreadsheetsOption),
                    Value = "Any available spreadsheet"
                };

                SpecificSpreadsheetOption = new RadioButtonOption()
                {
                    Selected = false,
                    Name = nameof(SpecificSpreadsheetOption),
                    Value = "Specific spreadsheet",
                    Controls = new List<ControlDefinitionDTO>() { SpreadsheetList }
                };

                SpreadsheetSelectionGroup = new RadioButtonGroup()
                {
                    GroupName = nameof(SpreadsheetSelectionGroup),
                    Name = nameof(SpreadsheetSelectionGroup),
                    Label = "Would you like to monitor:",
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Radios = new List<RadioButtonOption>()
                    {
                        AllSpreadsheetsOption,
                        SpecificSpreadsheetOption
                    }
                };

                Controls.Add(SpreadsheetSelectionGroup);
            }
        }

        private const string RunTimeCrateLabel = "MonitorGoogleSpreadsheetChanges Changed Files";
        private const string SpreadsheetIdLabel = "Google Spreadsheet File ID";
        private const string SpreadsheetNameLabel = "Google Spreadsheet File Name";

        private IGoogleSheet _googleSheet;
        private IGoogleGDrivePolling _googleGDrivePolling;

        public Monitor_Google_Spreadsheet_Changes_v1(
            ICrateManager crateManager,
            IGoogleIntegration googleIntegration,
            IGoogleSheet googleSheet,
            IGoogleGDrivePolling googleGDrivePolling)
                : base(crateManager, googleIntegration)
        {
            _googleSheet = googleSheet;
            _googleGDrivePolling = googleGDrivePolling;
        }

        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        public override async Task Initialize()
        {
            EventSubscriptions.Manufacturer = "Google";
            EventSubscriptions.Add("GoogleSpreadsheet");

            var spreadsheets = await _googleSheet.GetSpreadsheets(GetGoogleAuthToken());
            var spreadsheetListItems = spreadsheets
                .Select(x => new ListItem { Key = x.Value, Value = ExtractFileId(x.Key) })
                .ToList();

            ActivityUI.SpreadsheetList.ListItems = spreadsheetListItems;

            CrateSignaller
                .MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel, true)
                .AddField(new FieldDTO(SpreadsheetIdLabel))
                .AddField(new FieldDTO(SpreadsheetNameLabel));
        }

        private string ExtractFileId(string feedUrl)
        {
            return feedUrl.Substring(feedUrl.LastIndexOf('/') + 1);
        }

        public override async Task FollowUp()
        {
            await Task.Yield();
        }

        public override async Task Activate()
        {
            Logger.GetLogger().Info("Monitor_Google_Spreadsheet_Changed activty is activated. Sending a request for polling");

            await _googleGDrivePolling.SchedulePolling(
                HubCommunicator,
                AuthorizationToken.ExternalAccountId,
                GDrivePollingType.Spreadsheets,
                true
            );
        }

        public override async Task Run()
        {
            var eventCrate = Payload.CratesOfType<EventReportCM>().FirstOrDefault()
                ?.Get<EventReportCM>()
                ?.EventPayload;

            KeyValueListCM changedFiles = null;
            if (eventCrate != null)
            {
                changedFiles = eventCrate
                    .CrateContentsOfType<KeyValueListCM>(x => x.Label == "ChangedFiles")
                    .SingleOrDefault();
            }

            if (changedFiles == null)
            {
                RequestPlanExecutionTermination("File list was not found in the payload.");
            }

            if (ActivityUI.AllSpreadsheetsOption.Selected)
            {
                var rows = new List<TableRowDTO>();
                foreach (var changedFile in changedFiles.Values)
                {
                    var row = new TableRowDTO();
                    row.Row.Add(TableCellDTO.Create(SpreadsheetIdLabel, changedFile.Key));
                    row.Row.Add(TableCellDTO.Create(SpreadsheetNameLabel, changedFile.Value));

                    rows.Add(row);
                }

                var tableData = new StandardTableDataCM(false, rows);

                Payload.Add(Crate.FromContent(RunTimeCrateLabel, tableData));
                Success();
            }
            else
            {
                var hasFileId = changedFiles.Values.Any(x => x.Key == ActivityUI.SpreadsheetList.Value);
                if (!hasFileId)
                {
                    RequestPlanExecutionTermination();
                }
                else
                {
                    var rows = new List<TableRowDTO>();
                    var changedFile = changedFiles.Values.Where(x => x.Key == ActivityUI.SpreadsheetList.Value).First();

                    var row = new TableRowDTO();
                    row.Row.Add(TableCellDTO.Create(SpreadsheetIdLabel, changedFile.Key));
                    row.Row.Add(TableCellDTO.Create(SpreadsheetNameLabel, changedFile.Value));
                    rows.Add(row);

                    var tableData = new StandardTableDataCM(false, rows);

                    Payload.Add(Crate.FromContent(RunTimeCrateLabel, tableData));
                    Success();
                }
            }
        }
    }
}