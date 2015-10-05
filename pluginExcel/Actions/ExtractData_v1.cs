using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using StructureMap;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using PluginBase.BaseClasses;
using PluginBase.Infrastructure;
using Utilities;
using System.IO;
using Utilities.Interfaces;
using Data.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;

namespace pluginExcel.Actions
{
    public class ExtractData_v1 : BasePluginAction
    {
        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<ActionDTO> Execute(ActionDTO curActionDTO)
        {
            return await CreateStandardPayloadDataFromStandardTableData(curActionDTO);
        }

        private async Task<ActionDTO> CreateStandardPayloadDataFromStandardTableData(ActionDTO curActionDTO)
        {
            var curActionDO = _action.MapFromDTO(curActionDTO);

            // Find crates of manifest type Standard Table Data
            var standardTableDataCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME, curActionDO.CrateStorageDTO());

            // If no crate of manifest type "Standard Table Data" found, try to find upstream for a crate of type "Standard File Handle"
            if (!standardTableDataCrates.Any())
            {
                var upstreamFileHandleCrate = await GetCratesByDirection(curActionDO.Id, CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME, GetCrateDirection.Upstream);

                //if no "Standard File Handle" crate found then return
                if (!upstreamFileHandleCrate.Any())
                    return curActionDTO;

                // Deserialize the Standard File Handle crate to StandardFileHandleMS object
                StandardFileHandleMS fileHandleMS = JsonConvert.DeserializeObject<StandardFileHandleMS>(upstreamFileHandleCrate.First().Contents);

                // Use the url for file from StandardFileHandleMS and read from the file and transform the data into StandardTableData and assign it to Action's crate storage
                curActionDTO = await TransformExcelFileDataToStandardTableDataCrate(curActionDTO, fileHandleMS.DockyardStorageUrl);

                // Find crates of manifest type Standard Table Data
                standardTableDataCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME, curActionDO.CrateStorageDTO());
            }

            StandardTableDataMS tableDataMS = JsonConvert.DeserializeObject<StandardTableDataMS>(standardTableDataCrates.First().Contents);
            if (!tableDataMS.FirstRowHeaders)
                throw new Exception("No headers found in the Standard Table Data Manifest.");

            // Use Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            var payloadDataMS = TransformStandardTableDataToStandardPayloadData(tableDataMS);

            // Add a crate of PayloadData to action's crate storage
            var payloadDataCrate = _crate.Create("Excel Data", JsonConvert.SerializeObject(payloadDataMS), CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID);
            _action.AddCrate(curActionDO, new List<CrateDTO>() { payloadDataCrate });

            curActionDTO.CrateStorage = curActionDO.CrateStorageDTO();

            return curActionDTO;
        }

        private StandardPayloadDataMS TransformStandardTableDataToStandardPayloadData(StandardTableDataMS tableDataMS)
        {
            var payloadDataMS = new StandardPayloadDataMS()
            {
                PayloadObjects = new List<PayloadObjectDTO>(),
                ObjectType = "ExcelTableRow",
            };

            // Rows containing column names
            var columnHeadersRowDTO = tableDataMS.Table[0];

            for (int i = 1; i < tableDataMS.Table.Count; ++i) // Since first row is headers; hence i starts from 1
            {
                var tableRowDTO = tableDataMS.Table[i];
                var fields = new List<FieldDTO>();
                for (int j = 0; j < tableRowDTO.Row.Count; ++j)
                {
                    var tableCellDTO = tableRowDTO.Row[j];
                    var listFieldDTO = new FieldDTO()
                    {
                        Key = columnHeadersRowDTO.Row[j].Cell.Value,
                        Value = tableCellDTO.Cell.Value,
                    };
                    fields.Add(listFieldDTO);
                }
                payloadDataMS.PayloadObjects.Add(new PayloadObjectDTO() { PayloadObject = fields, });
            }

            return payloadDataMS;
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
        /// Configure infrastructure.
        /// </summary>
        public async Task<ActionDTO> Configure(ActionDTO actionDTO)
        {
            return await ProcessConfigurationRequest(actionDTO, ConfigurationEvaluator);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private CrateDTO CreateStandardConfigurationControls()
        {
            var fieldFilterPane = new ControlsDefinitionDTO("filePicker")
            {
                Label = "Select Excel File",
                Name = "select_file",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Select Excel File",
                    ManifestType = CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
                },
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.Id > 0)
            {
                //this conversion from actiondto to Action should be moved back to the controller edge
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    ActionDO curActionDO = _action.MapFromDTO(curActionDTO);

                    var curUpstreamFields = (await GetDesignTimeFields(curActionDO.Id, GetCrateDirection.Upstream)).Fields.ToArray();

                    //2) Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
                    CrateDTO filePickerCrate = _crate.CreateDesignTimeFieldsCrate("Select Excel File", curUpstreamFields);

                    //build a controls crate to render the pane
                    CrateDTO configurationControlsCrate = CreateStandardConfigurationControls();

                    var crateStrorageDTO = AssembleCrateStorage(filePickerCrate, configurationControlsCrate);
                    curActionDTO.CrateStorage = crateStrorageDTO;
                }
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
        private ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            CrateStorageDTO curCrates = curActionDTO.CrateStorage;

            if (curCrates.CrateDTO.Count == 0)
                return ConfigurationRequestType.Initial;

            //load configuration crates of manifest type Standard Control Crates
            //look for a text field name select_file with a value
            var controlsCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);
            var filePaths = _crate.GetElementByKey(controlsCrates, key: "select_file", keyFieldName: "name")
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            //if there are more than 2 return file names, something is wrong
            //if there are none or if there's one but it's value is "" the return initial else return followup
            var objCount = filePaths.Length;
            if (objCount > 1)
                throw new ArgumentException("didn't expect to see more than one file names with the name select_file on this Action", "curActionDTO");
            if (objCount == 0)
                return ConfigurationRequestType.Initial;
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override async Task<ActionDTO> FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = _action.MapFromDTO(curActionDTO);

            var controlsCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);

            var filePathsFromUserSelection = _crate.GetElementByKey(controlsCrates, key: "select_file", keyFieldName: "name")
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            var selectedFilePath = filePathsFromUserSelection[0];

            return await TransformExcelFileDataToStandardTableDataCrate(curActionDTO, selectedFilePath);
        }

        private async Task<ActionDTO> TransformExcelFileDataToStandardTableDataCrate(ActionDTO curActionDTO, string selectedFilePath)
        {
            var curActionDO = _action.MapFromDTO(curActionDTO);

            // Check if the file is an Excel file.
            string ext = Path.GetExtension(selectedFilePath);
            if (ext != ".xls" && ext != ".xlsx")
                throw new ArgumentException("Expected '.xls' or '.xlsx'", "selectedFile");

            FileDO curFileDO = new FileDO()
            {
                CloudStorageUrl = selectedFilePath,
            };

            IFile file = ObjectFactory.GetInstance<IFile>();

            // Read file from repository
            var fileAsByteArray = file.Retrieve(curFileDO);

            // Fetch column headers in Excel file and assign them to the action's crate storage as Design TIme Fields crate
            var headersArray = ExcelUtils.GetColumnHeaders(fileAsByteArray, ext);
            if (headersArray != null)
            {
                var headers = headersArray.ToList();
                var curColumnHeadersCrateStorageDTO = new CrateStorageDTO
                {
                    //this needs to be updated to hold Crates instead of FieldDefinitionDTO
                    CrateDTO = new List<CrateDTO>
                    {
                        _crate.CreateDesignTimeFieldsCrate(
                            "Spreadsheet Column Headers",
                            headers.Select(col => new FieldDTO() { Key = col, Value = col }).ToArray()
                        ),
                    }
                };
                var columnHeaderCratesInActionDO = _action.GetCratesByLabel("Spreadsheet Column Headers", curActionDO.CrateStorageDTO());
                if (!columnHeaderCratesInActionDO.Any()) // no existing column headers crate found, then add the crate
                {
                    _action.AddCrate(curActionDO, curColumnHeadersCrateStorageDTO.CrateDTO.ToList());
                }
                else
                {
                    // Remove the existing crate for this label
                    _crate.RemoveCrateByLabel(curActionDO.CrateStorageDTO().CrateDTO, "Spreadsheet Column Headers");

                    //CrateStorageDTO actionCrateStorageDTO = curActionDO.CrateStorageDTO();
                    //curActionDO.CrateStorage = JsonConvert.SerializeObject(actionCrateStorageDTO);

                    // Add the newly created crate for this label to action's crate storage
                    _action.AddCrate(curActionDO, curColumnHeadersCrateStorageDTO.CrateDTO.ToList());
                }
            }

            // Fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, ext);
            //List<TableRowDTO> rows = GetTabularDataFromExcel(curActionDTO, selectedFilePath, csvFilePath);
            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = ConvertRowsDictToListOfTableRowDTO(rowsDictionary, headersArray);
                if (rows != null && rows.Count > 0)
                {
                    var curExcelPayloadRowsCrateStorageDTO = new CrateStorageDTO
                    {
                        CrateDTO = new List<CrateDTO>
                        {
                            _crate.CreateStandardTableDataCrate("Excel Payload Rows", true, rows.ToArray()),
                        }
                    };
                    _action.AddCrate(curActionDO, curExcelPayloadRowsCrateStorageDTO.CrateDTO.ToList());
                }
            }

            var curActionDTOCrateStorageDTO = curActionDO.CrateStorageDTO();
            curActionDTO.CrateStorage = curActionDTOCrateStorageDTO;
            return curActionDTO;
        }

        public List<TableRowDTO> ConvertRowsDictToListOfTableRowDTO(Dictionary<string, List<Tuple<string, string>>> rowsDictionary, string[] headersArray)
        {
            try
            {
                var listOfRows = new List<TableRowDTO>();
                // Add header as the first row in List<TableRowDTO> so that it is becomes the first row in StandardTableMS.
                var headerTableRowDTO = new TableRowDTO() { Row = new List<TableCellDTO>(), };
                for (int i = 0; i < headersArray.Count(); ++i)
                {
                    var tableCellDTO = new TableCellDTO()
                    {
                        Cell = new FieldDTO()
                        {
                            Key = (i + 1).ToString(),
                            Value = headersArray[i]
                        }
                    };
                    headerTableRowDTO.Row.Add(tableCellDTO);
                }
                listOfRows.Insert(0, headerTableRowDTO);

                // Process each item in the dictionary and add it as an item in List<TableRowDTO>
                foreach (var row in rowsDictionary.Keys)
                {
                    var listOfCells = new List<TableCellDTO>();
                    foreach (var columnCellValuePair in rowsDictionary[row])
                    {
                        listOfCells.Add(
                            new TableCellDTO()
                            {
                                Cell = new FieldDTO()
                                {
                                    Key = columnCellValuePair.Item1, // Column number
                                    Value = columnCellValuePair.Item2 // Column/cell value
                                }
                            }
                        );
                    }
                    listOfRows.Add(new TableRowDTO() { Row = listOfCells });
                }
                return listOfRows;
            }
            catch (Exception exp)
            {
                throw exp;
            }
        }
    }
}