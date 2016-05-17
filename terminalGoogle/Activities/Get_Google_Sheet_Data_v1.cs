using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using StructureMap;
using terminalGoogle.Actions;
using terminalGoogle.Interfaces;
using terminalUtilities;

namespace terminalGoogle.Activities
{
    public class Get_Google_Sheet_Data_v1 : BaseGoogleTerminalActivity<Get_Google_Sheet_Data_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Get_Google_Sheet_Data",
            Label = "Get Google Sheet Data",
            Version = "1",
            Category = ActivityCategory.Receivers,
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO,
            Tags = "Table Data Generator"
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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
        private readonly IGoogleIntegration _googleIntegration;

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
                var storedValues = Storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Fields.First();

            }
            set
            {
                if (value == null)
                {
                    Storage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
                value.Availability = AvailabilityType.Configuration;
                var newValues = Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration);
                Storage.ReplaceByLabel(newValues);
            }
        }

        

        protected override async Task InitializeETA()
        {
            var spreadsheets = await _googleApi.GetSpreadsheets(GetGoogleAuthToken());
            ActivityUI.SpreadsheetList.ListItems = spreadsheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();

            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task ConfigureETA()
        {
            List<Crate> crates = new List<Crate>();
            Crate fieldsCrate = null;
            var googleAuth = GetGoogleAuthToken();
            var spreadsheets = await _googleApi.GetSpreadsheets(googleAuth);
            ActivityUI.SpreadsheetList.ListItems = spreadsheets
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();

            var selectedSpreadsheet = ActivityUI.SpreadsheetList.selectedKey;
            if (!string.IsNullOrEmpty(selectedSpreadsheet))
            {
                if (ActivityUI.SpreadsheetList.ListItems.All(x => x.Key != selectedSpreadsheet))
                {
                    ActivityUI.SpreadsheetList.selectedKey = null;
                    ActivityUI.SpreadsheetList.Value = null;
                }
            }

            Storage.RemoveByLabel(ColumnHeadersCrateLabel);
            //If spreadsheet selection is cleared we hide worksheet DDLB
            if (string.IsNullOrEmpty(ActivityUI.SpreadsheetList.selectedKey))
            {
                ActivityUI.HideWorksheetList();
                SelectedSpreadsheet = null;
            }
            else
            {
                var previousValues = SelectedSpreadsheet;
                //Spreadsheet was changed - populate the list of worksheets and select first one
                if (previousValues == null || previousValues.Key != ActivityUI.SpreadsheetList.Value)
                {
                    var worksheets = await _googleApi.GetWorksheets(ActivityUI.SpreadsheetList.Value, googleAuth);
                    //We show worksheet list only if there is more than one worksheet
                    if (worksheets.Count > 1)
                    {
                        ActivityUI.ShowWorksheetList();
                        ActivityUI.WorksheetList.ListItems = worksheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
                        var firstWorksheet = ActivityUI.WorksheetList.ListItems.First();
                        ActivityUI.WorksheetList.SelectByKey(firstWorksheet.Key);
                    }
                    else
                    {
                        ActivityUI.HideWorksheetList();
                    }
                }
                //Retrieving worksheet headers to make them avaialble for downstream activities
                var selectedSpreasheetWorksheet = new FieldDTO(ActivityUI.SpreadsheetList.Value,
                                                               ActivityUI.WorksheetList.IsHidden
                                                                   ? string.Empty
                                                                   : ActivityUI.WorksheetList.Value);
                var columnHeaders = await _googleApi.GetWorksheetHeaders(selectedSpreasheetWorksheet.Key, selectedSpreasheetWorksheet.Value, googleAuth);
                var columnHeadersCrate = Crate.FromContent(ColumnHeadersCrateLabel,
                                                           new FieldDescriptionsCM(columnHeaders.Select(x => new FieldDTO(x.Key, x.Key, AvailabilityType.Always) { SourceCrateLabel = RunTimeCrateLabel})),
                                                           AvailabilityType.Always);
                Storage.ReplaceByLabel(columnHeadersCrate);
                SelectedSpreadsheet = selectedSpreasheetWorksheet;

                var table = await GetSelectedSpreadSheet();
                var hasHeaderRow = TryAddHeaderRow(table);
                Storage.ReplaceByLabel(Crate.FromContent(RunTimeCrateLabel,new StandardTableDataCM { Table = table, FirstRowHeaders = hasHeaderRow }));

                if (table?.Count() > 0)
                {
                    fieldsCrate = TabularUtilities.PrepareFieldsForOneRowTable(hasHeaderRow, false, table, columnHeaders.Select(ch => ch.Key).ToList());
                }

                if (fieldsCrate != null)
                {
                    Storage.ReplaceByLabel(fieldsCrate);
                }
                else
                {
                    Storage.RemoveByLabel(TabularUtilities.ExtractedFieldsCrateLabel);
                }
            }
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        private async Task<List<TableRowDTO>> GetSelectedSpreadSheet()
        {
            var selectedSpreadsheet = ActivityUI.SpreadsheetList.Value;
            if (string.IsNullOrEmpty(selectedSpreadsheet))
            {
                return new List<TableRowDTO>();
            }
            var selectedWorksheet = ActivityUI.WorksheetList == null
                ? string.Empty
                : ActivityUI.WorksheetList.Value;
            return (await _googleApi.GetData(selectedSpreadsheet, selectedWorksheet, GetGoogleAuthToken())).ToList();
        }

        private bool TryAddHeaderRow(List<TableRowDTO> table)
        {
            if (table.Count < 1)
            {
                return false;
            }
            table.Insert(0,
                    new TableRowDTO
                    {
                        Row =
                            table.First()
                                .Row.Select(x => new TableCellDTO { Cell = new FieldDTO(x.Cell.Key, x.Cell.Key) })
                                .ToList()
                    });

            return true;
        }

        protected override async Task RunETA()
        {
            if (string.IsNullOrEmpty(ActivityUI.SpreadsheetList.Value))
            {
                RaiseError("Spreadsheet is not selected",
                    ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
           
            var table = await GetSelectedSpreadSheet();
            var hasHeaderRow = TryAddHeaderRow(table);
            Payload.Add(Crate.FromContent(RunTimeCrateLabel, new StandardTableDataCM { Table = table, FirstRowHeaders = hasHeaderRow }));

            var fieldsCrate = TabularUtilities.PrepareFieldsForOneRowTable(hasHeaderRow, true, table, null); // assumes that hasHeaderRow is always true
            if (fieldsCrate != null)
            {
                Payload.ReplaceByLabel(fieldsCrate);
            }
        }
    }
}
