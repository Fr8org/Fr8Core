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
                    Controls.Add(UploadedFileTextBlock =  new TextBlock
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

        public Load_Excel_File_v1() : base("Load Excel File") { }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payloadCrates = await GetPayload(curActivityDO, containerId);
            var tableData = CrateManager.GetStorage(curActivityDO)
                                        .FirstCrateOrDefault<StandardTableDataCM>()
                                        ?.Content;
            if (tableData == null)
            {
                return Error(payloadCrates, "No Standard Table Data Manifest exists in activity storage. Probably Excel file is not uploaded or is empty", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if (!tableData.FirstRowHeaders)
            {
                return Error(payloadCrates, "No headers found in the Standard Table Data Manifest.");
            }
            // Create a crate of payload data by using Standard Table Data manifest and use its contents to tranform into a Payload Data manifest.
            // Add a crate of PayloadData to action's crate storage
            using (var crateStorage = CrateManager.GetUpdatableStorage(payloadCrates))
            {
                crateStorage.Add(Crate.FromContent(GenerateRuntimeCrateLabel(ExtractFileName(GetUploadFilePath(curActivityDO))), tableData, AvailabilityType.RunTime));
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
            }
            return Task.FromResult(curActivityDO);
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
                    fileName = activityUi.UploadedFileTextBlock.Value.Substring(ActivityUi.UploadedFileLabel.Length);
                }
                activityStorage.Remove<StandardConfigurationControlsCM>();
                activityStorage.Add(PackControls(new ActivityUi(fileName, uploadFilePath)));
                activityStorage.Remove<StandardTableDataCM>();
                if (!string.IsNullOrEmpty(uploadFilePath))
                {
                    activityStorage.Add(Crate.FromContent(GenerateRuntimeCrateLabel(fileName), ExcelUtils.GetTableData(uploadFilePath), AvailabilityType.RunTime));
                }
            }
            return Task.FromResult(curActivityDO);
        }

        private const string RuntimeCrateLabelPrefix = "Excel Data from ";

        protected override IEnumerable<CrateDescriptionDTO> GetRuntimeAvailableCrateDescriptions(ActivityDO curActivityDO)
        {
            yield return new CrateDescriptionDTO
            {
                Label = GenerateRuntimeCrateLabel(ExtractFileName(GetUploadFilePath(curActivityDO))),
                ManifestId = (int)MT.StandardTableData,
                ManifestType = MT.StandardTableData.GetEnumDisplayName(),
                ProducedBy = ActivityName
            };
        }

        private string GenerateRuntimeCrateLabel(string fileName) => $"{RuntimeCrateLabelPrefix} {fileName}";

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