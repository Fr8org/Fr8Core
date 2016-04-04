using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using HealthMonitor.Utility;
using Hub.Managers;
using Newtonsoft.Json;
using NUnit.Framework;
using terminalGoogle.Actions;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services;
using UtilitiesTesting.Fixtures;

namespace terminaBaselTests.Tools.Activities
{
    public class IntegrationTestTools_terminalGoogle
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalGoogle(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        public async Task<ActivityDTO> AddAndConfigureSaveToGoogleSheet(PlanDTO plan,
                                                                        int ordering,
                                                                        string manifestTypeToUse,
                                                                        string crateDescriptionLabelToUse,
                                                                        string newSpeadsheetName)
        {
            var activityName = "Save_To_Google_Sheet";
            var saveToGoogleSheetActivityDTO = await AddGoogleActivityToPlan(plan, ordering, ActivityCategory.Forwarders, activityName);
            //Activity won't be able to run if there is no upstream data
            var upstreamCrateDescriptions = await _baseHubITest.GetRuntimeCrateDescriptionsFromUpstreamActivities(saveToGoogleSheetActivityDTO.Id);
            Assert.Greater(upstreamCrateDescriptions.Count, 0, $"{activityName}: upstream activities didn't provide at least one runtime CrateDescription");
            var expectedCrateDescription = upstreamCrateDescriptions.FirstOrDefault(x => x.ManifestType == manifestTypeToUse && x.Label == crateDescriptionLabelToUse);
            Assert.IsNotNull(expectedCrateDescription, $"{activityName}: upstream activities didn't provide expected runtime CrateDescription");
            //Select the expected crate description
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(saveToGoogleSheetActivityDTO))
            {
                var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var activityUi = new Save_To_Google_Sheet_v1.ActivityUi();
                activityUi.SyncWith(controlsCrate.Content);
                crateStorage.Remove<StandardConfigurationControlsCM>();
                activityUi.UpstreamCrateChooser.CrateDescriptions = upstreamCrateDescriptions;
                activityUi.UpstreamCrateChooser.CrateDescriptions.First(x => x.Label == crateDescriptionLabelToUse && x.ManifestType == manifestTypeToUse).Selected = true;
                //Set the name of new spreadheet that need to be created
                activityUi.NewSpreadsheetName.Value = newSpeadsheetName;
                crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }

            saveToGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", saveToGoogleSheetActivityDTO);
            saveToGoogleSheetActivityDTO.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            saveToGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToGoogleSheetActivityDTO);

            return saveToGoogleSheetActivityDTO;
        }

        public async Task<ActivityDTO> AddAndConfigureGetFromGoogleSheet(PlanDTO plan,int ordering, string spreadsheetName, bool includeFixtureAuthToken)
        {
            var activityName = "Get_Google_Sheet_Data";

            var getFromGoogleSheetActivityDTO = await AddGoogleActivityToPlan(plan, ordering, ActivityCategory.Receivers, activityName);

            return await ConfigureGetFromGoogleSheetActivity(getFromGoogleSheetActivityDTO, spreadsheetName, includeFixtureAuthToken);
        }

        public async Task<ActivityDTO> ConfigureGetFromGoogleSheetActivity(ActivityDTO getFromGoogleSheetActivityDTO, string spreadsheetName, bool includeFixtureAuthToken, string worksheetName = null)
        {
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(getFromGoogleSheetActivityDTO))
            {
                var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var activityUi = new Get_Google_Sheet_Data_v1.ActivityUi();
                activityUi.SyncWith(controlsCrate.Content);
                crateStorage.Remove<StandardConfigurationControlsCM>();
                var spreadsheetUri = activityUi.SpreadsheetList.ListItems.Where(x => x.Key == spreadsheetName).Select(x => x.Value).FirstOrDefault();
                Assert.IsNotNullOrEmpty(spreadsheetUri, $"Default Google account doesn't contain spreadsheet '{spreadsheetName}'");
                activityUi.SpreadsheetList.selectedKey = spreadsheetName;
                activityUi.SpreadsheetList.Value = spreadsheetUri;

                crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
            }

            if (!string.IsNullOrEmpty(worksheetName))
            {
                getFromGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", getFromGoogleSheetActivityDTO);
                getFromGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", getFromGoogleSheetActivityDTO);

                using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(getFromGoogleSheetActivityDTO))
                {
                    var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                    var activityUi = new Get_Google_Sheet_Data_v1.ActivityUi();
                    activityUi.SyncWith(controlsCrate.Content);
                    crateStorage.Remove<StandardConfigurationControlsCM>();

                    var worksheetUri = activityUi.WorksheetList.ListItems.Where(x => x.Key == worksheetName).Select(x => x.Value).FirstOrDefault();
                    Assert.IsNotNullOrEmpty(worksheetUri, $"Default Google account doesn't contain worksheet '{worksheetName}' for the spreadsheet '{spreadsheetName}'");
                    activityUi.WorksheetList.selectedKey = worksheetName;
                    activityUi.WorksheetList.Value = worksheetUri;
                    
                    crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray()), controlsCrate.Availability));
                }
            }

            getFromGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", getFromGoogleSheetActivityDTO);

            if (includeFixtureAuthToken)
            {
                getFromGoogleSheetActivityDTO.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            }
            getFromGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", getFromGoogleSheetActivityDTO);
            return getFromGoogleSheetActivityDTO;
        }

        private async Task<ActivityDTO> AddGoogleActivityToPlan(PlanDTO plan, int ordering, ActivityCategory activityCategory, string activityName)
        {
            var googleActivityDTO = FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration();
            var activityCategoryParam = new[] { activityCategory };
            var activityTemplates = await _baseHubITest.HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(
                                                                                                                          _baseHubITest.GetHubApiBaseUrl() + "webservices/activities", activityCategoryParam);
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name == activityName);
            googleActivityDTO.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.Plan.SubPlans.FirstOrDefault();
            googleActivityDTO.ParentPlanNodeId = subPlan.SubPlanId;
            googleActivityDTO.RootPlanNodeId = plan.Plan.Id;
            googleActivityDTO.Ordering = ordering;

            //call initial configuration to server
            googleActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", googleActivityDTO);
            googleActivityDTO.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            googleActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", googleActivityDTO);
            var initialcrateStorage = _baseHubITest.Crate.FromDto(googleActivityDTO.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaulGoogleAuthTokenExists = stAuthCrate == null;

            Assert.AreEqual(true, defaulGoogleAuthTokenExists, $"{activityName}: GoogleService require authentication. They might be a problem with default authentication tokens and KeyVault authorization mode");

            initialcrateStorage = _baseHubITest.Crate.FromDto(googleActivityDTO.CrateStorage);
            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), $"{activityName}: Crate StandardConfigurationControlsCM is missing in API response.");
            return googleActivityDTO;
        }
    }
}
