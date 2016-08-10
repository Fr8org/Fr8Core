using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Manifests.Helpers;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Infrastructure;
using Google.GData.Client;
using Newtonsoft.Json;
using terminalGoogle.Actions;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;

namespace terminalGoogle.Activities
{
    public class Save_To_Google_Sheet_v1 : BaseGoogleTerminalActivity<Save_To_Google_Sheet_v1.ActivityUi>
    {
        public static ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
        {
            Id = new Guid("120110db-b8dd-41ca-b88e-9865db315528"),
            Name = "Save_To_Google_Sheet",
            Label = "Save To Google Sheet",
            Version = "1",
            Terminal = TerminalData.TerminalDTO,
            NeedsAuthentication = true,
            MinPaneWidth = 300,
            Categories = new[]
            {
                ActivityCategories.Forward,
                TerminalData.GooogleActivityCategoryDTO
            }
        };
        protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;

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

        public Save_To_Google_Sheet_v1(ICrateManager crateManager, IGoogleIntegration googleIntegration, IGoogleSheet googleSheet)
            : base(crateManager, googleIntegration)
        {
            _googleSheet = googleSheet;
        }

        private GoogleAuthDTO GetGoogleAuthToken()
        {
            return JsonConvert.DeserializeObject<GoogleAuthDTO>(AuthorizationToken.Token);
        }

        public override async Task Initialize()
        {
            ActivityUI.ExistingSpreadsheetsList.ListItems = (await _googleSheet.GetSpreadsheets(GetGoogleAuthToken())).Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
        }

        public override async Task FollowUp()
        {
            //If different existing spreadsheet is selected then we have to load worksheet list for it
            if (ActivityUI.UseExistingSpreadsheetOption.Selected && !string.IsNullOrEmpty(ActivityUI.ExistingSpreadsheetsList.Value))
            {
                var previousSpreadsheet = SelectedSpreadsheet;
                if (string.IsNullOrEmpty(previousSpreadsheet) || !string.Equals(previousSpreadsheet, ActivityUI.ExistingSpreadsheetsList.Value))
                {
                    ActivityUI.ExistingWorksheetsList.ListItems = (await _googleSheet.GetWorksheets(ActivityUI.ExistingSpreadsheetsList.Value, GetGoogleAuthToken()))
                        .Select(x => new ListItem { Key = x.Value, Value = x.Key })
                        .ToList();
                    var firstWorksheet = ActivityUI.ExistingWorksheetsList.ListItems.First();
                    ActivityUI.ExistingWorksheetsList.SelectByValue(firstWorksheet.Value);
                }
                SelectedSpreadsheet = ActivityUI.ExistingSpreadsheetsList.Value;
                ActivityUI.ExistingSpreadsheetsList.ListItems = (await _googleSheet.GetSpreadsheets(GetGoogleAuthToken())).Select(x => new ListItem { Key = x.Value, Value = x.Key }).ToList();
            }
            else
            {
                ActivityUI.ExistingWorksheetsList.ListItems.Clear();
                ActivityUI.ExistingWorksheetsList.selectedKey = string.Empty;
                ActivityUI.ExistingWorksheetsList.Value = string.Empty;
                SelectedSpreadsheet = string.Empty;
            }
        }

        protected override Task Validate()
        {
            ValidationManager.ValidateCrateChooserNotEmpty(ActivityUI.UpstreamCrateChooser, "upstream crate is not selected");

            if ((ActivityUI.UseNewSpreadsheetOption.Selected && string.IsNullOrWhiteSpace(ActivityUI.NewSpreadsheetName.Value))
                || (ActivityUI.UseExistingSpreadsheetOption.Selected && string.IsNullOrEmpty(ActivityUI.ExistingSpreadsheetsList.Value)))
            {
                ValidationManager.SetError("Spreadsheet name is not specified", ActivityUI.SpreadsheetSelectionGroup);
            }

            if ((ActivityUI.UseNewWorksheetOption.Selected && string.IsNullOrWhiteSpace(ActivityUI.NewWorksheetName.Value))
                || (ActivityUI.UseExistingWorksheetOption.Selected && string.IsNullOrEmpty(ActivityUI.ExistingWorksheetsList.Value)))
            {
                ValidationManager.SetError("Worksheet name is not specified", ActivityUI.WorksheetSelectionGroup);
            }

            return Task.FromResult(0);
        }
        
        public override async Task Run()
        {
            var crateToProcess = FindCrateToProcess();

            if (crateToProcess == null)
            {
                throw new ActivityExecutionException($"Failed to run {ActivityPayload.Name} because specified upstream crate was not found in payload");
            }

            var tableToSave = StandardTableDataCMTools.ExtractPayloadCrateDataToStandardTableData(crateToProcess);

            try
            {
                var spreadsheetUri = await GetOrCreateSpreadsheet();
                var worksheetUri = await GetOrCreateWorksheet(spreadsheetUri);
                await _googleSheet.WriteData(spreadsheetUri, worksheetUri, tableToSave, GetGoogleAuthToken());
            }
            catch (GDataRequestException ex)
            {
                if (ex.InnerException.Message.IndexOf("(401) Unauthorized") > -1)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException();
                }

                throw;
            }
        }

        private async Task<string> GetOrCreateWorksheet(string spreadsheetUri)
        {
            if (ActivityUI.UseExistingSpreadsheetOption.Selected && ActivityUI.UseExistingWorksheetOption.Selected)
            {
                return ActivityUI.ExistingWorksheetsList.Value;
            }
            var authToken = GetGoogleAuthToken();
            var existingWorksheets = await _googleSheet.GetWorksheets(spreadsheetUri, authToken);
            //If this is a new spreadsheet and user specified to use existing then we just use the first one (as there is only one existing worksheet in new spreadsheet)
            if (ActivityUI.UseExistingWorksheetOption.Selected)
            {
                return existingWorksheets.First().Key;
            }
            var existingWorksheet = existingWorksheets.Where(x => string.Equals(x.Value.Trim(), ActivityUI.NewWorksheetName.Value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                                                      .Select(x => x.Key)
                                                      .FirstOrDefault();
            //If user entered exactly the name of existing worksheet we return it
            if (!string.IsNullOrEmpty(existingWorksheet))
            {
                return existingWorksheet;
            }
            //Anyway create a new worksheet
            var result = await _googleSheet.CreateWorksheet(spreadsheetUri, authToken, ActivityUI.NewWorksheetName.Value);
            //If this is a new name and new worksheet we delete the default one (as there is no sense in keeping it)
            if (ActivityUI.UseNewSpreadsheetOption.Selected && ActivityUI.UseNewWorksheetOption.Selected)
            {
                await _googleSheet.DeleteWorksheet(spreadsheetUri, existingWorksheets.First().Key, authToken);
            }
            return result;
        }

        private async Task<string> GetOrCreateSpreadsheet()
        {
            if (ActivityUI.UseExistingSpreadsheetOption.Selected)
            {
                return ActivityUI.ExistingSpreadsheetsList.Value;
            }
            var authToken = GetGoogleAuthToken();
            var existingSpreadsheets = await _googleSheet.GetSpreadsheets(authToken);
            var existingSpreadsheet = existingSpreadsheets.Where(x => string.Equals(x.Value.Trim(), ActivityUI.NewSpreadsheetName.Value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                                                          .Select(x => x.Key)
                                                          .FirstOrDefault();
            if (!string.IsNullOrEmpty(existingSpreadsheet))
            {
                return existingSpreadsheet;
            }
            return await _googleSheet.CreateSpreadsheet(ActivityUI.NewSpreadsheetName.Value, authToken);
        }

        private Crate FindCrateToProcess()
        {
            var desiredCrateDescription = ActivityUI.UpstreamCrateChooser.CrateDescriptions.Single(x => x.Selected);
            return Payload.FirstOrDefault(x => x.Label == desiredCrateDescription.Label && x.ManifestType.Type == desiredCrateDescription.ManifestType);
        }
    }
}