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
    public class Get_Google_Sheet_Data_v1 : BaseTerminalAction
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
            var token = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // we may also post token to google api to check its validity
            return (token.Expires - DateTime.Now > TimeSpan.FromMinutes(5) ||
                    !string.IsNullOrEmpty(token.RefreshToken));
        }

        public override Task<ActionDO> Configure(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return base.Configure(curActionDO, authTokenDO);
        }


        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return await CreateStandardPayloadDataFromStandardTableData(curActionDO, containerId);
        }

        private async Task<PayloadDTO> CreateStandardPayloadDataFromStandardTableData(ActionDO curActionDO, Guid containerId)
        {
            var processPayload = await GetProcessPayload(curActionDO, containerId);

            var tableDataMS = await GetTargetTableData(curActionDO);

            // Create a crate of payload data by using Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            // Add a crate of PayloadData to action's crate storage
            var payloadDataCrate = Crate.CreatePayloadDataCrate("ExcelTableRow", "Excel Data", tableDataMS);

            using (var updater = Crate.UpdateStorage(processPayload))
            {
                updater.CrateStorage.Add(payloadDataCrate);
            }

            return processPayload;
        }

        private async Task<StandardTableDataCM> GetTargetTableData(ActionDO curActionDO)
        {
            // Find crates of manifest type Standard Table Data
            var storage = Crate.GetStorage(curActionDO);
            var standardTableDataCrates = storage.CratesOfType<StandardTableDataCM>().ToArray();

            // If no crate of manifest type "Standard Table Data" found, try to find upstream for a crate of type "Standard File Handle"
            if (!standardTableDataCrates.Any())
            {
                return await GetUpstreamTableData(curActionDO);
            }

            return standardTableDataCrates.First().Content;
        }

        private async Task<StandardTableDataCM> GetUpstreamTableData(ActionDO curActionDO)
        {
            var upstreamFileHandleCrates = await GetUpstreamFileHandleCrates(curActionDO);

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
                Events = new List<ControlEvent>()
                {
                    new ControlEvent("onChange", "requestConfig")
                },
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
                selectedKey = spreadsheets.Where(a => a.Key == selectedSpreadsheet).FirstOrDefault().Value,
                Value = selectedSpreadsheet
            };
            controlList.Add(spreadsheetControl);

            var textBlockControlField = new TextBlock()
            {
                Label = "",
                Value = "This Action will try to extract a table of rows from the first worksheet in the selected spreadsheet. The rows should have a header row.",
                CssClass = "well well-lg TextBlockClass"
            };
            controlList.Add(textBlockControlField);
            return PackControlsCrate(controlList.ToArray());
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActionDO.Id != Guid.Empty)
            {
                //build a controls crate to render the pane
                var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
                var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
                var configurationControlsCrate = CreateConfigurationControlsCrate(spreadsheets);

                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    updater.CrateStorage = AssembleCrateStorage(configurationControlsCrate);
                }
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId.");
            }

            return Task.FromResult(curActionDO);
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            var spreadsheetsFromUserSelectionControl = FindControl(Crate.GetStorage(curActionDO.CrateStorage), "select_spreadsheet");

            var hasDesignTimeFields = Crate.GetStorage(curActionDO)
                .Any(x => x.IsOfType<StandardConfigurationControlsCM>());

            if (hasDesignTimeFields && !string.IsNullOrEmpty(spreadsheetsFromUserSelectionControl.Value))
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var spreadsheetsFromUserSelection =
                Action.GetControlsManifest(curActionDO).FindByName("select_spreadsheet").Value;

            // Creating configuration control crate with a file picker and textblock
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
            var configControlsCrate = CreateConfigurationControlsCrate(spreadsheets, spreadsheetsFromUserSelection);
            // Remove previously created configuration control crate
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();

                updater.CrateStorage.Add(configControlsCrate);
            }

            if (!string.IsNullOrEmpty(spreadsheetsFromUserSelection))
            {
                return await TransformSpreadsheetDataToStandardTableDataCrate(curActionDO, authTokenDO, spreadsheetsFromUserSelection);
            }
            else
            {
                return curActionDO;
            }
        }

        private async Task<ActionDO> TransformSpreadsheetDataToStandardTableDataCrate(ActionDO curActionDO, AuthorizationTokenDO authTokenDO, string spreadsheetUri)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // Fetch column headers in Excel file and assign them to the action's crate storage as Design TIme Fields crate
            var headers = _google.EnumerateColumnHeaders(spreadsheetUri, authDTO);
            if (headers.Any())
            {
                using (var updater = Crate.UpdateStorage(curActionDO))
                {
                    const string label = "Spreadsheet Column Headers";
                    updater.CrateStorage.RemoveByLabel(label);
                    var curCrateDTO = Crate.CreateDesignTimeFieldsCrate(
                                label,
                                headers.Select(col => new FieldDTO() { Key = col.Key, Value = col.Key }).ToArray()
                            );
                    updater.CrateStorage.Add(curCrateDTO);
                }
            }

            CreatePayloadCrate_SpreadsheetRows(curActionDO, spreadsheetUri, authDTO, headers);

            return curActionDO;
        }

        private void CreatePayloadCrate_SpreadsheetRows(ActionDO curActionDO, string spreadsheetUri, GoogleAuthDTO authDTO, IDictionary<string, string> headers)
        {
            // Fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate
            var rows = _google.EnumerateDataRows(spreadsheetUri, authDTO);
            var headerTableRowDTO = new TableRowDTO() { Row = new List<TableCellDTO>(), };
            foreach (var header in headers)
            {
                var tableCellDTO = TableCellDTO.Create(header.Key, header.Value);
                headerTableRowDTO.Row.Add(tableCellDTO);
            }
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                const string label = "Spreadsheeet Payload Rows";
                updater.CrateStorage.RemoveByLabel(label);
                updater.CrateStorage.Add(Crate.CreateStandardTableDataCrate(label, false, rows.ToArray()));
            }
        }
    }
}