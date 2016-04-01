using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using HealthMonitor.Utility;
using Hub.Managers;
using NUnit.Framework;
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

        public async Task<ActivityDTO> AddAndConfigure_SaveToGoogleSheet(PlanDTO plan, int ordering,
             string manufestTypeToAssert, string crateDescriptionLabelToAssert, string newSpeadsheetName)
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
            saveToGoogleActivity =await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToGoogleActivity);
            var initialcrateStorage = _baseHubITest.Crate.FromDto(saveToGoogleActivity.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaulGoogleAuthTokenExists = stAuthCrate == null;

            Assert.AreEqual(true, defaulGoogleAuthTokenExists, "Save_To_Google_Sheet: GoogleService require authentication. They might be a problem with default authentication tokens and KeyVault authorization mode");

            initialcrateStorage = _baseHubITest.Crate.FromDto(saveToGoogleActivity.CrateStorage);
            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(),
                "Save_To_Google_Sheet: Crate StandardConfigurationControlsCM is missing in API response.");

            var controlsCrate = initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            //set the value of folder to drafts and 
            var controls = controlsCrate.Content.Controls;

            var crateChooser = controls.SingleOrDefault(x => x.Type == ControlTypes.CrateChooser && x.Name == "UpstreamCrateChooser") as CrateChooser;

            Assert.NotNull(crateChooser, "Save_To_Google_Sheet: CrateChooser for upstream crate values is missing from configuration");
            Assert.AreEqual(1, crateChooser.CrateDescriptions.Count, "Save_To_Google_Sheet CrateChooser control doesn't contain any CrateDescriptions from upstream activities");
            Assert.AreEqual(manufestTypeToAssert, crateChooser.CrateDescriptions[0].ManifestType, "Save_To_Google_Sheet: Provided ManufestType was not found into Crate description of crate control");
            Assert.AreEqual(crateDescriptionLabelToAssert, crateChooser.CrateDescriptions[0].Label, "Save_To_Google_Sheet: CrateDescription label has different value then provided.");

            //select the first crate description
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(saveToGoogleActivity))
            {
                var contrCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var contrls = contrCrate.Content.Controls;
                var crateChoose = contrls.SingleOrDefault(x => x.Type == ControlTypes.CrateChooser && x.Name == "UpstreamCrateChooser") as CrateChooser;
                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateChoose.CrateDescriptions.First().Selected = true;

                //set the name of new spreadheet that need to be created
                var radiobtnGroup = contrls.First(x => x.Type == ControlTypes.RadioButtonGroup) as RadioButtonGroup;
                Assert.NotNull(radiobtnGroup, "Save_To_Google_Sheet: Configuration Controls is missing radio button group for spreadsheet select");
                var radioOption = radiobtnGroup.Radios.First(x => x.Name == "newSpreadsheet");
                radioOption.Controls.First(x => x.Name == "NewSpreadsheetText").Value = newSpeadsheetName;

                crateStorage.Add(contrCrate);
            }

            saveToGoogleActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", saveToGoogleActivity);
            saveToGoogleActivity.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            saveToGoogleActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToGoogleActivity);

            return await Task.FromResult(saveToGoogleActivity);
        }
    }
}
