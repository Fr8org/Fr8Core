using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Exceptions;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalGoogle.Actions
{
    public class Extract_Spreadsheet_Data_v1 : BasePluginAction
    {
        private readonly IGoogleSheet _google;

        public Extract_Spreadsheet_Data_v1()
        {
            _google = new GoogleSheet();
        }

        public override Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }
            return base.Configure(curActionDTO);
        }

        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActionDTO curActionDTO)
        {
            if (NeedsAuthentication(curActionDTO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }
            return await CreateStandardPayloadDataFromStandardTableData(curActionDTO);
        }

        private async Task<PayloadDTO> CreateStandardPayloadDataFromStandardTableData(ActionDTO curActionDTO)
        {
            var processPayload = await GetProcessPayload(curActionDTO.ProcessId);

            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var tableDataMS = await GetTargetTableData(
                curActionDO.Id,
                curActionDO.CrateStorageDTO()
            );

            if (!tableDataMS.FirstRowHeaders)
            {
                throw new Exception("No headers found in the Standard Table Data Manifest.");
            }

            // Create a crate of payload data by using Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            // Add a crate of PayloadData to action's crate storage
            var payloadDataCrate = Crate.CreatePayloadDataCrate("ExcelTableRow", "Excel Data", tableDataMS);
            Crate.AddCrate(processPayload, payloadDataCrate);
            
            return processPayload;            
        }

        private async Task<StandardTableDataCM> GetTargetTableData(int actionId, CrateStorageDTO curCrateStorageDTO)
        {
            // Find crates of manifest type Standard Table Data
            var standardTableDataCrates = Crate.GetCratesByManifestType(CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME, curCrateStorageDTO);

            // If no crate of manifest type "Standard Table Data" found, try to find upstream for a crate of type "Standard File Handle"
            if (!standardTableDataCrates.Any())
            {
                return await GetUpstreamTableData(actionId, curCrateStorageDTO);
            }

            return JsonConvert.DeserializeObject<StandardTableDataCM>(standardTableDataCrates.First().Contents);
        }

        private async Task<StandardTableDataCM> GetUpstreamTableData(int actionId, CrateStorageDTO curCrateStorageDTO)
        {
            var upstreamFileHandleCrates = await GetUpstreamFileHandleCrates(actionId);

            //if no "Standard File Handle" crate found then return
            if (!upstreamFileHandleCrates.Any())
                return null;

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

        //private async Task<FileDO> ProcessFile(string dockyardStorageUrl, string fileName)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var values = new Dictionary<string, string>
        //                {
        //                    { "DockyardStorageURL", dockyardStorageUrl },
        //                    { "Filename", fileName },
        //                    //{ "AuthorizationToken", curFileDO.DockyardAccountID } //ignoring for now
        //                };

        //        var content = new FormUrlEncodedContent(values);

        //        var response = await client.PostAsync(ConfigurationManager.AppSettings["FileServerApiUrl"], content);

        //        var curFileDOTask = await response.Content.ReadAsAsync<FileDO>();

        //        return curFileDOTask;
        //    }
        //}

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private CrateDTO CreateConfigurationControlsCrate(IDictionary<string, string> spreadsheets)
        {
            var controlList = new List<ControlDefinitionDTO>();

            var spreadsheetControl = new DropDownListControlDefinitionDTO()
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
                    ManifestType = CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                },
                ListItems = spreadsheets.Select(pair => new ListItem() { Key = pair.Key, Value = pair.Value }).ToList()
            };
            controlList.Add(spreadsheetControl);

            var textBlockControlField = new TextBlockControlDefinitionDTO()
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
        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.Id > 0)
            {
                ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

                //Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
                CrateDTO upstreamFieldsCrate = await MergeUpstreamFields(curActionDO.Id, "Select a Google Spreadsheet");

                //build a controls crate to render the pane
                var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(curActionDTO.AuthToken.Token);
                var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
                CrateDTO configurationControlsCrate = CreateConfigurationControlsCrate(spreadsheets);

                var crateStrorageDTO = AssembleCrateStorage(upstreamFieldsCrate, configurationControlsCrate);
                curActionDTO.CrateStorage = crateStrorageDTO;
            }
            else
            {
                throw new ArgumentException(
                    "Configuration requires the submission of an Action that has a real ActionId");
            }
            return curActionDTO;
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var spreadsheetsFromUserSelection = Action.FindKeysByCrateManifestType(
                    curActionDO,
                    new Manifest(Data.Constants.MT.StandardConfigurationControls),
                    "select_spreadsheet"
                )
                .Result
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            var hasDesignTimeFields = curActionDTO.CrateStorage.CrateDTO
                .Any(x => x.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME
                    && x.Label == "Worksheet Column Headers");

            if (spreadsheetsFromUserSelection.Length == 1 || hasDesignTimeFields)
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var spreadsheetsFromUserSelection =
                (await Action.FindKeysByCrateManifestType(
                    curActionDO,
                    new Manifest(Data.Constants.MT.StandardConfigurationControls),
                    "select_spreadsheet"))
                    .Select(e => (string) e["value"])
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

            if (spreadsheetsFromUserSelection.Length > 1)
                throw new AmbiguityException();

            // RFemove previously created configuration control crate
            Crate.RemoveCrateByManifestType(curActionDTO.CrateStorage.CrateDTO, CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME);
            // Creating configuration control crate with a file picker and textblock
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(curActionDTO.AuthToken.Token);
            var spreadsheets = _google.EnumerateSpreadsheetsUris(authDTO);
            CrateDTO configControlsCrateDTO = CreateConfigurationControlsCrate(spreadsheets);
            curActionDTO.CrateStorage.CrateDTO.Add(configControlsCrateDTO);

            if (spreadsheetsFromUserSelection.Length > 0)
            {
                var selectedSpreadsheetUri = spreadsheetsFromUserSelection[0];
                return await TransformExcelFileDataToStandardTableDataCrate(curActionDTO, selectedSpreadsheetUri);
            }
            else
            {
                return curActionDTO;
            }
        }

        private async Task<ActionDTO> TransformExcelFileDataToStandardTableDataCrate(ActionDTO curActionDTO, string spreadsheetUri)
        {
            var curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(curActionDTO.AuthToken.Token);
            // Fetch column headers in Excel file and assign them to the action's crate storage as Design TIme Fields crate
            var headers = _google.EnumerateColumnHeaders(spreadsheetUri, authDTO).ToArray();
            if (headers.Any())
            {
                var curCrateDTO = Crate.CreateDesignTimeFieldsCrate(
                            "Spreadsheet Column Headers",
                            headers.Select(col => new FieldDTO() { Key = col, Value = col }).ToArray()
                        );
                Crate.AddOrReplaceCrate("Spreadsheet Column Headers", curActionDO, curCrateDTO);
            }

            CreatePayloadCrate_ExcelRows(curActionDO, spreadsheetUri, authDTO, headers);

            return AutoMapper.Mapper.Map<ActionDTO>(curActionDO);
        }

        private void CreatePayloadCrate_ExcelRows(ActionDO curActionDO, string spreadsheetUri, GoogleAuthDTO authDTO, string[] headers)
        {
            // Fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate
            var rows = _google.EnumerateDataRows(spreadsheetUri, authDTO);
            var headerTableRowDTO = new TableRowDTO() { Row = new List<TableCellDTO>(), };
            for (int i = 0; i < headers.Length; ++i)
            {
                var tableCellDTO = TableCellDTO.Create((i + 1).ToString(), headers[i]);
                headerTableRowDTO.Row.Add(tableCellDTO);
            }
            Crate.AddCrate(curActionDO, Crate.CreateStandardTableDataCrate("Excel Payload Rows", true, rows.ToArray()));
        }
    }
}