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
            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);

            // Find crate with with nmanifest type "Standard Table Data".
            var curCrateStorage = curActionDO.CrateStorageDTO();
            var curControlsCrate = curCrateStorage.CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME);

            if (curControlsCrate == null || string.IsNullOrEmpty(curControlsCrate.Contents))
            {
                var curUpstreamFileHandle = GetCratesByDirection(curActionDO, CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME, GetCrateDirection.Upstream);
                /*
                // TODO: temporary code for testing code below 
                StandardFileHandleMS fileHandleMS = new StandardFileHandleMS()
                {
                    DockyardStorageUrl = @"..\..\Sample Files\",
                    Filename = "SampleFile1.xlsx",
                };

                var crate = new CrateDTO()
                {
                    Contents = JsonConvert.SerializeObject(fileHandleMS),
                    Label = "file_handle",
                    ManifestType = CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_NAME,
                    ManifestId = CrateManifests.STANDARD_FILE_HANDLE_MANIFEST_ID
                };
                curUpstreamFileHandle.Insert(0, crate);
                // Temp code end
                */

                // TODO : This code has not been tested yet - unit test creation is getting quite complex
                if(curUpstreamFileHandle != null && curUpstreamFileHandle.Count() > 0)
                {
                    var fileHandle = JsonConvert.DeserializeObject<StandardFileHandleMS>(curUpstreamFileHandle.ElementAt(0).Contents);
                    
                    var curFileDO = await ProcessFile(fileHandle.DockyardStorageUrl, fileHandle.Filename);

                    var interimActionDTO = ProcessExcelFileToCreateStandardTableDataCrate(curActionDTO, curFileDO.CloudStorageUrl);

                    _action.AddCrate(curActionDO, interimActionDTO.CrateStorage.CrateDTO);
                }
            }

            curActionDO = ProcessCrate(curActionDO);

            return AutoMapper.Mapper.Map<ActionDTO>(curActionDO);
        }

        private ActionDO ProcessCrate(ActionDO curActionDO)
        {
            var curControlsCrate = curActionDO.CrateStorageDTO().CrateDTO
                .FirstOrDefault(x => x.ManifestType == CrateManifests.STANDARD_TABLE_DATA_MANIFEST_NAME);

            if (curControlsCrate == null || string.IsNullOrEmpty(curControlsCrate.Contents))
                return curActionDO;

            var payloadData = new StandardPayloadDataMS()
            {
                Payload = new List<PayloadObjectDTO>(),
                ObjectType = "ExcelTableRow",
            };

            var tableDataMS = JsonConvert.DeserializeObject<StandardTableDataMS>(curControlsCrate.Contents);

            foreach(var tableRowDTO in tableDataMS.Table)
            {
                var fields = new List<FieldDTO>();
                foreach(var tableCellDTO in tableRowDTO.Row)
                {
                    var listFieldDTO = new FieldDTO()
                    {
                        Key = tableCellDTO.Cell.Key,
                        Value = tableCellDTO.Cell.Value,
                    };
                    fields.Add(listFieldDTO);
                }
                payloadData.Payload.Add(new PayloadObjectDTO() { Fields = fields, });
            }

            var crate = _crate.Create("ExcelTableRow", JsonConvert.SerializeObject(payloadData), CrateManifests.STANDARD_PAYLOAD_MANIFEST_NAME, CrateManifests.STANDARD_PAYLOAD_MANIFEST_ID);
            _action.AddCrate(curActionDO, new List<CrateDTO>() { crate });
            return curActionDO;
        }

        private async Task<FileDO> ProcessFile(string dockyardStorageUrl, string fileName)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                        {
                            { "DockyardStorageURL", dockyardStorageUrl },
                            { "Filename", fileName },
                            //{ "AuthorizationToken", curFileDO.DockyardAccountID } //ignoring for now
                        };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(ConfigurationManager.AppSettings["FileServerApiUrl"], content);

                var curFileDOTask = await response.Content.ReadAsAsync<FileDO>();

                return curFileDOTask;
            }
        }

        /// <summary>
        /// Configure infrastructure.
        /// </summary>
        public ActionDTO Configure(ActionDTO actionDTO)
        {
            return ProcessConfigurationRequest(actionDTO, ConfigurationEvaluator);
        }

        /// <summary>
        /// Create configuration controls crate.
        /// </summary>
        private CrateDTO CreateStandardConfigurationControls()
        {
            var fieldFilterPane = new FilterPaneFieldDefinitionDTO()
            {
                FieldLabel = "Select Excel File",
                Type = "filePicker",
                Name = "select_file",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Select Excel File",
                    ManifestType = CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME
                }
            };

            return PackControlsCrate(fieldFilterPane);
        }

        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override ActionDTO InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            if (curActionDTO.Id > 0)
            {
                //this conversion from actiondto to Action should be moved back to the controller edge
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    ActionDO curActionDO = _action.MapFromDTO(curActionDTO);

                    var curUpstreamFields = GetDesignTimeFields(curActionDO, GetCrateDirection.Upstream).Fields.ToArray();

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
        protected override ActionDTO FollowupConfigurationResponse(ActionDTO curActionDTO)
        {
            var controlsCrates = _action.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME,
                curActionDTO.CrateStorage);

            var filePaths = _crate.GetElementByKey(controlsCrates, key: "select_file", keyFieldName: "name")
                .Select(e => (string)e["value"])
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            return ProcessExcelFileToCreateStandardTableDataCrate(curActionDTO, filePaths[0]);
        }

        private ActionDTO ProcessExcelFileToCreateStandardTableDataCrate(ActionDTO curActionDTO, string curExcelFilePath)
        {
            string csvFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".csv";

            //In all followup calls, update data fields of the configuration store          
            List<String> headers = GetHeadersFromExcel(curActionDTO, curExcelFilePath, csvFilePath);
            List<TableRowDTO> rows = GetTabularDataFromExcel(curActionDTO, curExcelFilePath, csvFilePath);

            var curCrateStorageDTO = new CrateStorageDTO
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

            if (rows != null && rows.Count > 0)
            {
                curCrateStorageDTO.CrateDTO.Add(
                    _crate.CreateStandardTableDataCrate(
                        "Excel Payload Rows", false, rows.ToArray()
                    ));
            }

            var curActionDO = AutoMapper.Mapper.Map<ActionDO>(curActionDTO);

            int foundSameCrateDTOAtIndex = curActionDO.CrateStorageDTO().CrateDTO.FindIndex(m => m.Label == "Spreadsheet Column Headers");
            if (foundSameCrateDTOAtIndex == -1)
            {
                _action.AddCrate(curActionDO, curCrateStorageDTO.CrateDTO.ToList());
            }
            else
            {
                CrateStorageDTO localList = curActionDO.CrateStorageDTO();
                localList.CrateDTO.RemoveAt(foundSameCrateDTOAtIndex);
                curActionDO.CrateStorage = JsonConvert.SerializeObject(localList);
                _action.AddCrate(curActionDO, curCrateStorageDTO.CrateDTO.ToList());
            }
            curCrateStorageDTO = curActionDO.CrateStorageDTO();
            curActionDTO.CrateStorage = curCrateStorageDTO;
            return curActionDTO;
        }

        private List<string> GetHeadersFromExcel(ActionDTO curActionDTO, string excelFilePath, string csvFilePath)
        {
            try
            {
                ExcelUtils.ConvertToCsv(excelFilePath, csvFilePath);
                using (ICsvReader csvReader = new CsvReader(csvFilePath))
                {
                    var columns = csvReader.GetColumnHeaders();
                    return columns.ToList<string>();
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                try { File.Delete(csvFilePath); }
                catch { }
            }
        }

        private List<TableRowDTO> GetTabularDataFromExcel(ActionDTO curActionDTO, string excelFilePath, string csvFilePath)
        {
            try
            {
                var rowsList = new List<TableRowDTO>();
                ExcelUtils.ConvertToCsv(excelFilePath, csvFilePath);
                using (ICsvReader csvReader = new CsvReader(csvFilePath))
                {
                    var tabularData = csvReader.GetTabularData();
                    var listOfRows = new List<TableRowDTO>();
                    foreach(var row in tabularData.Keys)
                    {
                        var listOfCells = new List<TableCellDTO>();                        
                        foreach(var colHeaderCellValuePair in tabularData[row])
                        {
                            listOfCells.Add(
                                new TableCellDTO()
                                {
                                    Cell = new FieldDTO()
                                    {
                                        Key = colHeaderCellValuePair.Item1,
                                        Value = colHeaderCellValuePair.Item2
                                    }
                                }
                            );
                        }
                        listOfRows.Add(new TableRowDTO() { Row = listOfCells });
                    }
                    return listOfRows;
                }
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                try { File.Delete(csvFilePath); }
                catch { }
            }
        }

        ////Returning the crate with text field control 
        //private CrateDTO GetTextBoxControlForDisplayingError(string fieldLabel, string errorMessage)
        //{
        //    var fields = new List<FieldDefinitionDTO>() 
        //    {
        //        new TextBlockFieldDTO()
        //        {
        //            FieldLabel = fieldLabel,
        //            Value = errorMessage,
        //            Type = "textBlockField",
        //            cssClass = "well well-lg"

        //        }
        //    };

        //    var controls = new StandardConfigurationControlsMS()
        //    {
        //        Controls = fields
        //    };

        //    var crateControls = _crate.Create(
        //                "Configuration_Controls",
        //                JsonConvert.SerializeObject(controls),
        //                "Standard Configuration Controls"
        //            );

        //    return crateControls;
        //}
    }
}