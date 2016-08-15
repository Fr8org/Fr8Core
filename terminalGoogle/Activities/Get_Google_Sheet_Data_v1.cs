using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using terminalGoogle.Actions;
using terminalGoogle.Interfaces;
using terminalUtilities;
using System;

namespace terminalGoogle.Activities
{
    public class Get_Google_Sheet_Data_v1 : BaseGoogleTerminalActivity<Get_Google_Sheet_Data_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("f389bea8-164c-42c8-bdc5-121d7fb93d73"),
            Name = "Get_Google_Sheet_Data",
            Label = "Get Google Sheet Data",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Tags = "Table Data Generator",
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.GooogleActivityCategoryDTO
            }
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

        private const string ConfigurationCrateLabel = "Selected Spreadsheet & Worksheet";

        private readonly IGoogleSheet _googleApi;
        private readonly IGoogleIntegration _googleIntegration;

        public Get_Google_Sheet_Data_v1(ICrateManager crateManager, IGoogleIntegration googleIntegration, IGoogleSheet googleSheet)
            : base(crateManager, googleIntegration)
        {
            _googleApi = googleSheet;
        }
        //This property is used to store and retrieve user-selected spreadsheet and worksheet between configuration responses 
        //to avoid extra fetch from Google
        private KeyValueDTO StoredSelectedSheet
        {
            get
            {
                var storedValues = Storage.FirstCrateOrDefault<KeyValueListCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Values.First();

            }
            set
            {
                if (value == null)
                {
                    Storage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
         
                var newValues = Crate.FromContent(ConfigurationCrateLabel, new KeyValueListCM(value));
                Storage.ReplaceByLabel(newValues);
            }
        }



        public override async Task Initialize()
        {
            var spreadsheets = await _googleApi.GetSpreadsheets(GetGoogleAuthToken());
            ActivityUI.SpreadsheetList.ListItems = spreadsheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
        }

        public override async Task FollowUp()
        {
            CrateSignaller.ClearAvailableCrates();

            List<Crate> crates = new List<Crate>();
            var googleAuth = GetGoogleAuthToken();
            var spreadsheets = await _googleApi.GetSpreadsheets(googleAuth);
            ActivityUI.SpreadsheetList.ListItems = spreadsheets
                .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                .ToList();

            var selectedSpreadsheet = ActivityUI.SpreadsheetList.selectedKey;
            if (!string.IsNullOrEmpty(selectedSpreadsheet))
            {
                //This chunk here is about removing a selection if a spreadsheet was deleted but at the same time it was chosen in UI
                if (ActivityUI.SpreadsheetList.ListItems.All(x => x.Key != selectedSpreadsheet))
                {
                    ActivityUI.SpreadsheetList.selectedKey = null;
                    ActivityUI.SpreadsheetList.Value = null;
                }
            }

            //If spreadsheet selection is cleared we hide worksheet DDLB
            if (string.IsNullOrEmpty(ActivityUI.SpreadsheetList.selectedKey))
            {
                ActivityUI.HideWorksheetList();
                StoredSelectedSheet = null;
            }
            else
            {
                var previousValues = StoredSelectedSheet;
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
                var selectedSpreasheetWorksheet = new KeyValueDTO(ActivityUI.SpreadsheetList.Value,
                                                               ActivityUI.WorksheetList.IsHidden
                                                                   ? string.Empty
                                                                   : ActivityUI.WorksheetList.Value);
                var columnHeaders = await _googleApi.GetWorksheetHeaders(selectedSpreasheetWorksheet.Key, selectedSpreasheetWorksheet.Value, googleAuth);
                
                StoredSelectedSheet = selectedSpreasheetWorksheet;
                var table = await GetSelectedSpreadSheet();
                var hasHeaderRow = TryAddHeaderRow(table); 
                Storage.ReplaceByLabel(Crate.FromContent(GetRuntimeCrateLabel(), new StandardTableDataCM { Table = table, FirstRowHeaders = hasHeaderRow }));

                CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(GetRuntimeCrateLabel(), true)
                    .AddFields(columnHeaders.Select(x => new FieldDTO(x.Key)));

                //here was logic responsible for handling one-row tables but it was faulty. It's main purpose was to spawn fields like "value immediatly below of" in a StandardPayload. 
                //You might view TabularUtilities.PrepareFieldsForOneRowTable for reference
            }
        }

        private string GetRuntimeCrateLabel()
                {
            return string.Format("Spreadsheet Data from \"{0}\"",
                ((!ActivityUI.WorksheetList.IsHidden && !string.IsNullOrEmpty(ActivityUI.WorksheetList.selectedKey))
                ? ActivityUI.SpreadsheetList.selectedKey + "-" + ActivityUI.WorksheetList.selectedKey
                : ActivityUI.SpreadsheetList.selectedKey));
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
                                .Row.Select(x => new TableCellDTO { Cell = new KeyValueDTO(x.Cell.Key, x.Cell.Key) })
                                .ToList()
                    });

            return true;
        }

        public override async Task Run()
        {
            if (string.IsNullOrEmpty(ActivityUI.SpreadsheetList.Value))
            {
                RaiseError("Spreadsheet is not selected",
                    ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }
           
            var table = await GetSelectedSpreadSheet();
            var hasHeaderRow = TryAddHeaderRow(table);
            Payload.Add(Crate.FromContent(GetRuntimeCrateLabel(), new StandardTableDataCM { Table = table, FirstRowHeaders = hasHeaderRow }));
        }
    }
}
