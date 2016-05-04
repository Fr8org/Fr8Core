using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalUtilities.Excel;
using TerminalBase.BaseClasses;

namespace terminalExcel.Actions
{
    public class Load_Excel_File_v1 : EnhancedTerminalActivity<Load_Excel_File_v1.ActivityUi>
    {
        public class ActivityUi : StandardConfigurationControlsCM
        {
            public FilePicker FilePicker { get; set; }

            public TextBlock UploadedFileDescription { get; set; }

            public TextBlock ActivityDescription { get; set; }

            public ActivityUi()
            {
                FilePicker = new FilePicker
                {
                    Label = "Select an Excel file",
                    Name = nameof(FilePicker),
                    Required = true,
                    Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                };
                Controls.Add(FilePicker);
                UploadedFileDescription = new TextBlock
                {
                    Name = nameof(UploadedFileDescription),
                    IsHidden = true
                };
                Controls.Add(UploadedFileDescription);
                ActivityDescription = new TextBlock
                {
                    Name = nameof(ActivityDescription),
                    Value = "This action will try to extract a table of rows from the first worksheet in the file. The rows should have a header row"
                };
                Controls.Add(ActivityDescription);
            }

            public void MarkFileAsUploaded(string fileName, string filePath)
            {
                FilePicker.Value = filePath;
                UploadedFileDescription.Value = $"Uploaded file: {fileName}";
                UploadedFileDescription.IsHidden = false;
            }

            public void ClearFileDescription()
            {
                UploadedFileDescription.Value = string.Empty;
                UploadedFileDescription.IsHidden = true;
            }
        }

        private const string FileCrateLabel = "File uploaded by Load Excel";

        private const string RunTimeCrateLabel = "Table Generated From Load Excel File";

        private const string ConfigurationCrateLabel = "Selected File & Worksheet";

        private const string ColumnHeadersCrateLabel = "Spreadsheet Column Headers";

        private const string ExternalObjectHandlesLabel = "External Object Handles";


        public Load_Excel_File_v1() : base(false)
        {
            ActivityName = "Load Excel File";
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            CurrentActivityStorage.RemoveByLabel(ColumnHeadersCrateLabel);
            //If file is not uploaded we hide file description
            if (string.IsNullOrEmpty(ConfigurationControls.FilePicker.Value))
            {
                ConfigurationControls.ClearFileDescription();
                SelectedFileDescription = null;
            }
            else
            {
                var previousValues = SelectedFileDescription;
                //Update column header only if new file was uploaded
                if (previousValues == null || previousValues.Key != ConfigurationControls.FilePicker.Value)
                {
                    var selectedFileDescription = new FieldDTO(ConfigurationControls.FilePicker.Value, ExtractFileName(ConfigurationControls.FilePicker.Value));
                    var columnHeadersCrate = Crate.FromContent(
                        ColumnHeadersCrateLabel,
                        await Task.Run(() => ExcelUtils.GetColumnHeadersData(selectedFileDescription.Key)),
                        AvailabilityType.Always
                    );

                    ConfigurationControls.MarkFileAsUploaded(selectedFileDescription.Value, selectedFileDescription.Key);
                    CurrentActivityStorage.ReplaceByLabel(columnHeadersCrate);
                    SelectedFileDescription = selectedFileDescription;

                    //lets publish table manifest
                    var byteArray = ExcelUtils.GetExcelFileAsByteArray(ConfigurationControls.FilePicker.Value);
                    var tableData = ExcelUtils.GetExcelFile(byteArray, ConfigurationControls.FilePicker.Value);
                    CurrentActivityStorage.ReplaceByLabel(Crate.FromContent(RunTimeCrateLabel, tableData, AvailabilityType.Always));

                    CurrentActivityStorage.ReplaceByLabel(CreateExternalObjectHandlesCrate());
                }
            }

            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
            CurrentActivityStorage.Add(Crate.FromContent(FileCrateLabel, new StandardFileDescriptionCM() { Filename = FileCrateLabel }));
        }

        protected override async Task RunCurrentActivity()
        {
            if (string.IsNullOrEmpty(ConfigurationControls.FilePicker.Value))
            {
                throw new ActivityExecutionException("Excel file is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            var byteArray = ExcelUtils.GetExcelFileAsByteArray(ConfigurationControls.FilePicker.Value);
            var payload = ExcelUtils.GetExcelFile(byteArray, ConfigurationControls.FilePicker.Value);
            var fileDescription = new StandardFileDescriptionCM
            {
                TextRepresentation = Convert.ToBase64String(byteArray),
                Filetype = Path.GetExtension(ConfigurationControls.FilePicker.Value),
                Filename = Path.GetFileName(ConfigurationControls.FilePicker.Value)
            };
            CurrentPayloadStorage.Add(Crate.FromContent(RunTimeCrateLabel, payload, AvailabilityType.RunTime));
            CurrentPayloadStorage.Add(Crate.FromContent(FileCrateLabel, fileDescription, AvailabilityType.Always));
        }

        private Crate CreateExternalObjectHandlesCrate()
        {
            var fileName = ExtractFileName(ConfigurationControls.FilePicker.Value);

            var externalObjectHandle = new ExternalObjectHandleDTO()
            {
                Name = fileName,
                Description = $"Table Data from Excel File '{fileName}'",
                DirectUrl = ConfigurationControls.FilePicker.Value,
                ManifestType = ManifestDiscovery.Default.GetManifestType<StandardTableDataCM>().Type
            };

            var crate = Crate.FromContent(
                ExternalObjectHandlesLabel,
                new ExternalObjectHandlesCM(externalObjectHandle),
                AvailabilityType.Always
            );

            return crate;
        }

        private FieldDTO SelectedFileDescription
        {
            get
            {
                var storedValues = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Fields.First();
            }
            set
            {
                if (value == null)
                {
                    CurrentActivityStorage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
                value.Availability = AvailabilityType.Configuration;
                CurrentActivityStorage.ReplaceByLabel(Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration));
            }
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
    }

    // For backward compatibility
    public class Extract_Data_v1 : Load_Excel_File_v1 { }
}