using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using StructureMap;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using TerminalBase.BaseClasses;
using Google.GData.Client;

namespace terminalGoogle.Actions
{
    public class Get_Google_Sheet_Data_v1 : BaseGoogleTerminalActivity<Get_Google_Sheet_Data_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SpreadsheetList { get; set; }

            public DropDownList WorksheetList { get; set; }

            public TextBlock ActivityDescription { get; set; }
            public ActivityUi()
            {
                SpreadsheetList = new DropDownList
                {
                    Label = "Select a Google Spreadsheet",
                    Name = nameof(SpreadsheetList),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Add(SpreadsheetList);
                WorksheetList = new DropDownList
                {
                    Label = "Select worksheet",
                    Name = nameof(WorksheetList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Add(WorksheetList);
                ActivityDescription = new TextBlock
                {
                    Name = nameof(ActivityDescription),
                };
                Controls.Add(ActivityDescription);
                HideWorksheetList();
            }

            public void ShowWorksheetList()
            {
                WorksheetList.IsHidden = false;
                UpdateDescription();
            }

            public void HideWorksheetList()
            {
                WorksheetList.IsHidden = true;
                UpdateDescription();
            }

            private void UpdateDescription()
            {
                ActivityDescription.Value = $"This action will try to extract a table of rows from the {(WorksheetList.IsHidden ? "first" : "specified")} worksheet of the selected spreadsheet. The rows should have a header row";
            }
        }

        private const string RunTimeCrateLabel = "Table Generated From Google Sheet Data";

        private const string ConfigurationCrateLabel = "Selected Spreadsheet & Worksheet";

        private const string ColumnHeadersCrateLabel = "Spreadsheet Column Headers";

        private readonly IGoogleSheet _googleApi;

        public Get_Google_Sheet_Data_v1()
        {
            _googleApi = ObjectFactory.GetInstance<IGoogleSheet>();
        }
        //This property is used to store and retrieve user-selected spreadsheet and worksheet between configuration responses 
        //to avoid extra fetch from Google
        private FieldDTO SelectedSpreadsheet
        {
            get
            {
                var storedValues = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Fields.First();
            }
            set
            {
                if (value == null)
                {
                    CurrentActivityStorage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
                value.Availability = AvailabilityType.Configuration;
                var newValues = Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration);
                CurrentActivityStorage.ReplaceByLabel(newValues);
            }
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            var spreadsheets = await _googleApi.GetSpreadsheets(GetGoogleAuthToken());
            ConfigurationControls.SpreadsheetList.ListItems = spreadsheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();

            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            var googleAuth = GetGoogleAuthToken();
            var spreadsheets = await _googleApi.GetSpreadsheets(googleAuth);
            ConfigurationControls.SpreadsheetList.ListItems = spreadsheets
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();

            var selectedSpreadsheet = ConfigurationControls.SpreadsheetList.selectedKey;
            if (!string.IsNullOrEmpty(selectedSpreadsheet))
            {
                if (!ConfigurationControls.SpreadsheetList.ListItems.Any(x => x.Key == selectedSpreadsheet))
                {
                    ConfigurationControls.SpreadsheetList.selectedKey = null;
                    ConfigurationControls.SpreadsheetList.Value = null;
                }
            }

            CurrentActivityStorage.RemoveByLabel(ColumnHeadersCrateLabel);
            //If spreadsheet selection is cleared we hide worksheet DDLB
            if (string.IsNullOrEmpty(ConfigurationControls.SpreadsheetList.selectedKey))
            {
                ConfigurationControls.HideWorksheetList();
                SelectedSpreadsheet = null;
            }
            else
            {
                var previousValues = SelectedSpreadsheet;
                //Spreadsheet was changed - populate the list of worksheets and select first one
                if (previousValues == null || previousValues.Key != ConfigurationControls.SpreadsheetList.Value)
                {
                    var worksheets = await _googleApi.GetWorksheets(ConfigurationControls.SpreadsheetList.Value, googleAuth);
                    //We show worksheet list only if there is more than one worksheet
                    if (worksheets.Count > 1)
                    {
                        ConfigurationControls.ShowWorksheetList();
                        ConfigurationControls.WorksheetList.ListItems = worksheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
                        var firstWorksheet = ConfigurationControls.WorksheetList.ListItems.First();
                        ConfigurationControls.WorksheetList.SelectByKey(firstWorksheet.Key);
                    }
                    else
                    {
                        ConfigurationControls.HideWorksheetList();
                    }
                }
                //Retrieving worksheet headers to make them avaialble for downstream activities
                var selectedSpreasheetWorksheet = new FieldDTO(ConfigurationControls.SpreadsheetList.Value,
                                                               ConfigurationControls.WorksheetList.IsHidden
                                                                   ? string.Empty
                                                                   : ConfigurationControls.WorksheetList.Value);
                var columnHeaders = await _googleApi.GetWorksheetHeaders(selectedSpreasheetWorksheet.Key, selectedSpreasheetWorksheet.Value, googleAuth);
                var columnHeadersCrate = Crate.FromContent(ColumnHeadersCrateLabel,
                                                           new FieldDescriptionsCM(columnHeaders.Select(x => new FieldDTO(x.Key, x.Key, AvailabilityType.Always))),
                                                           AvailabilityType.Always);
                CurrentActivityStorage.ReplaceByLabel(columnHeadersCrate);
                SelectedSpreadsheet = selectedSpreasheetWorksheet;
            }
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task RunCurrentActivity()
        {
            var selectedSpreadsheet = ConfigurationControls.SpreadsheetList.Value;
            if (string.IsNullOrEmpty(selectedSpreadsheet))
            {
                throw new ActivityExecutionException("Spreadsheet is not selected",
                    ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var selectedWorksheet = ConfigurationControls.WorksheetList == null
                ? string.Empty
                : ConfigurationControls.WorksheetList.Value;
            var data = (await _googleApi.GetData(selectedSpreadsheet, selectedWorksheet, GetGoogleAuthToken())).ToList();
            var hasHeaderRow = false;
            //Adding header row if possible
            if (data.Count > 0)
            {
                data.Insert(0,
                    new TableRowDTO
                    {
                        Row =
                            data.First()
                                .Row.Select(x => new TableCellDTO { Cell = new FieldDTO(x.Cell.Key, x.Cell.Key) })
                                .ToList()
                    });
                hasHeaderRow = true;
            }
            CurrentPayloadStorage.Add(Crate.FromContent(RunTimeCrateLabel,
                new StandardTableDataCM { Table = data, FirstRowHeaders = hasHeaderRow }));
        }
        public override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (base.NeedsAuthentication(authTokenDO))
            {
                return true;
            }
            var token = GetGoogleAuthToken(authTokenDO);
            // we may also post token to google api to check its validity
            return token.Expires - DateTime.Now < TimeSpan.FromMinutes(5) && string.IsNullOrEmpty(token.RefreshToken);
        }
    }
}
