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
using pluginExcel.Infrastructure;

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

            StandardTableDataMS tableDataMS = await GetTargetTableData(curActionDO.Id, curActionDO.CrateStorageDTO());
            if (!tableDataMS.FirstRowHeaders)
                throw new Exception("No headers found in the Standard Table Data Manifest.");

            // Use Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            var payloadDataMS = TransformStandardTableDataToStandardPayloadData(tableDataMS);

            // Add a crate of PayloadData to action's crate storage
            var payloadDataCrate = _crate.Create("Excel Data", JsonConvert.SerializeObject(payloadDataMS), CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID);
            _action.AddCrate(curActionDO, payloadDataCrate);

            return Mapper.Map<ActionDTO>(curActionDO);
        }

        private async Task<StandardTableDataMS> GetTargetTableData(int actionId, CrateStorageDTO curCrateStorageDTO)
        {
            // Find crates of manifest type Standard Table Data
            var standardTableDataCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME, curCrateStorageDTO);

            // If no crate of manifest type "Standard Table Data" found, try to find upstream for a crate of type "Standard File Handle"
            if (!standardTableDataCrates.Any())
            {
                return await GetUpstreamTableData(actionId, curCrateStorageDTO);
            }

            return JsonConvert.DeserializeObject<StandardTableDataMS>(standardTableDataCrates.First().Contents);
        }

        private async Task<StandardTableDataMS> GetUpstreamTableData(int actionId, CrateStorageDTO curCrateStorageDTO)
        {
            var upstreamFileHandleCrate = await GetCratesByDirection(actionId, CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME, GetCrateDirection.Upstream);

            //if no "Standard File Handle" crate found then return
            if (!upstreamFileHandleCrate.Any())
                return null;

            //if more than one "Standard File Handle" crates found then throw an exception
            if (upstreamFileHandleCrate.Count > 1)
                throw new Exception("More than one Standard File Handle crates found upstream.");

            // Deserialize the Standard File Handle crate to StandardFileHandleMS object
            StandardFileHandleMS fileHandleMS = JsonConvert.DeserializeObject<StandardFileHandleMS>(upstreamFileHandleCrate.First().Contents);

            // Use the url for file from StandardFileHandleMS and read from the file and transform the data into StandardTableData and assign it to Action's crate storage
            StandardTableDataMS tableDataMS = ExcelUtils.GetTableData(fileHandleMS.DockyardStorageUrl);

            return tableDataMS;
        }

        private StandardPayloadDataMS TransformStandardTableDataToStandardPayloadData(StandardTableDataMS tableDataMS)
        {
            var payloadDataMS = _crate.CreatePayloadDataCrate("ExcelTableRow");

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
        /// Create configuration controls crate.
        /// </summary>
        private CrateDTO CreateConfigurationControls()
        {
            var fieldFilterPane = new FieldDefinitionDTO("filePicker")
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
                    CrateDTO upstreamFieldsCrate = _crate.CreateDesignTimeFieldsCrate("Select Excel File", curUpstreamFields);

                    //build a controls crate to render the pane
                    CrateDTO configurationControlsCrate = CreateConfigurationControls();

                    var crateStrorageDTO = AssembleCrateStorage(upstreamFieldsCrate, configurationControlsCrate);
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
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
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
                var curCrateDTO = _crate.CreateDesignTimeFieldsCrate(
                            "Spreadsheet Column Headers",
                            headers.Select(col => new FieldDTO() { Key = col, Value = col }).ToArray()
                        );
                _action.AddOrReplaceCrate("Spreadsheet Column Headers", curActionDO, curCrateDTO);
            }

            CreatePayloadCrate_ExcelRows(curActionDO, fileAsByteArray, headersArray, ext);

            return Mapper.Map<ActionDTO>(curActionDO);
        }

        private void CreatePayloadCrate_ExcelRows(ActionDO curActionDO, byte[] fileAsByteArray, string[] headersArray, string extension)
        {
            // Fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, extension);
            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = ExcelUtils.CreateTableCellPayloadObjects(rowsDictionary, headersArray);
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
        }
    }
}