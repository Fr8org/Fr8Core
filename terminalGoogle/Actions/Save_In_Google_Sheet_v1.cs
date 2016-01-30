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
            var spreadsheetsFromUserSelectionControl = FindControl(Crate.GetStorage(curActivityDO.CrateStorage), "Spreadsheet");

            var hasDesignTimeFields = Crate.GetStorage(curActivityDO)
                .Any(x => x.IsOfType<StandardConfigurationControlsCM>());

            if (hasDesignTimeFields && !string.IsNullOrEmpty(spreadsheetsFromUserSelectionControl.Value))
            {
                _spreedsheetUri = spreadsheetsFromUserSelectionControl.Value;
                return ConfigurationRequestType.Followup;
            }

            return ConfigurationRequestType.Initial;
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Clear();
                updater.CrateStorage.Add(CreateControlsCrate());
                await AddCrateDesignTimeFieldsSource(curActivityDO);
            }

            return await Task.FromResult(curActivityDO);
        }

        private Crate CreateControlsCrate()
        {
            var controls = new List<ControlDefinitionDTO>()
            {
                CreateUpstreamCrateChooser("UpstreamCrateChooser", "Store which Crates?"),
                CreateSpecificOrUpstreamValueChooser("Spreadsheet", "Spreadsheet", "Spreadsheet Fields"),
                CreateSpecificOrUpstreamValueChooser("Worksheet", "Worksheet", "Worksheet Fields")
            };

            return Crate.CreateStandardConfigurationControlsCrate(ConfigurationControlsLabel, controls.ToArray());
        }

        private async Task<ActivityDO> AddCrateDesignTimeFieldsSource(ActivityDO curActivityDO)
        {
            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.RemoveByLabel("Store which Crates?");

                var mergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActivityDO, "Available Run-Time Objects");
                FieldDTO[] upstreamLabels = mergedUpstreamRunTimeObjects.Content.
                    Fields.Select(field => new FieldDTO { Key = field.Key, Value = field.Value }).ToArray();

                var upstreamLabelsCrate = Crate.CreateDesignTimeFieldsCrate("Store which Crates?", upstreamLabels);
                updater.CrateStorage.Add(upstreamLabelsCrate);
            }

            return curActivityDO;
        }

        private async Task<ActivityDO> AddSpreadsheetDesignTimeFieldsSource(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var spreadsheets = _googleSheet.EnumerateSpreadsheetsUris(authDTO);

            var fields = spreadsheets.Select(x => new FieldDTO() { Key = x.Key, Value = x.Value, Availability = AvailabilityType.Configuration }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Spreadsheet Fields",
                AvailabilityType.Configuration,
                fields);

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.RemoveByLabel("Spreadsheet Fields");

                updater.CrateStorage.Add(createDesignTimeFields);
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        private async Task<ActivityDO> AddWorksheetDesignTimeFieldsSource(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var worksheet = _googleSheet.EnumerateWorksheet(_spreedsheetUri, authDTO);

            var fields = worksheet.Select(x => new FieldDTO() { Key = x.Key, Value = x.Value, Availability = AvailabilityType.Configuration }).ToArray();
            var createDesignTimeFields = Crate.CreateDesignTimeFieldsCrate(
                "Worksheet Fields",
                AvailabilityType.Configuration,
                fields);

            using (var updater = Crate.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.RemoveByLabel("Worksheet Fields");

                updater.CrateStorage.Add(createDesignTimeFields);
            }

            return await Task.FromResult<ActivityDO>(curActivityDO);
        }

        private Dictionary<string, string> GetControlsValue(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            CheckAuthentication(authTokenDO);

            StandardConfigurationControlsCM configurationControls = GetConfigurationControls(curActivityDO);
            if (configurationControls == null)
            {
                var storage = Crate.GetStorage(curActivityDO);
                configurationControls = storage.CrateContentsOfType<StandardConfigurationControlsCM>(c => String.Equals(c.Label, "SendGrid", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            }

            var crateStorage = Crate.GetStorage(curActivityDO.CrateStorage);

            var spreadsheetField = (TextSource)GetControl(configurationControls, "Spreadsheet", ControlTypes.TextSource);
            var worksheetField = (TextSource)GetControl(configurationControls, "Worksheet", ControlTypes.TextSource);

            values.Add("Spreadsheet", spreadsheetField.GetValue(crateStorage));
            values.Add("Worksheet", worksheetField.GetValue(crateStorage));

            return values;
        }

        public override async Task<ActivityDO> Activate(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var authDTO = JsonConvert.DeserializeObject<GoogleAuthDTO>(authTokenDO.Token);
            var values = GetControlsValue(curActivityDO, authTokenDO);

            //Create spreadsheet if its new
            var spreadSheet = _googleSheet.FindSpreadsheet(values["Spreadsheet"], authDTO);
            string spreadSheetId = "";
            if (spreadSheet == null)
            {
                spreadSheetId = await _googleSheet.CreateSpreadsheet(values["Spreadsheet"], authDTO);
            }else
                spreadSheetId = values["Spreadsheet"];

            //Create worksheet if its new 
            var worksheets = _googleSheet.EnumerateWorksheet(spreadSheetId, authDTO);



            return curActivityDO;
        }
    }
}