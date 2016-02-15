using Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
using Hub.Managers;
using Data.Interfaces.Manifests;
using Data.Interfaces.DataTransferObjects;
using Data.Crates;
using Data.States;
using Data.Control;
namespace terminalGoogle.Actions
{
    public class Save_In_Google_Sheet_v1 : BaseTerminalActivity
    {
        private readonly IGoogleSheet _googleSheet;
        private string _spreedsheetUri = "";

        public Save_In_Google_Sheet_v1()
        {
            _googleSheet = new GoogleSheet();
        }

        #region Overriden Methods
        protected new bool NeedsAuthentication(AuthorizationTokenDO authTokenDO)
        {
            if (authTokenDO == null) return true;
            if (!base.NeedsAuthentication(authTokenDO))
                return false;
            var token = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            // we may also post token to google api to check its validity
            return (token.Expires - DateTime.Now > TimeSpan.FromMinutes(5) ||
                    !string.IsNullOrEmpty(token.RefreshToken));
        }

        public override Task<ActivityDO> Configure(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            CheckAuthentication(authTokenDO);

            return base.Configure(curActivityDO, authTokenDO);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (!CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Clear();
                crateStorage.Add(CreateControlsCrate());
            }
            await AddCrateDesignTimeFieldsSource(curActivityDO);
            await AddSpreadsheetDesignTimeFieldsSource(curActivityDO, authTokenDO);
            await AddUpstreamManifestSource(curActivityDO);
            await AddUpstreamLabelSource(curActivityDO);

            return curActivityDO;
        }

        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var payloadCrates = await GetPayload(curActivityDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payloadCrates);
            }

            //get payload crates for data
            StandardTableDataCM standardTableCM = await ExtractDataFromUpstreamCrates("UpstreamCrateChooser", curActivityDO);
            if(standardTableCM.Table.Count > 0)
            {
                try
                {
                    //uploadspreadsheet
                    var uploadedSpreadsheet = await UploadSpreadsheet(curActivityDO, authTokenDO);
                    if (String.IsNullOrEmpty(uploadedSpreadsheet.Key))
                        throw new ArgumentNullException("Please select a spreadsheet to upload.");

                    //uploadworksheet
                    var worksheets = UploadWorksheet(curActivityDO, authTokenDO, uploadedSpreadsheet);
                    if (String.IsNullOrEmpty(worksheets.Key))
                        throw new ArgumentNullException("Please select a worksheet(pane).");

                    //get worksheet
                    //write data into worksheet
                    _googleSheet.WriteData(uploadedSpreadsheet.Key, worksheets.Key, standardTableCM, authDTO);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }

            return Success(payloadCrates);
        }

        protected override async Task<ActivityDO> FollowupConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            UpdateWorksheetFields(curActivityDO, authTokenDO);

            return await Task.FromResult(curActivityDO);
        }
        #endregion

        #region Configuration Controls
        private Crate CreateControlsCrate()
        {
            var controls = new List<ControlDefinitionDTO>()
            {
                CreateUpstreamCrateChooser("UpstreamCrateChooser", "Store which Crates?"),
                CreateSpreadsheetControls(),
                CreateWorksheetControls()
            };

            return CrateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel, controls.ToArray());
        }

        private ControlDefinitionDTO CreateSpreadsheetControls()
        {
            var templateSpreadsheet = new RadioButtonGroup()
            {
                GroupName = "SpreadsheetGroup",
                Name = "SpreadsheetGroup",
                Events = new List<ControlEvent> { new ControlEvent("onChange", "requestConfig") },
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "newSpreadsheet",
                        Value = "Store in a new Google Spreadsheet",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new TextBox()
                            {
                                Label = "",
                                Value = string.Format("NewFr8Data{0:dd-MM-yyyy}", DateTime.Now.Date),
                                Name = "NewSpreadsheetText"
                            }
                        }
                    },

                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "existingSpreadsheet",
                        Value = "Store in an existing Spreadsheet",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new DropDownList()
                            {
                                Label = "",
                                Name = "ExistingSpreadsheetDropdown",
                                Source = new FieldSourceDTO
                                {
                                    Label = "Spreadsheet Fields",
                                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                },
                                Events = new List<ControlEvent> {new ControlEvent("onChange", "requestConfig")}
                            }
                        }
                    }
                }
            };

            return templateSpreadsheet;
        }

        private ControlDefinitionDTO CreateWorksheetControls()
        {
            var templateWorksheet = new RadioButtonGroup()
            {
                Label = "Inside the spreadsheet, store in",
                GroupName = "WorksheetGroup",
                Name = "WorksheetGroup",
                Radios = new List<RadioButtonOption>()
                {
                    new RadioButtonOption()
                    {
                        Selected = true,
                        Name = "newWorksheet",
                        Value = "A new Sheet (Pane)",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new TextBox()
                            {
                                Label = "",
                                Value = "Sheet1",
                                Name = "NewWorksheetText"
                            }
                        }
                    },

                    new RadioButtonOption()
                    {
                        Selected = false,
                        Name = "existingWorksheet",
                        Value = "Existing Pane",
                        Controls = new List<ControlDefinitionDTO>
                        {
                            new DropDownList()
                            {
                                Label = "",
                                Name = "ExistingWorksheetDropdown",
                                Source = new FieldSourceDTO
                                {
                                    Label = "Worksheet Fields",
                                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                                }
                            }
                        }
                    }
                }
            };

            return templateWorksheet;
        }

        private async Task<ActivityDO> AddCrateDesignTimeFieldsSource(ActivityDO curActivityDO)
        {
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("Store which Crates?");

                var mergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActivityDO, "Available Run-Time Objects");
                FieldDTO[] upstreamLabels = mergedUpstreamRunTimeObjects.Content.
                    Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();

                var upstreamLabelsCrate = CrateManager.CreateDesignTimeFieldsCrate("Store which Crates?", upstreamLabels);
                crateStorage.Add(upstreamLabelsCrate);
            }

            return curActivityDO;
        }

        private async Task<ActivityDO> AddSpreadsheetDesignTimeFieldsSource(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var spreadsheets = _googleSheet.EnumerateSpreadsheetsUris(authDTO);

            var fields = spreadsheets.Select(x => new FieldDTO() { Key = x.Value, Value = x.Key, Availability = AvailabilityType.Configuration }).ToArray();
            var createDesignTimeFields = CrateManager.CreateDesignTimeFieldsCrate(
                "Spreadsheet Fields",
                AvailabilityType.Configuration,
                fields);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("Spreadsheet Fields");

                crateStorage.Add(createDesignTimeFields);
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        private async Task<ActivityDO> AddWorksheetDesignTimeFieldsSource(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var worksheet = _googleSheet.EnumerateWorksheet(_spreedsheetUri, authDTO);

            var fields = worksheet.Select(x => new FieldDTO() { Key = x.Value, Value = x.Key, Availability = AvailabilityType.Configuration }).ToArray();
            var createDesignTimeFields = CrateManager.CreateDesignTimeFieldsCrate(
                "Worksheet Fields",
                AvailabilityType.Configuration,
                fields);

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("Worksheet Fields");

                crateStorage.Add(createDesignTimeFields);
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        private async Task<ActivityDO> AddUpstreamManifestSource(ActivityDO curActivityDO)
        {
            //add upstream crates 
            var manifest = await GetUpstreamManifestListCrate(curActivityDO);
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("AvailableUpstreamManifests");

                crateStorage.Add(manifest);
            }

            return curActivityDO;
        }

        private async Task<ActivityDO> AddUpstreamLabelSource(ActivityDO curActivityDO)
        {
            var labels = await GetUpstreamCrateLabelListCrate(curActivityDO);
            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.RemoveByLabel("AvailableUpstreamLabels");

                crateStorage.Add(labels);
            }

            return curActivityDO;
        }
        #endregion

        #region Helper Methods
        private async Task<FieldDTO> UploadSpreadsheet(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActivityDO);
            FieldDTO uploadedSpreadSheet = new FieldDTO();
            uploadedSpreadSheet.Availability = AvailabilityType.Configuration;

            if (configurationControls != null)
            {
                var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);

                var spreadSheetGroupControl = configurationControls.Controls.OfType<RadioButtonGroup>()
                    .Where(w => w.Name == "SpreadsheetGroup").SelectMany(s => s.Radios);

                //Create spreadsheet if its new

                //check if its a textbox selection
                if(spreadSheetGroupControl.Where(w => w.Name == "newSpreadsheet").FirstOrDefault().Selected == true)
                {
                    var newSpreadSheetTextbox = spreadSheetGroupControl.Where(w => w.Name == "newSpreadsheet").FirstOrDefault().Controls.OfType<TextBox>().FirstOrDefault();

                    //get spreadsheets
                    var existingSpreadSheets = _googleSheet.EnumerateSpreadsheetsUris(authDTO);

                    //check if spreadsheet name in textbox is present in spreadsheets 
                    //if spreadsheet name already exist do nothing to upload
                    var existingSpreadsheet = existingSpreadSheets.Where(w => w.Value.ToLower().Trim() == newSpreadSheetTextbox.Value.ToLower().Trim()).FirstOrDefault();
                    if (!String.IsNullOrEmpty(existingSpreadsheet.Key))
                    {
                        uploadedSpreadSheet.Key = existingSpreadsheet.Key;
                        uploadedSpreadSheet.Value = existingSpreadsheet.Value;
                    }
                    else
                    {
                        uploadedSpreadSheet.Key = await _googleSheet.CreateSpreadsheet(newSpreadSheetTextbox.Value, authDTO);
                        uploadedSpreadSheet.Value = newSpreadSheetTextbox.Value;
                    }
                }
                else
                {
                    //for dropdown get selected key and value
                    var existingSpreadsheetRadioOption = spreadSheetGroupControl.Where(w => w.Name == "existingSpreadsheet").FirstOrDefault();

                    var dropDownSpreadsheet = existingSpreadsheetRadioOption.Controls.OfType<DropDownList>().FirstOrDefault();

                    if (dropDownSpreadsheet != null)
                    {
                        if (existingSpreadsheetRadioOption.Selected == true)
                        {
                            //if spreadsheet is existing, update worksheet dropdown
                            if (!String.IsNullOrEmpty(dropDownSpreadsheet.Value))
                            {
                                uploadedSpreadSheet.Key = dropDownSpreadsheet.Value; //spreadsheet URI
                                uploadedSpreadSheet.Value = dropDownSpreadsheet.selectedKey;
                            }
                        }
                    }

                }
            }
            return uploadedSpreadSheet;
        }

        private FieldDTO UploadWorksheet(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO, FieldDTO uploadedSpreadSheet)
        {
            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActivityDO);
            FieldDTO uploadedWorksheet = new FieldDTO();
            uploadedWorksheet.Availability = AvailabilityType.Configuration;

            if (configurationControls != null)
            {
                var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);

                var spreadSheetGroupControl = configurationControls.Controls.OfType<RadioButtonGroup>()
                    .Where(w => w.Name == "WorksheetGroup").SelectMany(s => s.Radios);

                //Create worksheet if its new

                //check if its a textbox selection
                if (spreadSheetGroupControl.Where(w => w.Name == "newWorksheet").FirstOrDefault().Selected == true)
                {
                    var newWorksheetTextbox = spreadSheetGroupControl.Where(w => w.Name == "newWorksheet").FirstOrDefault().Controls.OfType<TextBox>().FirstOrDefault();

                    //get worksheets
                    var existingWorksheets = _googleSheet.EnumerateWorksheet(uploadedSpreadSheet.Key, authDTO);
                    
                    //check if worksheet name in textbox is present in worksheets 
                    //if worksheet name already exist do nothing to upload
                    var existingWorksheet = existingWorksheets.Where(w => w.Value.ToLower().Trim() == newWorksheetTextbox.Value.ToLower().Trim()).FirstOrDefault();
                    if (!String.IsNullOrEmpty(existingWorksheet.Key))
                    {
                        uploadedWorksheet.Key = existingWorksheet.Key;
                        uploadedWorksheet.Value = existingWorksheet.Value;
                    }
                    else
                    {
                        uploadedWorksheet.Key = _googleSheet.CreateWorksheet(uploadedSpreadSheet.Key, authDTO, newWorksheetTextbox.Value);
                        uploadedWorksheet.Value = newWorksheetTextbox.Value;
                    }
                }
                else
                {
                    //for dropdown get selected key and value
                    var existingWorksheetRadioOption = spreadSheetGroupControl.Where(w => w.Name == "existingWorksheet").FirstOrDefault();

                    var dropDownWorksheet = existingWorksheetRadioOption.Controls.OfType<DropDownList>().FirstOrDefault();

                    if (dropDownWorksheet != null)
                    {
                        if (existingWorksheetRadioOption.Selected == true)
                        {
                            //if spreadsheet is existing, update worksheet dropdown
                            if (!String.IsNullOrEmpty(dropDownWorksheet.Value))
                            {
                                uploadedWorksheet.Key = dropDownWorksheet.Value; //spreadsheet URI
                                uploadedWorksheet.Value = dropDownWorksheet.selectedKey;
                            }
                        }
                    }
                }
            }
            return uploadedWorksheet;
        }

        private async Task UpdateWorksheetFields(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActivityDO);
            if (configurationControls != null)
            {
                var existingSpreadsheetRadioOption = configurationControls.Controls.OfType<RadioButtonGroup>()
                    .Where(w => w.Name == "SpreadsheetGroup").SelectMany(s => s.Radios)
                    .Where(w => w.Name == "existingSpreadsheet").FirstOrDefault();

                var dropDownSpreadsheet = existingSpreadsheetRadioOption.Controls.OfType<DropDownList>().FirstOrDefault();

                if (dropDownSpreadsheet != null)
                {
                    if (existingSpreadsheetRadioOption.Selected == false)
                    {
                        //if spreadsheet is new Clear out worksheet dropdown and select new worksheet
                        using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                        {
                            crateStorage.RemoveByLabel("Worksheet Fields");

                            configurationControls.Controls[2] = CreateWorksheetControls();
                            crateStorage.ReplaceByLabel(CrateManager.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel, configurationControls.Controls.ToArray()));
                        }
                    }
                    else
                    {
                        //if spreadsheet is existing, update worksheet dropdown
                        if (!String.IsNullOrEmpty(dropDownSpreadsheet.Value))
                        {
                            _spreedsheetUri = dropDownSpreadsheet.Value;
                            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
                            {
                                crateStorage.RemoveByLabel("Worksheet Fields");

                            }
                            await AddWorksheetDesignTimeFieldsSource(curActivityDO, authTokenDO);
                        }
                    }
                }
            }
        }

        #endregion
    }
}