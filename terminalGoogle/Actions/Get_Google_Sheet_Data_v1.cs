using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalGoogle.Actions
{
    public class Get_Google_Sheet_Data_v1 : BaseTerminalActivity
    {
        private readonly IGoogleSheet _google;

        public Get_Google_Sheet_Data_v1()
        {
            _google = new GoogleSheet();
        }

        protected bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (authTokenDO == null) return true;
            if (!base.NeedsAuthentication(authTokenDO))
                return false;
            var token = JsonConvert.DeserializeObject<GoogleAuthDTO>(
                authTokenDO.Token);
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

            return await CreateStandardPayloadDataFromStandardTableData(curActivityDO, containerId, payloadCrates, authTokenDO);
        }

        private async Task<PayloadDTO> CreateStandardPayloadDataFromStandardTableData(ActivityDO curActivityDO, Guid containerId, PayloadDTO payloadCrates, AuthorizationTokenDO authTokenDO)
        {
            //at run time pull the entire sheet and store it as payload
            var spreadsheetControl = FindControl(Crate.GetStorage(curActivityDO.CrateStorage), "select_spreadsheet");
            var spreadsheetsFromUserSelection = string.Empty;
            if (spreadsheetControl != null)
                spreadsheetsFromUserSelection = spreadsheetControl.Value;
            
            if (!string.IsNullOrEmpty(spreadsheetsFromUserSelection))
            {
                curActivityDO = TransformSpreadsheetDataToPayloadDataCrate(curActivityDO, authTokenDO, spreadsheetsFromUserSelection);
            }

            var tableDataMS = await GetTargetTableData(curActivityDO);

            // Create a crate of payload data by using Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            // Add a crate of PayloadData to action's crate storage
            var payloadDataCrate = Crate.CreatePayloadDataCrate("ExcelTableRow", "Excel Data", tableDataMS);

            using (var updater = Crate.UpdateStorage(payloadCrates))
            {
                updater.CrateStorage.Add(payloadDataCrate);
            }


            return Success(payloadCrates);
        }

        private async Task<StandardTableDataCM> GetTargetTableData(ActivityDO curActivityDO)
        {
            // Find crates of manifest type Standard Table Data
            var storage = Crate.GetStorage(curActivityDO);
            var standardTableDataCrates = storage.CratesOfType<StandardTableDataCM>().ToArray();

            // If no crate of manifest type "Standard Table Data" found, try to find upstream for a crate of type "Standard File Handle"
            if (!standardTableDataCrates.Any())
            {
                return await GetUpstreamTableData(curActivityDO);
            }

            return standardTableDataCrates.First().Content;
        }

        private async Task<StandardTableDataCM> GetUpstreamTableData(ActivityDO curActivityDO)
        {
            var upstreamFileHandleCrates = await GetUpstreamFileHandleCrates(curActivityDO);

            //if no "Standard File Handle" crate found then return
            if (!upstreamFileHandleCrates.Any())
                throw new Exception("No Standard File Handle crate found in upstream.");

            //if more than one "Standard File Handle" crates found then throw an exception
            if (upstreamFileHandleCrates.Count > 1)
                throw new Exception("More than one Standard File Handle crates found upstream.");

            throw new NotImplementedException();
            // Deserialize the Standard File Handle crate to StandardFileHandleMS object
            //StandardFileHandleMS fileHandleMS = JsonConvert.DeserializeObject<StandardFileHandleMS>(upstreamFileHandleCrates.First().Contents);

            // Use the url for file from StandardFileHandleMS and read from the file and transform the data into StandardTableData and assign it to Action's crate storage
            //StandardTableDataCM tableDataMS = ExcelUtils.GetTableData(fileHandleMS.DockyardStorageUrl);

            //return tableDataMS;
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

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
            }

            return Task.FromResult(curActivityDO);
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            var spreadsheetsFromUserSelectionControl = FindControl(Crate.GetStorage(curActivityDO.CrateStorage), "select_spreadsheet");

            var hasDesignTimeFields = Crate.GetStorage(curActivityDO)
                .Any(x => x.IsOfType<StandardConfigurationControlsCM>());

            if (hasDesignTimeFields && !string.IsNullOrEmpty(spreadsheetsFromUserSelectionControl.Value))
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var spreadsheetsFromUserSelection =
                Activity.GetControlsManifest(curActivityDO).FindByName("select_spreadsheet").Value;

            // Creating configuration control crate with a file picker and textblock
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
            var configControlsCrate = CreateConfigurationControlsCrate(spreadsheets, spreadsheetsFromUserSelection);
            // Remove previously created configuration control crate
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();

                updater.CrateStorage.Add(configControlsCrate);
            }

            if (!string.IsNullOrEmpty(spreadsheetsFromUserSelection))
            {
                return TransformSpreadsheetDataToStandardTableDataCrate(curActivityDO, authTokenDO, spreadsheetsFromUserSelection);
            }
            else
            {
                return curActivityDO;
            }
        }

        private ActivityDO TransformSpreadsheetDataToStandardTableDataCrate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, string spreadsheetUri)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // Fetch column headers in Excel file and assign them to the action's crate storage as Design TIme Fields crate
            var headers = _google.EnumerateColumnHeaders(spreadsheetUri, authDTO);
            if (headers.Any())
            {
                using (var updater = Crate.UpdateStorage(curActivityDO))
                {
                    const string label = "Spreadsheet Column Headers";
                    updater.CrateStorage.RemoveByLabel(label);
                    var curCrateDTO = Crate.CreateDesignTimeFieldsCrate(
                                label,
                                headers.Select(col => new FieldDTO() { Key = col.Key, Value = col.Key, Availability = Data.States.AvailabilityType.RunTime }).ToArray()
                            );
                    updater.CrateStorage.Add(curCrateDTO);
                }
            }

            CreatePayloadCrate_SpreadsheetRows(curActivityDO, spreadsheetUri, authDTO, headers);

            return curActivityDO;
        }

        private ActivityDO TransformSpreadsheetDataToPayloadDataCrate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, string spreadsheetUri)
        {
            var rows = new List<TableRowDTO>();
       
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var extractedData = _google.EnumerateDataRows(spreadsheetUri, authDTO);
            if (extractedData == null) return curActivityDO;

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                const string label = "Spreadsheet Payload Rows";
                updater.CrateStorage.RemoveByLabel(label);
                updater.CrateStorage.Add(Crate.CreateStandardTableDataCrate(label, false, extractedData.ToArray()));
            }

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

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                const string label = "Spreadsheet Payload Rows";
                updater.CrateStorage.RemoveByLabel(label);
                updater.CrateStorage.Add(Crate.CreateStandardTableDataCrate(label, false, rows.ToArray()));
            }
        }
    }
}