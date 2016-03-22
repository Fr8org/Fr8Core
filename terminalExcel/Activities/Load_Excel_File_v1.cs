using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalExcel.Infrastructure;
using Utilities;

namespace terminalExcel.Actions
{
    public class Load_Excel_File_v1 : BaseTerminalActivity
    {
        private class ActivityUi : StandardConfigurationControlsCM
        {
            public const string UploadedFileLabel = "Uploaded file: ";
            [JsonIgnore]
            public readonly ControlDefinitionDTO select_file;

            [JsonIgnore]
            public TextBlock UploadedFileTextBlock { get; set; }

            public ActivityUi(string uploadedFileName = null, string uploadedFilePath = null)
            {
                Controls = new List<ControlDefinitionDTO>();
                Controls.Add(select_file = new ControlDefinitionDTO(ControlTypes.FilePicker)
                {
                    Label = "Select an Excel file",
                    Name = nameof(select_file),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    Source = new FieldSourceDTO
                    {
                        Label = "Select an Excel file",
                        ManifestType = CrateManifestTypes.StandardConfigurationControls
                    },
                    Value = uploadedFilePath,
                });
                if (!string.IsNullOrEmpty(uploadedFileName))
                {
                    Controls.Add(UploadedFileTextBlock = new TextBlock
                    {
                        Label = string.Empty,
                        Value = UploadedFileLabel + Uri.UnescapeDataString(uploadedFileName),
                        CssClass = "well well-lg",
                        Name = nameof(UploadedFileTextBlock)
                    });
                }
                Controls.Add(new TextBlock
                {
                    Label = string.Empty,
                    Value = "This Action will try to extract a table of rows from the first spreadsheet in the file. The rows should have a header row.",
                    CssClass = "well well-lg TextBlockClass"
                });
            }
        }

        private const string ColumnHeadersCrateLabel = "Spreadsheet Column Headers";

        public Load_Excel_File_v1() : base("Load Excel File") { }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            return await CreateStandardPayloadDataFromStandardTableData(curActivityDO, containerId);
        }

        private async Task<PayloadDTO> CreateStandardPayloadDataFromStandardTableData(ActivityDO curActivityDO, Guid containerId)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var tableData = CrateManager.GetStorage(curActivityDO)
                                        .FirstCrateOrDefault<StandardTableDataCM>()
                                        ?.Content;
            if (tableData == null)
            {
                return Error(payloadCrates, "No Standard Table Data Manifest exists in activity storage. Probably Excel file is not uploaded or is empty", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            // Create a crate of payload data by using Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            // Add a crate of PayloadData to action's crate storage
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                //crateStorage.Add(Crate.FromContent(GenerateRuntimeCrateLabel(ExtractFileName(GetUploadFilePath(curActivityDO))), tableData, AvailabilityType.RunTime));
                crateStorage.Add(Crate.FromContent(RuntimeCrateLabel, tableData, AvailabilityType.RunTime));
            }
            return Success(payloadCrates);
        }
        /// <summary>
        /// Looks for upstream and downstream Creates.
        /// </summary>
        protected override Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            if (curActivityDO.Id == Guid.Empty)
            {
                throw new ArgumentException("Configuration requires the submission of an Action that has a real ActionId");
            }
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(PackControls(new ActivityUi()));
                crateStorage.Add(GetAvailableRunTimeTableCrate());
            }
            return Task.FromResult(curActivityDO);
        }

        private Crate GetAvailableRunTimeTableCrate()
        {
            var availableRunTimeCrates = Crate.FromContent("Available Run Time Crates", new CrateDescriptionCM(
                    new CrateDescriptionDTO
                    {
                        ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                        Label = RuntimeCrateLabel,
                        ManifestId = (int)MT.StandardTableData,
                        ProducedBy = ActivityName
                    }), AvailabilityType.RunTime);
            return availableRunTimeCrates;
        }

        /// <summary>
        /// If there's a value in select_file field of the crate, then it is a followup call.
        /// </summary>
        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            return CrateManager.IsStorageEmpty(curActivityDO) ? ConfigurationRequestType.Initial : ConfigurationRequestType.Followup;
        }

        //if the user provides a file name, this action attempts to load the excel file and extracts the column headers from the first sheet in the file.
        protected override Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var uploadFilePath = GetUploadFilePath(curActivityDO);
            using (var activityStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                string fileName = null;
                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    fileName = ExtractFileName(uploadFilePath);
                }
                else
                {
                    var activityUi = new ActivityUi();
                    activityUi.ClonePropertiesFrom(activityStorage.FirstCrate<StandardConfigurationControlsCM>().Content);
                    fileName = activityUi.UploadedFileTextBlock.Value?.Substring(ActivityUi.UploadedFileLabel.Length);
                }
                activityStorage.Remove<StandardConfigurationControlsCM>();
                activityStorage.Add(PackControls(new ActivityUi(fileName, uploadFilePath)));
                activityStorage.Remove<StandardTableDataCM>();
                activityStorage.RemoveByLabel(ColumnHeadersCrateLabel);
                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    //activityStorage.Add(Crate.FromContent(GenerateRuntimeCrateLabel(fileName), ExcelUtils.GetTableData(uploadFilePath), AvailabilityType.RunTime));
                    activityStorage.Add(Crate.FromContent(RuntimeCrateLabel, ExcelUtils.GetTableData(uploadFilePath), AvailabilityType.RunTime));
                    activityStorage.Add(Crate.FromContent(ColumnHeadersCrateLabel, ExcelUtils.GetColumnHeadersData(uploadFilePath)));
                }
            }
            return Task.FromResult(curActivityDO);
            /*
            var uploadFilePath = GetUploadFilePath(curActivityDO);
            using (var activityStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
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
                        .FirstOrDefault(x => x.Value != null && x.Value.StartsWith("Uploaded file: "));

                    if (labelControl != null)

                        --------------------------------------------------
                    var activityUi = new ActivityUi();
                    activityUi.ClonePropertiesFrom(activityStorage.FirstCrate<StandardConfigurationControlsCM>().Content);
                    fileName = activityUi.UploadedFileTextBlock.Value?.Substring(ActivityUi.UploadedFileLabel.Length);
                }
                activityStorage.Remove<StandardConfigurationControlsCM>();
                activityStorage.Add(PackControls(new ActivityUi(fileName, uploadFilePath)));
                activityStorage.Remove<StandardTableDataCM>();
                activityStorage.RemoveByLabel(ColumnHeadersCrateLabel);
                if (!string.IsNullOrEmpty(uploadFilePath))
                    {
                    activityStorage.Add(Crate.FromContent(GenerateRuntimeCrateLabel(fileName), ExcelUtils.GetTableData(uploadFilePath), AvailabilityType.RunTime));
                    activityStorage.Add(Crate.FromContent(ColumnHeadersCrateLabel, ExcelUtils.GetColumnHeadersData(uploadFilePath), AvailabilityType.RunTime));
                }
                    }
            return Task.FromResult(curActivityDO);
            */
        }

        private const string RuntimeCrateLabel = "Table Generated From Excel File";

        //private string GenerateRuntimeCrateLabel(string fileName) => $"{RuntimeCrateLabel}";

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
        /*
        private void TransformExcelFileHeadersToStandardTableDataCrate(ICrateStorage storage, string selectedFilePath)
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
                var curCrateDTO = CrateManager.CreateDesignTimeFieldsCrate(
                    "Spreadsheet Column Headers", 
                    AvailabilityType.RunTime,
                    headers.Select(col => new FieldDTO() { Key = col, Value = col, Availability = AvailabilityType.RunTime }).ToArray());

                storage.RemoveByLabel("Spreadsheet Column Headers");
                storage.Add(curCrateDTO);
            }

            //FR-2449 - The use mentioned in this issue involves with Excel File Rows too.
            //Extract the rows and put them in the design time fields.
            var rowsArray = ExcelUtils.GetTabularData(fileAsByteArray, ext);
            if(rowsArray != null)
            {
                var rows = rowsArray.SelectMany(r => r.Value.Select(rv => rv.Item2)).Where(rowValue => !string.IsNullOrEmpty(rowValue));

                var curRowsCrateDTO = CrateManager.CreateDesignTimeFieldsCrate(
                    "Spreadsheet Rows",
                    AvailabilityType.RunTime,
                    rows.Select(col => new FieldDTO() { Key = col, Value = col, Availability = AvailabilityType.RunTime }).ToArray());

                storage.RemoveByLabel("Spreadsheet Rows");
                storage.Add(curRowsCrateDTO);
            }                                
        }*/

        private void CreatePayloadCrate_ExcelRows(ICrateStorage storage, byte[] fileAsByteArray, string extension)
        {

            var headersArray = ExcelUtils.GetColumnHeaders(fileAsByteArray, extension);

            // Fetch rows in Excel file and assign them to the action's crate storage as Standard Table Data crate
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, extension);
            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = ExcelUtils.CreateTableCellPayloadObjects(rowsDictionary, headersArray);
                if (rows != null && rows.Count > 0)
                {
                    storage.Add(CrateManager.CreateStandardTableDataCrate("Standard Data Table", true, rows.ToArray()));
                }
            }
        }

        private string GetUploadFilePath(ActivityDO activityDO)
        {
            var storage = CrateManager.GetStorage(activityDO);
            var activityUi = new ActivityUi();
            activityUi.ClonePropertiesFrom(storage.FirstCrate<StandardConfigurationControlsCM>().Content);
            return activityUi.select_file.Value;
        }
    }

    // For backward compatibility
    public class Extract_Data_v1 : Load_Excel_File_v1 { }
}