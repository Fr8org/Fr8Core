using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using StructureMap;
using Data.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Exceptions;
using Hub.Interfaces;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalExcel.Infrastructure;

namespace terminalExcel.Actions
{
    public class Load_Excel_File_v1 : BaseTerminalActivity
    {
        private class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public readonly ControlDefinitionDTO select_file;

            public ActionUi(string uploadedFileName = null)
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add((select_file = new ControlDefinitionDTO(ControlTypes.FilePicker)
                {
                    Label = "Select an Excel file",
                    Name = "select_file",
                    Required = true,
                    Events = new List<ControlEvent>()
                    {
                        new ControlEvent("onChange", "requestConfig")
                    },
                    Source = new FieldSourceDTO
                    {
                        Label = "Select an Excel file",
                        ManifestType = CrateManifestTypes.StandardConfigurationControls
                    },
                }));

                if (!string.IsNullOrEmpty(uploadedFileName))
                {
                    Controls.Add(new TextBlock
                    {
                        Label = "",
                        Value = "Uploaded file: " + Uri.UnescapeDataString(uploadedFileName),
                        CssClass = "well well-lg"
                    });
                }

                Controls.Add(new TextBlock
                {
                    Label = "",
                    Value = "This Action will try to extract a table of rows from the first spreadsheet in the file. The rows should have a header row.",
                    CssClass = "well well-lg TextBlockClass"
                });                
            }
        }


        /// <summary>
        /// Action processing infrastructure.
        /// </summary>
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return await CreateStandardPayloadDataFromStandardTableData(curActivityDO, containerId);
        }

        private async Task<PayloadDTO> CreateStandardPayloadDataFromStandardTableData(ActivityDO curActivityDO, Guid containerId)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            var tableDataMS = await GetTargetTableData(
                curActivityDO,
                Crate.GetStorage(curActivityDO)
            );

            if (!tableDataMS.FirstRowHeaders)
            {
                return Error(payloadCrates, "No headers found in the Standard Table Data Manifest.");
            }

            // Create a crate of payload data by using Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            // Add a crate of PayloadData to action's crate storage
            
            
            using (var crateStorage = Crate.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(Crate.CreatePayloadDataCrate("ExcelTableRow", "Excel Data", tableDataMS));
            }
            return Success(payloadCrates);        
        }

        private async Task<StandardTableDataCM> GetTargetTableData(ActivityDO activityDO, ICrateStorage curCrateStorageDTO)
        {
            // Find crates of manifest type Standard Table Data
            var standardTableDataCrates = curCrateStorageDTO.CratesOfType<StandardTableDataCM>();

            // If no crate of manifest type "Standard Table Data" found, try to find upstream for a crate of type "Standard File Handle"
            if (!standardTableDataCrates.Any())
            {
                return await GetUpstreamTableData(activityDO);
            }

            return standardTableDataCrates.First().Content;
        }

        private async Task<StandardTableDataCM> GetUpstreamTableData(ActivityDO activityDO)
        {
            var upstreamFileHandleCrates = await GetUpstreamFileHandleCrates(activityDO);

            //if no "Standard File Handle" crate found then return
            if (!upstreamFileHandleCrates.Any())
                return null;

            //if more than one "Standard File Handle" crates found then throw an exception
            if (upstreamFileHandleCrates.Count > 1)
                throw new Exception("More than one Standard File Handle crates found upstream.");

            // Deserialize the Standard File Handle crate to StandardFileHandleMS object
            StandardFileDescriptionCM fileHandleMS = upstreamFileHandleCrates.First().Get<StandardFileDescriptionCM>();

            // Use the url for file from StandardFileHandleMS and read from the file and transform the data into StandardTableData and assign it to Action's crate storage
            StandardTableDataCM tableDataMS = ExcelUtils.GetTableData(fileHandleMS.DockyardStorageUrl);

            return tableDataMS;
        }

        //private async Task<FileDO> ProcessFile(string dockyardStorageUrl, string fileName)
        //{
        //    using (var client = new HttpxcsClient())
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
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActivityDO.Id != Guid.Empty)
            {
                //Pack the merged fields into a new crate that can be used to populate the dropdownlistbox
                Crate upstreamFieldsCrate = await MergeUpstreamFields(curActivityDO, "Select Excel File");

                using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
                {
                    crateStorage.Clear();
                    crateStorage.Add(upstreamFieldsCrate);
                    crateStorage.Add(PackControls(new ActionUi()));
                }
            }
            else
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }
            return curActivityDO;
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (Crate.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override Task<ActivityDO> FollowupConfigurationResponse(
            ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var storage = Crate.GetStorage(curActivityDO);
            var filePathsFromUserSelection = storage.CrateContentsOfType<StandardConfigurationControlsCM>()
                .Select(x =>
                {
                    var actionUi = new ActionUi();
                    actionUi.ClonePropertiesFrom(x);
                    return actionUi;
                })
                 .Where(x => !string.IsNullOrEmpty(x.select_file.Value)).ToArray();

            if (filePathsFromUserSelection.Length > 1)
            {
                throw new AmbiguityException();
            }

            using (var crateStorage = Crate.GetUpdatableStorage(curActivityDO))
            {
                string uploadFilePath = null;
                if (filePathsFromUserSelection.Length > 0)
                {
                    uploadFilePath = filePathsFromUserSelection[0].select_file.Value;
                }

                string fileName = null;
                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    fileName = ExtractFileName(uploadFilePath);
                }
                else
                {
                    var labelControl = storage.CrateContentsOfType<StandardConfigurationControlsCM>()
                        .First()
                        .Controls
                        .Where(x => x.Value != null && x.Value.StartsWith("Uploaded file: "))
                        .FirstOrDefault();

                    if (labelControl != null)
                    {
                        fileName = labelControl.Value.Substring("Uploaded file: ".Length);
                    }
                }

                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Add(PackControls(new ActionUi(fileName)));

                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    TransformExcelFileDataToStandardTableDataCrate(crateStorage, uploadFilePath);
                }
            }

            return Task.FromResult(curActivityDO);
        }

        private string ExtractFileName(string uploadFilePath)
        {
            if (uploadFilePath == null)
            {
                return null;
            }

            var index = uploadFilePath.LastIndexOf('/');
            if (index >= 0 && (uploadFilePath.Length > index + 1))
            {
                return uploadFilePath.Substring(index + 1);
            }

            return uploadFilePath;
        }

        private void TransformExcelFileDataToStandardTableDataCrate(ICrateStorage storage, string selectedFilePath)
        {
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
                var curCrateDTO = Crate.CreateDesignTimeFieldsCrate(
                            "Spreadsheet Column Headers",
                            headers.Select(col => new FieldDTO() { Key = col, Value = col }).ToArray()
                        );

                storage.RemoveByLabel("Spreadsheet Column Headers");
                storage.Add(curCrateDTO);
            }

            CreatePayloadCrate_ExcelRows(storage, fileAsByteArray, headersArray, ext);
        }

        private void CreatePayloadCrate_ExcelRows(ICrateStorage storage, byte[] fileAsByteArray, string[] headersArray, string extension)
        {
            // Fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, extension);
            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = ExcelUtils.CreateTableCellPayloadObjects(rowsDictionary, headersArray);
                if (rows != null && rows.Count > 0)
                {
                    storage.Add(Crate.CreateStandardTableDataCrate("Excel Payload Rows", true, rows.ToArray()));
                }
            }
        }
    }

    // For backward compatibility
    public class Extract_Data_v1 : Load_Excel_File_v1 { }
}