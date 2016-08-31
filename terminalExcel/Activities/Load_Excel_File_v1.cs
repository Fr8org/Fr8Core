using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using StructureMap;
using terminalUtilities;
using terminalUtilities.Excel;

namespace terminalExcel.Activities
{
    public class Load_Excel_File_v1 : TerminalActivity<Load_Excel_File_v1.ActivityUi>
    {
        private readonly ExcelUtils _excelUtils;

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

        public Load_Excel_File_v1(ICrateManager crateManager, ExcelUtils excelUtils)
            : base(crateManager)
        {
            _excelUtils = excelUtils;
        }

        public override async Task Initialize()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel);
        }

        public override async Task FollowUp()
        {
            Storage.RemoveByLabel(ColumnHeadersCrateLabel);
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
                    var selectedFileDescription = new KeyValueDTO(ActivityUI.FilePicker.Value, ExtractFileName(ActivityUI.FilePicker.Value));
                   
                    CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel, true)
                        .AddFields((await _excelUtils.GetColumnHeaders(selectedFileDescription.Key)).Select(x => new FieldDTO(x)));

                    ActivityUI.MarkFileAsUploaded(selectedFileDescription.Value, selectedFileDescription.Key);
                    SelectedFileDescription = selectedFileDescription;

                    // Process table and get the Table and optionally (if one row) fields crate
                    var fileAsByteArray = await _excelUtils.GetExcelFileAsByteArray(ActivityUI.FilePicker.Value);
                    var tableCrates = GetExcelFileDescriptionCrates(fileAsByteArray, ActivityUI.FilePicker.Value, null, false);

                    foreach (var crate in tableCrates)
                    {
                        Storage.ReplaceByLabel(crate);
                    }

                    Storage.ReplaceByLabel(CreateExternalObjectHandlesCrate());
                }
            }
            
            Storage.Add(Crate.FromContent(FileCrateLabel, new StandardFileDescriptionCM() { Filename = FileCrateLabel }));
        }

        public List<Crate> GetExcelFileDescriptionCrates(byte[] fileAsByteArray, string selectedFilePath, string sheetName = null, bool isRunTime = false)
        {
            var ext = Path.GetExtension(selectedFilePath);
            List<Crate> crates = new List<Crate>();

            // Read file from repository
            // Fetch column headers in Excel file
            var headersArray = ExcelUtils.GetColumnHeaders(fileAsByteArray, ext, sheetName);

            var isFirstRowAsColumnNames = true;
            var hasFirstRowHeader = ExcelUtils.DetectContainsHeader(fileAsByteArray, ext, sheetName);

            // Fetch rows in Excel file
            var rowsDictionary = ExcelUtils.GetTabularData(fileAsByteArray, ext, isFirstRowAsColumnNames, sheetName);

            Crate tableCrate = Crate.FromContent(RunTimeCrateLabel, new StandardTableDataCM(isFirstRowAsColumnNames, new TableRowDTO[0])); // default one

            if (rowsDictionary != null && rowsDictionary.Count > 0)
            {
                var rows = ExcelUtils.CreateTableCellPayloadObjects(rowsDictionary, headersArray, isFirstRowAsColumnNames);
                if (rows != null && rows.Count > 0)
                {
                    tableCrate = Crate.FromContent(RunTimeCrateLabel, new StandardTableDataCM(hasFirstRowHeader, rows));
                    var fieldsCrate = TabularUtilities.PrepareFieldsForOneRowTable(isFirstRowAsColumnNames, rows, headersArray);
                    if (fieldsCrate != null)
                    {
                        crates.Add(fieldsCrate);
                    }
                }
            }
            crates.Add(tableCrate);
            return crates;
        }

        public override async Task Run()
        {
            if (string.IsNullOrEmpty(ActivityUI.FilePicker.Value))
            {
                RaiseError("Excel file is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
                return;
            }

            var byteArray = await _excelUtils.GetExcelFileAsByteArray(ActivityUI.FilePicker.Value);
            var tableCrates = GetExcelFileDescriptionCrates(byteArray, ActivityUI.FilePicker.Value, null, true);

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
            Payload.Add(Crate.FromContent(FileCrateLabel, fileDescription));
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
                new ExternalObjectHandlesCM(externalObjectHandle)
            );

            return crate;
        }

        private KeyValueDTO SelectedFileDescription
        {
            get
            {
                var storedValues = Storage.FirstCrateOrDefault<KeyValueListCM>(x => x.Label == ConfigurationCrateLabel)?.Content;
                return storedValues?.Values.First();
            }
            set
            {
                if (value == null)
                {
                    Storage.RemoveByLabel(ConfigurationCrateLabel);
                    return;
                }
      
                Storage.ReplaceByLabel(Crate.FromContent(ConfigurationCrateLabel, new KeyValueListCM(value)));
            }
        }
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("df2df85f-9364-48af-aa97-bb8adccc91d7"),
            Name = "Load_Excel_File",
            Label = "Load Excel File",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            Tags = string.Join(",", Tags.TableDataGenerator, Tags.Getter),
            MinPaneWidth = 300,
            Categories = new[]
            {
                ActivityCategories.Receive,
                TerminalData.ActivityCategoryDTO
            }
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
    public class Extract_Data_v1 : Load_Excel_File_v1
    {
        public Extract_Data_v1(ICrateManager crateManager, ExcelUtils excelUtils) 
            : base(crateManager, excelUtils)
        {
        }
    }
}