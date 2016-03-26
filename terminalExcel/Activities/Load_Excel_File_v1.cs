using System;
using System.Collections.Generic;
using System.Linq;
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
using terminalUtilities.Excel;
using Utilities;

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

        private const string RunTimeCrateLabel = "Table Generated From Load Excel File";

        private const string ConfigurationCrateLabel = "Selected File & Worksheet";

        private const string ColumnHeadersCrateLabel = "Spreadsheet Column Headers";

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
                    var columnHeadersCrate = Crate.FromContent(ColumnHeadersCrateLabel,
                                                               ExcelUtils.GetColumnHeadersData(selectedFileDescription.Key),
                                                               AvailabilityType.Always);
                    ConfigurationControls.MarkFileAsUploaded(selectedFileDescription.Value, selectedFileDescription.Key);
                    CurrentActivityStorage.ReplaceByLabel(columnHeadersCrate);
                    SelectedFileDescription = selectedFileDescription;
                }
            }
            runtimeCrateManager.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task RunCurrentActivity()
        {
            if (string.IsNullOrEmpty(ConfigurationControls.FilePicker.Value))
            {
                throw new ActivityExecutionException("Excel file is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            CurrentPayloadStorage.Add(Crate.FromContent(RunTimeCrateLabel, ExcelUtils.GetTableData(ConfigurationControls.FilePicker.Value), AvailabilityType.RunTime));
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