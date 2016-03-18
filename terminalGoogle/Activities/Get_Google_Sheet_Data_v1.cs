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
using Hub.Managers;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalGoogle.Actions
{
    public class Get_Google_Sheet_Data_v1 : BaseTerminalActivity
    {
        private readonly IGoogleSheet _google;
        private const string TableCrateLabelPrefix = "Data from ";
        private const string RunTimeCrateLabel = "Table Generated From Google Sheet Data";
        public Get_Google_Sheet_Data_v1()
        {
            _google = new GoogleSheet();
        }

        protected override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (authTokenDO == null) return true;
            if (!base.NeedsAuthentication(authTokenDO))
                return false;
            var token = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // we may also post token to google api to check its validity
            return (token.Expires - DateTime.Now > TimeSpan.FromMinutes(5) ||
                    !string.IsNullOrEmpty(token.RefreshToken));
        }

        public override Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return base.Configure(curActivityDO, authTokenDO);
        }


        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            ///// ********** This code is what have to be done by FR-2246 **************
            var dropDownListControl =
                (DropDownList)GetControlsManifest(curActivityDO).FindByName("select_spreadsheet");
            //get the spreadsheet name
            var spreadsheetName = dropDownListControl.selectedKey;
            //get the link to spreadsheet
            var spreadsheetsFromUserSelection = dropDownListControl.Value;
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            //get the data
            var data = _google.EnumerateDataRows(spreadsheetsFromUserSelection, authDTO);
            var crate = CrateManager.CreateStandardTableDataCrate(RunTimeCrateLabel, true, data.ToArray());
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(crate);
            }

            return Success(payloadCrates);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private Crate CreateConfigurationControlsCrate(IDictionary<string, string> spreadsheets, string selectedSpreadsheet = null)
        {
            var controlList = new List<ControlDefinitionDTO>();

            var spreadsheetControl = new DropDownList()
            {
                Label = "Select a Google Spreadsheet",
                Name = "select_spreadsheet",
                Required = true,
                Events = new List<ControlEvent>() { ControlEvent.RequestConfig },
                Source = new FieldSourceDTO
                {
                    Label = "Select a Google Spreadsheet",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
                ListItems = spreadsheets
                    .Select(pair => new ListItem()
                    {
                        Key = pair.Value,
                        Value = pair.Key,
                        Selected = string.Equals(pair.Key, selectedSpreadsheet, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList(),
                selectedKey = spreadsheets.FirstOrDefault(a => a.Key == selectedSpreadsheet).Value,
                Value = selectedSpreadsheet
            };
            controlList.Add(spreadsheetControl);

            var textBlockControlField = GenerateTextBlock("",
                "This Action will try to extract a table of rows from the first worksheet in the selected spreadsheet. The rows should have a header row.",
                "well well-lg TextBlockClass");
            controlList.Add(textBlockControlField);
            return PackControlsCrate(controlList.ToArray());
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {

            //build a controls crate to render the pane
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
            var configurationControlsCrate = CreateConfigurationControlsCrate(spreadsheets);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                crateStorage.Add(GetAvailableRunTimeTableCrate(RunTimeCrateLabel));
            }

            return Task.FromResult(curActivityDO);
        }

        private Crate GetAvailableRunTimeTableCrate(string descriptionLabel)
        {
            var availableRunTimeCrates = Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                        Label = descriptionLabel,
                        ManifestId = (int)MT.StandardTableData,
                        ProducedBy = "Get_Google_Sheet_Data_v1"
                    }), AvailabilityType.RunTime);
            return availableRunTimeCrates;
        }

/// <summary>
/// If there's a value in select_file field of the crate, then it is a followup call.
/// </summary>
public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            var spreadsheetsFromUserSelectionControl = FindControl(CrateManager.GetStorage(curActivityDO.CrateStorage), "select_spreadsheet");

            var hasDesignTimeFields = CrateManager.GetStorage(curActivityDO)
                .Any(x => x.IsOfType<StandardConfigurationControlsCM>());

            if (hasDesignTimeFields && !string.IsNullOrEmpty(spreadsheetsFromUserSelectionControl.Value))
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        //protected override IEnumerable<CrateDescriptionDTO> GetRuntimeAvailableCrateDescriptions(ActivityDO curActivityDO)
        //{
        //    yield return new CrateDescriptionDTO
        //                 {
        //                     ManifestType = MT.StandardTableData.GetEnumDisplayName(),
        //                     Label = TableCrateLabelPrefix + GetSelectSpreadsheetName(curActivityDO),
        //                     ManifestId = (int)MT.StandardTableData,
        //                     ProducedBy = nameof(Get_Google_Sheet_Data_v1)
        //                 };
        //}

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var spreadsheetsFromUserSelection =
                GetControlsManifest(curActivityDO).FindByName("select_spreadsheet").Value;
            //Create label based on the selected by user spreadsheet name
            var selectedSpreadsheetName = GetSelectSpreadsheetName(curActivityDO);
            // Creating configuration control crate with a file picker and textblock
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
            var configControlsCrate = CreateConfigurationControlsCrate(spreadsheets, spreadsheetsFromUserSelection);

            // Remove previously created configuration control crate
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Add(configControlsCrate);
            }

            return !string.IsNullOrEmpty(spreadsheetsFromUserSelection) 
                ? TransformSpreadsheetDataToStandardTableDataCrate(curActivityDO, authTokenDO, spreadsheetsFromUserSelection) 
                : curActivityDO;
        }

        private ActivityDO TransformSpreadsheetDataToStandardTableDataCrate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, string spreadsheetUri)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // Fetch column headers in Excel file and assign them to the action's crate storage as Design TIme Fields crate
            var headers = _google.EnumerateColumnHeaders(spreadsheetUri, authDTO);
            if (headers.Any())
            {
                using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                {
                    const string label = "Spreadsheet Column Headers";
                    crateStorage.RemoveByLabel(label);
                    var curCrateDTO = CrateManager.CreateDesignTimeFieldsCrate(
                                label,
                                headers.Select(col => new FieldDTO() { Key = col.Key, Value = col.Key, Availability = Data.States.AvailabilityType.RunTime }).ToArray()
                            );
                    crateStorage.Add(curCrateDTO);
                }
            }

            CreatePayloadCrate_SpreadsheetRows(curActivityDO, spreadsheetUri, authDTO, headers);

            return curActivityDO;
        }

        private void CreatePayloadCrate_SpreadsheetRows(ActivityDO curActivityDO, string spreadsheetUri, GoogleAuthDTO authDTO, IDictionary<string, string> headers)
        {
            // To fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate. 
            // This functionality is commented due to performance issue. 
            // To fetch rows in excel file, please uncomment below line of code.
            // var rows = _google.EnumerateDataRows(spreadsheetUri, authDTO);

            var rows = new List<TableRowDTO>();
            var headerTableRowDTO = new TableRowDTO() { Row = new List<TableCellDTO>(), };

            foreach (var header in headers)
            {
                var tableCellDTO = TableCellDTO.Create(header.Key, header.Value);
                headerTableRowDTO.Row.Add(tableCellDTO);
            }

            rows.Add(headerTableRowDTO);

            //var selectedSpreadsheetName = GetSelectSpreadsheetName(curActivityDO);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                //crateStorage.RemoveByLabelPrefix(TableCrateLabelPrefix);
                crateStorage.RemoveByLabel("Available Run Time Crates");
                crateStorage.Add(CrateManager.CreateStandardTableDataCrate(RunTimeCrateLabel, false, rows.ToArray()));
            }
        }

        private string GetSelectSpreadsheetName(ActivityDO curActivityDO)
        {
            var dropDownListControl = (DropDownList)GetControlsManifest(curActivityDO).FindByName("select_spreadsheet");
            //get the spreadsheet name
            var spreadsheetName = dropDownListControl.selectedKey;
            return spreadsheetName;
        }
    }
}