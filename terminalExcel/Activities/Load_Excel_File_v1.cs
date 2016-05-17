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
        }

        protected override async Task InitializeETA()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        protected override async Task ConfigureETA()
        {
            CurrentActivityStorage.RemoveByLabel(ColumnHeadersCrateLabel);
            //If file is not uploaded we hide file description
            if (string.IsNullOrEmpty(ActivityUI.FilePicker.Value))
            {
                Storage.RemoveByLabel(ColumnHeadersCrateLabel);
                Storage.RemoveByLabel(TabularUtilities.ExtractedFieldsCrateLabel);
                ActivityUI.ClearFileDescription();
                SelectedFileDescription = null;
            }
            else
            {
                var previousValues = SelectedFileDescription;
                //Update column header only if new file was uploaded
                if (previousValues == null || previousValues.Key != ActivityUI.FilePicker.Value)
                {
                    Storage.RemoveByLabel(ColumnHeadersCrateLabel);
                    Storage.RemoveByLabel(TabularUtilities.ExtractedFieldsCrateLabel);
                    var selectedFileDescription = new FieldDTO(ActivityUI.FilePicker.Value, ExtractFileName(ActivityUI.FilePicker.Value));
                    var columnHeadersCrate = Crate.FromContent(
                        ColumnHeadersCrateLabel,
                        ExcelUtils.GetColumnHeadersData(selectedFileDescription.Key,ColumnHeadersCrateLabel),
                        AvailabilityType.Always
                    );

                    ActivityUI.MarkFileAsUploaded(selectedFileDescription.Value, selectedFileDescription.Key);
                    Storage.ReplaceByLabel(columnHeadersCrate);
                    SelectedFileDescription = selectedFileDescription;

                    // Process table and get the Table and optionally (if one row) fields crate
                    var fileAsByteArray = ExcelUtils.GetExcelFileAsByteArray(ConfigurationControls.FilePicker.Value);
                    var tableCrates = GetExcelFileDescriptionCrates(fileAsByteArray, ConfigurationControls.FilePicker.Value, true, null, false);

                    foreach (var crate in tableCrates)
                    {
                        Storage.ReplaceByLabel(crate);
                    }

                    Storage.ReplaceByLabel(CreateExternalObjectHandlesCrate());
                }
            }

            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
            Storage.Add(Crate.FromContent(FileCrateLabel, new StandardFileDescriptionCM() { Filename = FileCrateLabel }));
        }

        public List<Crate> GetExcelFileDescriptionCrates(byte[] fileAsByteArray, string selectedFilePath, bool isFirstRowAsColumnNames = true, string sheetName = null, bool isRunTime = false)
        {
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();

            var ext = Path.GetExtension(selectedFilePath);
            List<Crate> crates = new List<Crate>();

            // Read file from repository
            // Fetch column headers in Excel file
            var headersArray = ExcelUtils.GetColumnHeaders(fileAsByteArray, ext, sheetName);

            // Fetch rows in Excel file
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, ext, isFirstRowAsColumnNames, sheetName);

            Crate tableCrate = crateManager.CreateStandardTableDataCrate(RunTimeCrateLabel, isFirstRowAsColumnNames, new TableRowDTO[] { } ); // default one

            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = ExcelUtils.CreateTableCellPayloadObjects(rowsDictionary, headersArray, isFirstRowAsColumnNames);
                if (rows != null && rows.Count > 0)
                {
                    tableCrate = crateManager.CreateStandardTableDataCrate(RunTimeCrateLabel, isFirstRowAsColumnNames, rows.ToArray());
                    var fieldsCrate = TabularUtilities.PrepareFieldsForOneRowTable(isFirstRowAsColumnNames, isRunTime, rows, headersArray);
                    if (fieldsCrate != null)
                    {
                        crates.Add(fieldsCrate);
                    }
                }
            }
            crates.Add(tableCrate);
            return crates;
        }

        protected override async Task RunETA()
        {
            if (string.IsNullOrEmpty(ActivityUI.FilePicker.Value))
            {
                RaiseError("Excel file is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }

            var byteArray = ExcelUtils.GetExcelFileAsByteArray(ConfigurationControls.FilePicker.Value);
            var tableCrates = GetExcelFileDescriptionCrates(byteArray, ConfigurationControls.FilePicker.Value, true, null, true);

            var fileDescription = new StandardFileDescriptionCM
            {
                TextRepresentation = Convert.ToBase64String(byteArray),
                Filetype = Path.GetExtension(ActivityUI.FilePicker.Value),
                Filename = Path.GetFileName(ActivityUI.FilePicker.Value)
            };
            foreach (var crate in tableCrates)
            {
                Payload.Add(crate);
            }
            Payload.Add(Crate.FromContent(FileCrateLabel, fileDescription, AvailabilityType.Always));
        }

        private Crate CreateExternalObjectHandlesCrate()
        {
            var fileName = ExtractFileName(ActivityUI.FilePicker.Value);

            var externalObjectHandle = new ExternalObjectHandleDTO()
            {
                Name = fileName,
                Description = $"Table Data from Excel File '{fileName}'",
                DirectUrl = ActivityUI.FilePicker.Value,
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
                var storedValues = Storage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Fields.First();
            }
            set
            {
                if (value == null)
                {
                    Storage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
                value.Availability = AvailabilityType.Configuration;
                Storage.ReplaceByLabel(Crate.FromContent(ConfigurationCrateLabel, new FieldDescriptionsCM(value), AvailabilityType.Configuration));
            }
        }
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Name = "Load_Excel_File",
            Label = "Load Excel File",
            Version = "1",
            Category = ActivityCategory.Receivers,
            Terminal = TerminalData.TerminalDTO,
            Tags = "Table Data Generator,Getter",
            MinPaneWidth = 300,
            WebService = TerminalData.WebServiceDTO
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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