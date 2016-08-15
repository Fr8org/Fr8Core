using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Manifests.Helpers;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.BaseClasses;
using Fr8.TerminalBase.Services;
using terminalUtilities.Excel;
using Fr8.TerminalBase.Infrastructure;

namespace terminalExcel.Actions
{
    public class Save_To_Excel_v1 : TerminalActivity<Save_To_Excel_v1.ActivityUi>
    {
        private readonly ExcelUtils _excelUtils;
        private readonly IPushNotificationService _pushNotificationService;

        public class ActivityUi : StandardConfigurationControlsCM
        {
            public CrateChooser UpstreamCrateChooser { get; set; }

            public RadioButtonGroup SpreadsheetSelectionGroup { get; set; }

            public RadioButtonOption UseNewSpreadsheetOption { get; set; }

            public TextBox NewSpreadsheetName { get; set; }

            public RadioButtonOption UseExistingSpreadsheetOption { get; set; }

            public DropDownList ExistingSpreadsheetsList { get; set; }

            public RadioButtonGroup WorksheetSelectionGroup { get; set; }

            public RadioButtonOption UseNewWorksheetOption { get; set; }

            public TextBox NewWorksheetName { get; set; }

            public RadioButtonOption UseExistingWorksheetOption { get; set; }

            public DropDownList ExistingWorksheetsList { get; set; }

            public ActivityUi(UiBuilder builder)
            {
                UpstreamCrateChooser = builder.CreateCrateChooser(
                        "Available_Crates",
                        "Save which data:",
                        true,
                        requestConfig: true
                    );

                Controls.Add(UpstreamCrateChooser);
                NewSpreadsheetName = new TextBox
                                     {
                                         Value = $"NewFr8Data{DateTime.Now.Date:dd-MM-yyyy}",
                                         Name = nameof(NewSpreadsheetName)
                                     };
                ExistingSpreadsheetsList = new DropDownList
                                           {
                                               Name = nameof(ExistingSpreadsheetsList),
                                               Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                                           };
                UseNewSpreadsheetOption = new RadioButtonOption
                                          {
                                              Selected = true,
                                              Name = nameof(UseNewSpreadsheetOption),
                                              Value = "Store in a new Excel Spreadsheet",
                                              Controls = new List<ControlDefinitionDTO> { NewSpreadsheetName }
                                          };
                UseExistingSpreadsheetOption = new RadioButtonOption()
                                               {
                                                   Selected = false,
                                                   Name = nameof(UseExistingSpreadsheetOption),
                                                   Value = "Store in an existing Spreadsheet",
                                                   Controls = new List<ControlDefinitionDTO> { ExistingSpreadsheetsList }
                                               };
                SpreadsheetSelectionGroup = new RadioButtonGroup
                                            {
                                                GroupName = nameof(SpreadsheetSelectionGroup),
                                                Name = nameof(SpreadsheetSelectionGroup),
                                                Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                                                Radios = new List<RadioButtonOption>
                                                         {
                                                             UseNewSpreadsheetOption,
                                                             UseExistingSpreadsheetOption
                                                         }
                                            };
                Controls.Add(SpreadsheetSelectionGroup);
                NewWorksheetName = new TextBox
                                   {
                                       Value = "Sheet1",
                                       Name = nameof(NewWorksheetName)
                                   };
                ExistingWorksheetsList = new DropDownList
                                         {
                                             Name = nameof(ExistingWorksheetsList),
                                         };
                UseNewWorksheetOption = new RadioButtonOption()
                                        {
                                            Selected = true,
                                            Name = nameof(UseNewWorksheetOption),
                                            Value = "A new Sheet (Pane)",
                                            Controls = new List<ControlDefinitionDTO> { NewWorksheetName }
                                        };
                UseExistingWorksheetOption = new RadioButtonOption()
                                             {
                                                 Selected = false,
                                                 Name = nameof(UseExistingWorksheetOption),
                                                 Value = "Existing Pane",
                                                 Controls = new List<ControlDefinitionDTO> { ExistingWorksheetsList }
                                             };
                WorksheetSelectionGroup = new RadioButtonGroup()
                                          {
                                              Label = "Inside the spreadsheet, store in",
                                              GroupName = nameof(WorksheetSelectionGroup),
                                              Name = nameof(WorksheetSelectionGroup),
                                              Radios = new List<RadioButtonOption>
                                                       {
                                                           UseNewWorksheetOption,
                                                           UseExistingWorksheetOption
                                                       }
                                          };
                Controls.Add(WorksheetSelectionGroup);
            }
        }

        private const string SelectedSpreadsheetCrateLabel = "Selected Spreadsheet";

        public Save_To_Excel_v1(ICrateManager crateManager, ExcelUtils excelUtils, IPushNotificationService pushNotificationService)
            : base(crateManager)
        {
            _excelUtils = excelUtils;
            _pushNotificationService = pushNotificationService;
        }

        public override async Task Initialize()
        {
            CrateSignaller.MarkAvailableAtRuntime<StandardFileDescriptionCM>("StoredFile");
            ActivityUI.ExistingSpreadsheetsList.ListItems = await GetCurrentUsersFiles();
        }

        public override async Task FollowUp()
        {
            //If different existing spreadsheet is selected then we have to load worksheet list for it
            if (ActivityUI.UseExistingSpreadsheetOption.Selected && !string.IsNullOrEmpty(ActivityUI.ExistingSpreadsheetsList.Value))
            {
                var previousSpreadsheet = SelectedSpreadsheet;
                if (string.IsNullOrEmpty(previousSpreadsheet) || !string.Equals(previousSpreadsheet, ActivityUI.ExistingSpreadsheetsList.Value))
                {
                    ActivityUI.ExistingWorksheetsList.ListItems = (
                        await GetWorksheets(
                            int.Parse(ActivityUI.ExistingSpreadsheetsList.Value),
                            ActivityUI.ExistingSpreadsheetsList.selectedKey)
                        )
                        .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                        .ToList();
                    var firstWorksheet = ActivityUI.ExistingWorksheetsList.ListItems.First();
                    ActivityUI.ExistingWorksheetsList.SelectByValue(firstWorksheet.Value);
                }
                SelectedSpreadsheet = ActivityUI.ExistingSpreadsheetsList.Value;
            }
            else
            {
                ActivityUI.ExistingWorksheetsList.ListItems.Clear();
                ActivityUI.ExistingWorksheetsList.selectedKey = string.Empty;
                ActivityUI.ExistingWorksheetsList.Value = string.Empty;
                SelectedSpreadsheet = string.Empty;
            }
        }

        private async Task<List<ListItem>> GetWorksheets(int fileId, string fileName)
        {
            // Let's download this file
            Stream file = await HubCommunicator.DownloadFile(fileId);
            var fileBytes = ExcelUtils.StreamToByteArray(file);

            //TODO: Optimize this to retrieve spreadsheet list only. Now it reads and loads into memory whole file. 
            var spreadsheetList = ExcelUtils.GetSpreadsheets(fileBytes, Path.GetExtension(fileName));
            return spreadsheetList.Select(s => new ListItem() { Key = s.Key.ToString(), Value = s.Value }).ToList();
        }

        private string SelectedSpreadsheet
        {
            get
            {
                var storedValue = Storage.FirstCrateOrDefault<KeyValueListCM>(x => x.Label == SelectedSpreadsheetCrateLabel);
                return storedValue?.Content.Values.First().Key;
            }
            set
            {
                Storage.RemoveByLabel(SelectedSpreadsheetCrateLabel);
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                Storage.Add(Crate<KeyValueListCM>.FromContent(SelectedSpreadsheetCrateLabel, new KeyValueListCM(new KeyValueDTO(value, value))));
            }
        }

        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("f3c99f97-e6e2-4343-b592-6674ac5b4c16"),
            Name = "Save_To_Excel",
            Label = "Save to Excel",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            MinPaneWidth = 300,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.ActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

        protected override Task Validate()
        {
            ValidationManager.ValidateCrateChooserNotEmpty(ActivityUI.UpstreamCrateChooser, "A selection must be made.");
            return Task.FromResult(0);
        }

        public override async Task Run()
        {
            if (!ActivityUI.UpstreamCrateChooser.CrateDescriptions.Any(x => x.Selected))
            {
                RaiseError($"Failed to run {ActivityTemplateDTO.Name} because upstream crate is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if ((ActivityUI.UseNewSpreadsheetOption.Selected && string.IsNullOrWhiteSpace(ActivityUI.NewSpreadsheetName.Value))
                || (ActivityUI.UseExistingSpreadsheetOption.Selected && string.IsNullOrEmpty(ActivityUI.ExistingSpreadsheetsList.Value)))
            {
                RaiseError($"Failed to run {ActivityTemplateDTO.Name} because spreadsheet name is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if ((ActivityUI.UseNewWorksheetOption.Selected && string.IsNullOrWhiteSpace(ActivityUI.NewWorksheetName.Value))
                || (ActivityUI.UseExistingWorksheetOption.Selected && string.IsNullOrEmpty(ActivityUI.ExistingWorksheetsList.Value)))
            {
                RaiseError($"Failed to run {ActivityTemplateDTO.Name} because worksheet name is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var crateToProcess = FindCrateToProcess();
            if (crateToProcess == null)
            {
                RaiseError($"Failed to run {ActivityTemplateDTO.Name} because specified upstream crate was not found in payload");
            }

            var tableToSave = StandardTableDataCMTools
                .ExtractPayloadCrateDataToStandardTableData(crateToProcess);
            var url = await AppendOrCreateSpreadsheet(tableToSave);
            await PushLaunchURLNotification(url);
        }

        private async Task<string> AppendOrCreateSpreadsheet(StandardTableDataCM tableToSave)
        {
            byte[] fileData;
            string fileName;

            if (ActivityUI.UseNewSpreadsheetOption.Selected)
            {
                fileData = ExcelUtils.CreateExcelFile(
                    tableToSave,
                    ActivityUI.NewWorksheetName.Value
                );

                fileName = ActivityUI.NewSpreadsheetName.Value;
            }
            else
            {
                var existingFileStream = await HubCommunicator.DownloadFile(
                    int.Parse(ActivityUI.ExistingSpreadsheetsList.Value)
                );

                byte[] existingFileBytes;
                using (var memStream = new MemoryStream())
                {
                    await existingFileStream.CopyToAsync(memStream);
                    existingFileBytes = memStream.ToArray();
                }

                fileName = ActivityUI.ExistingSpreadsheetsList.selectedKey;

                var worksheetName = ActivityUI.UseNewWorksheetOption.Selected
                    ? ActivityUI.NewWorksheetName.Value
                    : ActivityUI.ExistingWorksheetsList.selectedKey;

                StandardTableDataCM dataToInsert;
                if (ActivityUI.UseExistingWorksheetOption.Selected
                    || ActivityUI.ExistingWorksheetsList.ListItems.Any(x => x.Key == ActivityUI.NewWorksheetName.Value))
                {
                    var existingData = _excelUtils.GetExcelFile(existingFileBytes, fileName, true, worksheetName);

                    StandardTableDataCMTools.AppendToStandardTableData(existingData, tableToSave);
                    dataToInsert = existingData;
                }
                else
                {
                    dataToInsert = tableToSave;
                }

                fileData = ExcelUtils.RewriteSheetForFile(
                    existingFileBytes,
                    dataToInsert,
                    worksheetName
                );
            }

            using (var stream = new MemoryStream(fileData, false))
            {
                if (!fileName.ToUpper().EndsWith(".XLSX"))
                {
                    fileName += ".xlsx";
                }

                var file = await HubCommunicator.SaveFile(fileName, stream);
                Payload.Add(Crate.FromContent("StoredFile", new StandardFileDescriptionCM
                {
                    Filename = file.Id.ToString(), // dirty hack
                    TextRepresentation = file.OriginalFileName, // another hack
                    Filetype = ".xlsx"
                }));
				return file.CloudStorageUrl;
            }
        }

        private async Task<List<ListItem>> GetCurrentUsersFiles()
        {
            //Leave only XLSX files as activity fails to rewrite XLS files
            var curAccountFileList = (await HubCommunicator.GetFiles()).Where(x => x.OriginalFileName?.EndsWith(".xlsx", StringComparison.InvariantCultureIgnoreCase) ?? true);
            //TODO where tags == Docusign files
            return curAccountFileList.Select(c => new ListItem() { Key = c.OriginalFileName, Value = c.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
        }

        private Crate FindCrateToProcess()
        {
            var desiredCrateDescription = ActivityUI.UpstreamCrateChooser.CrateDescriptions.Single(x => x.Selected);
            return Payload.FirstOrDefault(x => x.Label == desiredCrateDescription.Label && x.ManifestType.Type == desiredCrateDescription.ManifestType);
        }

        private async Task PushLaunchURLNotification(string url)
        {
            await _pushNotificationService.PushUserNotification(MyTemplate, "Excel File URL Generated", $"The Excel file can be downloaded by navigating to this URL: {new Uri(url).AbsoluteUri}");
        }
    }
}