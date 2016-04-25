using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.Interfaces.Manifests.Helpers;
using Data.States;
using terminalUtilities.Excel;
using TerminalBase.BaseClasses;

namespace terminalExcel.Actions
{
    public class Save_To_Excel_v1 : EnhancedTerminalActivity<Save_To_Excel_v1.ActivityUi>
    {
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
                        "This Loop will process the data inside of",
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


        public Save_To_Excel_v1() : base(true)
        {
            ActivityName = "Save To Excel";
        }

        public override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            return false;
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.ExistingSpreadsheetsList.ListItems = await GetCurrentUsersFiles();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {

            //If different existing spreadsheet is selected then we have to load worksheet list for it
            if (ConfigurationControls.UseExistingSpreadsheetOption.Selected && !string.IsNullOrEmpty(ConfigurationControls.ExistingSpreadsheetsList.Value))
            {
                var previousSpreadsheet = SelectedSpreadsheet;
                if (string.IsNullOrEmpty(previousSpreadsheet) || !string.Equals(previousSpreadsheet, ConfigurationControls.ExistingSpreadsheetsList.Value))
                {
                    ConfigurationControls.ExistingWorksheetsList.ListItems = (
                        await GetWorksheets(
                            int.Parse(ConfigurationControls.ExistingSpreadsheetsList.Value),
                            ConfigurationControls.ExistingSpreadsheetsList.selectedKey)
                        )
                        .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                        .ToList();
                    var firstWorksheet = ConfigurationControls.ExistingWorksheetsList.ListItems.First();
                    ConfigurationControls.ExistingWorksheetsList.SelectByValue(firstWorksheet.Value);
                }
                SelectedSpreadsheet = ConfigurationControls.ExistingSpreadsheetsList.Value;
            }
            else
            {
                ConfigurationControls.ExistingWorksheetsList.ListItems.Clear();
                ConfigurationControls.ExistingWorksheetsList.selectedKey = string.Empty;
                ConfigurationControls.ExistingWorksheetsList.Value = string.Empty;
                SelectedSpreadsheet = string.Empty;
            }
        }

        private async Task<List<ListItem>> GetWorksheets(int fileId, string fileName)
        {
            //let's download this file
            Stream file = await HubCommunicator.DownloadFile(fileId, CurrentFr8UserId);
            var fileBytes = ExcelUtils.StreamToByteArray(file);

            //TODO: Optimize this to retrieve spreadsheet list only. Now it reads and loads into memory whole file. 
            var spreadsheetList = ExcelUtils.GetSpreadsheets(fileBytes, Path.GetExtension(fileName));
            return spreadsheetList.Select(s => new ListItem() { Key = s.Key.ToString(), Value = s.Value }).ToList();
        }

        private string SelectedSpreadsheet
        {
            get
            {
                var storedValue = CurrentActivityStorage.FirstCrateOrDefault<FieldDescriptionsCM>(x => x.Label == SelectedSpreadsheetCrateLabel);
                return storedValue?.Content.Fields.First().Key;
            }
            set
            {
                CurrentActivityStorage.RemoveByLabel(SelectedSpreadsheetCrateLabel);
                if (string.IsNullOrEmpty(value))
                {
                    return;
                }
                CurrentActivityStorage.Add(Crate<FieldDescriptionsCM>.FromContent(SelectedSpreadsheetCrateLabel, new FieldDescriptionsCM(new FieldDTO(value)), AvailabilityType.Configuration));
            }
        }

        protected override async Task RunCurrentActivity()
        {
            if (!ConfigurationControls.UpstreamCrateChooser.CrateDescriptions.Any(x => x.Selected))
            {
                throw new ActivityExecutionException($"Failed to run {ActivityName} because upstream crate is not selected", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if ((ConfigurationControls.UseNewSpreadsheetOption.Selected && string.IsNullOrWhiteSpace(ConfigurationControls.NewSpreadsheetName.Value))
                || (ConfigurationControls.UseExistingSpreadsheetOption.Selected && string.IsNullOrEmpty(ConfigurationControls.ExistingSpreadsheetsList.Value)))
            {
                throw new ActivityExecutionException($"Failed to run {ActivityName} because spreadsheet name is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            if ((ConfigurationControls.UseNewWorksheetOption.Selected && string.IsNullOrWhiteSpace(ConfigurationControls.NewWorksheetName.Value))
                || (ConfigurationControls.UseExistingWorksheetOption.Selected && string.IsNullOrEmpty(ConfigurationControls.ExistingWorksheetsList.Value)))
            {
                throw new ActivityExecutionException($"Failed to run {ActivityName} because worksheet name is not specified", ActivityErrorCode.DESIGN_TIME_DATA_MISSING);
            }
            var crateToProcess = FindCrateToProcess();
            if (crateToProcess == null)
            {
                throw new ActivityExecutionException($"Failed to run {ActivityName} because specified upstream crate was not found in payload");
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

            if (ConfigurationControls.UseNewSpreadsheetOption.Selected)
            {
                fileData = ExcelUtils.CreateExcelFile(
                    tableToSave,
                    ConfigurationControls.NewWorksheetName.Value
                );

                fileName = ConfigurationControls.NewSpreadsheetName.Value;
            }
            else
            {
                var existingFileStream = await HubCommunicator.DownloadFile(
                    Int32.Parse(ConfigurationControls.ExistingSpreadsheetsList.Value),
                    CurrentFr8UserId
                );

                byte[] existingFileBytes;
                using (var memStream = new MemoryStream())
                {
                    await existingFileStream.CopyToAsync(memStream);
                    existingFileBytes = memStream.ToArray();
                }

                fileName = ConfigurationControls.ExistingSpreadsheetsList.selectedKey;

                var worksheetName = ConfigurationControls.UseNewWorksheetOption.Selected
                    ? ConfigurationControls.NewWorksheetName.Value
                    : ConfigurationControls.ExistingWorksheetsList.selectedKey;

                StandardTableDataCM dataToInsert;
                if (ConfigurationControls.UseExistingWorksheetOption.Selected
                    || ConfigurationControls.ExistingWorksheetsList.ListItems.Any(x => x.Key == ConfigurationControls.NewWorksheetName.Value))
                {
                    var existingData = ExcelUtils.GetExcelFile(existingFileBytes, fileName, true, worksheetName);

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

                var file = await HubCommunicator.SaveFile(fileName, stream, CurrentFr8UserId);
                return file.CloudStorageUrl;
            }
        }

        private async Task<List<ListItem>> GetCurrentUsersFiles()
        {
            var curAccountFileList = await HubCommunicator.GetFiles(CurrentFr8UserId);
            //TODO where tags == Docusign files
            return curAccountFileList.Select(c => new ListItem() { Key = c.OriginalFileName, Value = c.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
        }

        private Crate FindCrateToProcess()
        {
            var desiredCrateDescription = ConfigurationControls.UpstreamCrateChooser.CrateDescriptions.Single(x => x.Selected);
            return CurrentPayloadStorage.FirstOrDefault(x => x.Label == desiredCrateDescription.Label && x.ManifestType.Type == desiredCrateDescription.ManifestType);
        }

        private async Task PushLaunchURLNotification(string url)
        {
            await PushUserNotification(new TerminalNotificationDTO
            {
                Type = "Success",
                ActivityName = "Save_To_Excel",
                ActivityVersion = "1",
                TerminalName = "terminalFr8Core",
                TerminalVersion = "1",
                Message = $"The Excel file can be downloaded by navigating to this URL: {url}",
                //"api/v1/plans/clone?id=" + curActivityDO.RootPlanNodeId,
                Subject = "Excel File"
            });
        }

    }
}