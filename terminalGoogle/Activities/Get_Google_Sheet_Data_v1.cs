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
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalGoogle.Actions
{
    public class Get_Google_Sheet_Data_v1 : EnhancedTerminalActivity<Get_Google_Sheet_Data_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public DropDownList SpreadsheetList { get; set; }

            public DropDownList WorksheetList { get; set; }

            public TextBlock ActivityDescription { get; set; }

            public ActivityUi() : this(false)
            {
            }

            public ActivityUi(bool includeWorksheetList)
            {
                SpreadsheetList = new DropDownList
                {
                    Label = "Select a Google Spreadsheet",
                    Name = nameof(SpreadsheetList),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Add(SpreadsheetList);
                if (includeWorksheetList)
                {
                    AddWorksheetList();
                }
                ActivityDescription = new TextBlock
                {
                    Name = nameof(ActivityDescription),
                };
                Controls.Add(ActivityDescription);
                UpdateDescription();
            }

            public void AddWorksheetList()
            {
                if (WorksheetList != null)
                {
                    return;
                }
                WorksheetList = new DropDownList
                {
                    Label = "(Optional) Select worksheet",
                    Name = nameof(WorksheetList),
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                };
                Controls.Insert(1, WorksheetList);
                UpdateDescription();
            }

            public void RemoveWorksheetList()
            {
                if (WorksheetList == null)
                {
                    return;
                }
                Controls.Remove(WorksheetList);
                WorksheetList = null;
                UpdateDescription();
            }

            private void UpdateDescription()
            {
                ActivityDescription.Value = $"This action will try to extract a table of rows from the {(WorksheetList == null ? "first" : "specified")} worksheet of the selected spreadsheet. The rows should have a header row";
            }

            public override void SyncWith(StandardConfigurationControlsCM configurationControls)
            {
                var worksheetList = (DropDownList)configurationControls.Controls.FirstOrDefault(x => x.Name == nameof(WorksheetList));
                if (worksheetList != null)
                {
                    AddWorksheetList();
                    WorksheetList.ListItems = worksheetList.ListItems.ToList();
                    WorksheetList.SelectByKey(worksheetList.selectedKey);
                }
                else
                {
                    RemoveWorksheetList();
                }
                base.SyncWith(configurationControls);
            }
        }

        private readonly IGoogleSheet _googleApi;

        private const string RunTimeCrateLabel = "Table Generated From Google Sheet Data";

        public Get_Google_Sheet_Data_v1()
           : base(true)
        {
            _googleApi = new GoogleSheet();
        }

        protected override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (base.NeedsAuthentication(authTokenDO))
            {
                return true;
            }
            var token = GetGoogleAuthToken(authTokenDO);
            // we may also post token to google api to check its validity
            return token.Expires - DateTime.Now < TimeSpan.FromMinutes(5) && string.IsNullOrEmpty(token.RefreshToken);
        }

        protected override IEnumerable<CrateDescriptionDTO> GetRuntimeAvailableCrateDescriptions(ConfigurationRequestType configurationType)
        {
            yield return new CrateDescriptionDTO
            {
                Label = RunTimeCrateLabel,
                ManifestId = (int)MT.StandardTableData,
                ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                ProducedBy = "Load data from Google spreadsheet"
            };
        }

        protected override async Task Initialize()
        {
            var spreadsheets = await _googleApi.GetSpreadsheetsAsync(GetGoogleAuthToken());
            ConfigurationControls.SpreadsheetList.ListItems = spreadsheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
        }

        protected override async Task Configure()
        {
            CurrentActivityStorage.RemoveByLabel(ColumnHeadersCrateLabel);
            var googleAuth = GetGoogleAuthToken();
            //If spreadsheet selection is cleared we remove worksheet DDLB from the controls
            if (string.IsNullOrEmpty(ConfigurationControls.SpreadsheetList.selectedKey))
            {
                ConfigurationControls.RemoveWorksheetList();
                SelectedSpreadsheet = null;
            }
            else
            {
                var previousValues = SelectedSpreadsheet;
                //Spreadsheet was changed - populate the list of worksheets and select first one
                if (previousValues == null || previousValues.Key != ConfigurationControls.SpreadsheetList.Value)
                {
                    var worksheets = await _googleApi.GetWorksheetsAsync(ConfigurationControls.SpreadsheetList.Value, googleAuth);
                    //We show worksheet list only if there is more than one worksheet
                    if (worksheets.Count > 1)
                    {
                        ConfigurationControls.AddWorksheetList();
                        ConfigurationControls.WorksheetList.ListItems = worksheets.Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
                        var firstWorksheet = ConfigurationControls.WorksheetList.ListItems.First();
                        ConfigurationControls.WorksheetList.selectedKey = firstWorksheet.Key;
                        ConfigurationControls.WorksheetList.Value = firstWorksheet.Value;
                        firstWorksheet.Selected = true;
                    }
                    else
                    {
                        ConfigurationControls.RemoveWorksheetList();
                    }
                }
                //Retrieving worksheet headers to make them avaialble for downstream activities
                var selectedSpreasheetWorksheet = new FieldDTO(ConfigurationControls.SpreadsheetList.Value, ConfigurationControls.WorksheetList == null
                                                                                                                ? string.Empty
                                                                                                                : ConfigurationControls.WorksheetList.Value);
                var columnHeaders = await _googleApi.GetWorksheetHeadersAsync(selectedSpreasheetWorksheet.Key, selectedSpreasheetWorksheet.Value, googleAuth);
                var columnHeadersCrate = Crate.FromContent(ColumnHeadersCrateLabel,
                                                           new FieldDescriptionsCM(columnHeaders.Select(x => new FieldDTO(x.Key, x.Key, AvailabilityType.RunTime))));
                CurrentActivityStorage.ReplaceByLabel(columnHeadersCrate);
                SelectedSpreadsheet = selectedSpreasheetWorksheet;
            }
        }

        private const string ConfigurationCrateLabel = "Selected Spreadsheet & Worksheet";

        private const string ColumnHeadersCrateLabel = "Spreadsheet Column Headers";
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
                var newValues = Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration);
                CurrentActivityStorage.ReplaceByLabel(newValues);
            }
        }

        protected override async Task RunCurrentActivity()
        {
            var selectedSpreadsheet = ConfigurationControls.SpreadsheetList.Value;
            if (string.IsNullOrEmpty(selectedSpreadsheet))
            {
                throw new ActivityExecutionException("Spreadsheet is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var selectedWorksheet = ConfigurationControls.WorksheetList == null ? string.Empty : ConfigurationControls.WorksheetList.Value;
            var data = await _googleApi.GetDataAsync(selectedSpreadsheet, selectedWorksheet, GetGoogleAuthToken());
            CurrentPayloadStorage.Add(Crate.FromContent(RunTimeCrateLabel, new StandardTableDataCM { Table = data.ToList() }));
        }

        private GoogleAuthDTO GetGoogleAuthToken(AuthorizationTokenDO authTokenDO = null)
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>((authTokenDO ?? AuthorizationToken).Token);
        }
    }
}