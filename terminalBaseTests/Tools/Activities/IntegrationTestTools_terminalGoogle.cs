using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using HealthMonitor.Utility;
using Hub.Managers;
using NUnit.Framework;
using terminalGoogle.Actions;
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

        public async Task<ActivityDTO> AddAndConfigure_SaveToGoogleSheet(
            PlanDTO plan,
            int ordering,
            string manifestTypeToAssert,
            string crateDescriptionLabelToAssert,
            string newSpeadsheetName)
        {
            var saveToGoogleActivity = FixtureData.Save_To_Google_Sheet_v1_InitialConfiguration();
            var activityCategoryParam = new[] { ActivityCategory.Forwarders };
            var activityTemplates = await _baseHubITest.HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(
                        _baseHubITest.GetHubApiBaseUrl() + "webservices/activities", activityCategoryParam);
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name == "Save_To_Google_Sheet");
            saveToGoogleActivity.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.Plan.SubPlans.FirstOrDefault();
            saveToGoogleActivity.ParentPlanNodeId = subPlan.SubPlanId;
            saveToGoogleActivity.RootPlanNodeId = plan.Plan.Id;
            saveToGoogleActivity.Ordering = ordering;

            //call initial configuration to server
            saveToGoogleActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", saveToGoogleActivity);
            saveToGoogleActivity.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            saveToGoogleActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToGoogleActivity);
            var initialcrateStorage = _baseHubITest.Crate.FromDto(saveToGoogleActivity.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaulGoogleAuthTokenExists = stAuthCrate == null;

            Assert.AreEqual(true, defaulGoogleAuthTokenExists, "Save_To_Google_Sheet: GoogleService require authentication. They might be a problem with default authentication tokens and KeyVault authorization mode");

            initialcrateStorage = _baseHubITest.Crate.FromDto(saveToGoogleActivity.CrateStorage);
            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(),
                "Save_To_Google_Sheet: Crate StandardConfigurationControlsCM is missing in API response.");
            //Activity won't be able to run if there is no upstream data
            var upstreamCrateDescriptions = await _baseHubITest.GetRuntimeCrateDescriptionsFromUpstreamActivities(saveToGoogleActivity.Id);
            Assert.Greater(upstreamCrateDescriptions.Count, 0, "Save_To_Google_Sheet: upstream activities didn't provide at least one runtime CrateDescription");
            var expectedCrateDescription = upstreamCrateDescriptions.FirstOrDefault(x => x.ManifestType == manifestTypeToAssert && x.Label == crateDescriptionLabelToAssert);
            Assert.IsNotNull(expectedCrateDescription, "Save_To_Google_Sheet: upstream activities didn't provide expected runtime CrateDescription");
            //Select the expected crate description
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(saveToGoogleActivity))
            {
                var controlsCrate = initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var saveToGoogleActivityUi = new Save_To_Google_Sheet_v1.ActivityUi();
                saveToGoogleActivityUi.SyncWith(controlsCrate.Content);
                crateStorage.Remove<StandardConfigurationControlsCM>();
                saveToGoogleActivityUi.UpstreamCrateChooser.CrateDescriptions = upstreamCrateDescriptions;
                saveToGoogleActivityUi.UpstreamCrateChooser.CrateDescriptions.First(x => x.Label == crateDescriptionLabelToAssert && x.ManifestType == manifestTypeToAssert).Selected = true;
                //Set the name of new spreadheet that need to be created
                saveToGoogleActivityUi.NewSpreadsheetName.Value = newSpeadsheetName;
                crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(saveToGoogleActivityUi.Controls.ToArray()), controlsCrate.Availability));
            }

            saveToGoogleActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", saveToGoogleActivity);
            saveToGoogleActivity.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            saveToGoogleActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToGoogleActivity);

            return await Task.FromResult(saveToGoogleActivity);
        }
    }
}
