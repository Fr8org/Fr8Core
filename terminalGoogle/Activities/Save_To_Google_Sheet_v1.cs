using Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using TerminalBase.BaseClasses;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using Data.Interfaces.Manifests;
using Data.Interfaces.DataTransferObjects;
using Data.Crates;
using Data.States;
using Data.Control;
using Data.Interfaces.Manifests.Helpers;
using StructureMap;

namespace terminalGoogle.Actions
{
    public class Save_To_Google_Sheet_v1 : EnhancedTerminalActivity<Save_To_Google_Sheet_v1.ActivityUi>
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

            public ActivityUi()
            {
                UpstreamCrateChooser = new CrateChooser
                                       {
                                           Label = "Crate to store",
                                           Name = nameof(UpstreamCrateChooser),
                                           Required = true,
                                           RequestUpstream = true,
                                           SingleManifestOnly = true,
                                       };
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
                                              Value = "Store in a new Google Spreadsheet",
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

        private readonly IGoogleSheet _googleSheet;
        private readonly IGoogleIntegration _googleIntegration;

        public Save_To_Google_Sheet_v1() : base(true)
        {
            _googleSheet = ObjectFactory.GetInstance<IGoogleSheet>();
            _googleIntegration = ObjectFactory.GetInstance<IGoogleIntegration>();
            ActivityName = "Save To Google Sheet";
        }

        private GoogleAuthDTO GetGoogleAuthToken(AuthorizationTokenDO authTokenDO = null)
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>((authTokenDO ?? AuthorizationToken).Token);
        }

        public override bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (base.NeedsAuthentication(authTokenDO))
            {
                return true;
            }
            var token = GetGoogleAuthToken(authTokenDO);

            // Post token to google api to check its validity
            // Variable needs for more readability.
            var result = Task.Run(async () => await _googleIntegration.IsTokenInfoValid(token)).Result;
            return !result;
        }

        protected override async Task Initialize(RuntimeCrateManager runtimeCrateManager)
        {
            ConfigurationControls.ExistingSpreadsheetsList.ListItems = (await _googleSheet.GetSpreadsheets(GetGoogleAuthToken())).Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
        }

        protected override async Task Configure(RuntimeCrateManager runtimeCrateManager)
        {
            //If different existing spreadsheet is selected then we have to load worksheet list for it
            if (ConfigurationControls.UseExistingSpreadsheetOption.Selected && !string.IsNullOrEmpty(ConfigurationControls.ExistingSpreadsheetsList.Value))
            {
                var previousSpreadsheet = SelectedSpreadsheet;
                if (string.IsNullOrEmpty(previousSpreadsheet) || !string.Equals(previousSpreadsheet, ConfigurationControls.ExistingSpreadsheetsList.Value))
                {
                    ConfigurationControls.ExistingWorksheetsList.ListItems = (await _googleSheet.GetWorksheets(ConfigurationControls.ExistingSpreadsheetsList.Value, GetGoogleAuthToken()))
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
            var tableToSave = StandardTableDataCMTools.ExtractPayloadCrateDataToStandardTableData(crateToProcess);
            var spreadsheetUri = await GetOrCreateSpreadsheet();
            var worksheetUri = await GetOrCreateWorksheet(spreadsheetUri);
            await _googleSheet.WriteData(spreadsheetUri, worksheetUri, tableToSave, GetGoogleAuthToken());
        }

        private async Task<string> GetOrCreateWorksheet(string spreadsheetUri)
        {
            if (ConfigurationControls.UseExistingSpreadsheetOption.Selected && ConfigurationControls.UseExistingWorksheetOption.Selected)
            {
                return ConfigurationControls.ExistingWorksheetsList.Value;
            }
            var authToken = GetGoogleAuthToken();
            var existingWorksheets = await _googleSheet.GetWorksheets(spreadsheetUri, authToken);
            //If this is a new spreadsheet and user specified to use existing then we just use the first one (as there is only one existing worksheet in new spreadsheet)
            if (ConfigurationControls.UseExistingWorksheetOption.Selected)
            {
                return existingWorksheets.First().Key;
            }
            var existingWorksheet = existingWorksheets.Where(x => string.Equals(x.Value.Trim(), ConfigurationControls.NewWorksheetName.Value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                                                      .Select(x => x.Key)
                                                      .FirstOrDefault();
            //If user entered exactly the name of existing worksheet we return it
            if (!string.IsNullOrEmpty(existingWorksheet))
            {
                return existingWorksheet;
            }
            //Anyway create a new worksheet
            var result = await _googleSheet.CreateWorksheet(spreadsheetUri, authToken, ConfigurationControls.NewWorksheetName.Value);
            //If this is a new name and new worksheet we delete the default one (as there is no sense in keeping it)
            if (ConfigurationControls.UseNewSpreadsheetOption.Selected && ConfigurationControls.UseNewWorksheetOption.Selected)
            {
                await _googleSheet.DeleteWorksheet(spreadsheetUri, existingWorksheets.First().Key, authToken);
            }
            return result;
        }

        private async Task<string> GetOrCreateSpreadsheet()
        {
            if (ConfigurationControls.UseExistingSpreadsheetOption.Selected)
            {
                return ConfigurationControls.ExistingSpreadsheetsList.Value;
            }
            var authToken = GetGoogleAuthToken();
            var existingSpreadsheets = await _googleSheet.GetSpreadsheets(authToken);
            var existingSpreadsheet = existingSpreadsheets.Where(x => string.Equals(x.Value.Trim(), ConfigurationControls.NewSpreadsheetName.Value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                                                          .Select(x => x.Key)
                                                          .FirstOrDefault();
            if (!string.IsNullOrEmpty(existingSpreadsheet))
            {
                return existingSpreadsheet;
            }
            return await _googleSheet.CreateSpreadsheet(ConfigurationControls.NewSpreadsheetName.Value, authToken);
        }

        private Crate FindCrateToProcess()
        {
            var desiredCrateDescription = ConfigurationControls.UpstreamCrateChooser.CrateDescriptions.Single(x => x.Selected);
            return CurrentPayloadStorage.FirstOrDefault(x => x.Label == desiredCrateDescription.Label && x.ManifestType.Type == desiredCrateDescription.ManifestType);
        }
    }
}