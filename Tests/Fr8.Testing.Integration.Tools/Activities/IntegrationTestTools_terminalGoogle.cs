using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Fr8.Testing.Integration;
using NUnit.Framework;
using terminalGoogle.Actions;
using terminalGoogle.Activities;
using Fr8.Testing.Unit.Fixtures;

namespace Fr8.Testing.Integration.Tools.Activities
{
    public class IntegrationTestTools_terminalGoogle
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalGoogle(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        /// <summary>
        /// Add new Save_To_Google_Sheet activity to an existing plan and configure that activity with selected spreadsheet.
        /// Add support for upstream cratechooser to expect provided manifestType and crateDescriptionLabel
        /// </summary>
        /// <param name="plan">Existing plan that will be associated with new Save_To_Google_Sheet activity</param>
        /// <param name="ordering">Ordering of Save_To_Google_Sheet activity into plan </param>
        /// <param name="manifestTypeToUse">ManifestType that will be used inside UpstreamCrateChooser control</param>
        /// <param name="crateDescriptionLabelToUse">CrateDescriptionLabel that will be used inside UpstreamCrateChooser control</param>
        /// <param name="newSpeadsheetName">name of the spreadsheet that need to be created on Run Save_To_Google_Sheet activity</param>
        /// <returns></returns>
        public async Task<ActivityDTO> AddAndConfigureSaveToGoogleSheet(PlanDTO plan,
                                                                        int ordering, string manifestTypeToUse,
                                                                        string crateDescriptionLabelToUse,
                                                                        string newSpeadsheetName)
        {
            var activityName = "Save_To_Google_Sheet";

            var saveToGoogleSheetActivityDTO = await AddGoogleActivityToPlan(FixtureData.Save_To_Google_Sheet_v1_InitialConfiguration(), plan, ordering, ActivityCategories.ForwardId, activityName);
            //Activity won't be able to run if there is no upstream data

            var upstreamCrateDescriptions = await _baseHubITest.GetRuntimeCrateDescriptionsFromUpstreamActivities(saveToGoogleSheetActivityDTO.Id);
            Assert.Greater(upstreamCrateDescriptions.AvailableCrates.Count, 0, $"{activityName}: upstream activities didn't provide at least one runtime CrateDescription");

            var expectedCrateDescription = upstreamCrateDescriptions.AvailableCrates.FirstOrDefault(x => x.ManifestType == manifestTypeToUse && x.Label == crateDescriptionLabelToUse);
            Assert.IsNotNull(expectedCrateDescription, $"{activityName}: upstream activities didn't provide expected runtime CrateDescription");

            //Select the expected crate description
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(saveToGoogleSheetActivityDTO))
            {
                var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var activityUi = new Save_To_Google_Sheet_v1.ActivityUi();
                activityUi.SyncWith(controlsCrate.Content);
                crateStorage.Remove<StandardConfigurationControlsCM>();
                activityUi.UpstreamCrateChooser.CrateDescriptions = upstreamCrateDescriptions.AvailableCrates;
                activityUi.UpstreamCrateChooser.CrateDescriptions.First(x => x.Label == crateDescriptionLabelToUse && x.ManifestType == manifestTypeToUse).Selected = true;
                //Set the name of new spreadheet that need to be created
                activityUi.NewSpreadsheetName.Value = newSpeadsheetName;
                crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray())));
            }

            saveToGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", saveToGoogleSheetActivityDTO);
            saveToGoogleSheetActivityDTO.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            saveToGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", saveToGoogleSheetActivityDTO);

            return saveToGoogleSheetActivityDTO;
        }

        public async Task<ActivityDTO> CreateMonitorGmailInbox(PlanDTO plan, int ordering)
        {
            return await AddGoogleActivityToPlan(FixtureData.Monitor_Gmail_Inbox_v1_InitialConfiguration(), plan, ordering, ActivityCategories.MonitorId, "Monitor_Gmail_Inbox", false);
        }

        public async Task<ActivityDTO> SaveActivity(ActivityDTO activity)
        {
            return await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", activity);
        }

        public async Task<ActivityDTO> ConfigureActivity(ActivityDTO activity)
        {
            activity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", activity);
            return activity;
        }

        /// <summary>
        /// Add new Get_Google_Sheet_Data activity to an existing plan and configure that activity with selected spreadsheet
        /// </summary>
        /// <param name="plan">Existing plan that will be associated with new Get_Google_Sheet_Data activity</param>
        /// <param name="ordering">Ordering of Get_Google_Sheet_Data activity into plan </param>
        /// <param name="spreadsheetName">name of the spreadsheet use in configure procees</param>
        /// <param name="includeFixtureAuthToken">Use fixture data as google auth token</param>
        /// <returns>Configured ActivityDTO for Get_Google_Sheet_Data</returns>
        public async Task<ActivityDTO> AddAndConfigureGetFromGoogleSheet(PlanDTO plan, int ordering, string spreadsheetName, bool includeFixtureAuthToken)
        {
            var activityName = "Get_Google_Sheet_Data";

            var getFromGoogleSheetActivityDTO = await AddGoogleActivityToPlan(FixtureData.Get_Google_Sheet_Data_v1_InitialConfiguration(), plan, ordering, ActivityCategories.ReceiveId, activityName);

            return await ConfigureGetFromGoogleSheetActivity(getFromGoogleSheetActivityDTO, spreadsheetName, includeFixtureAuthToken);
        }

        /// <summary>
        /// Configuration for Get_Google_Sheet_Data activity. Select spreadsheet value based on spreadsheetName inside SpeadSheet DropDownLIst
        /// </summary>
        /// <param name="getFromGoogleSheetActivityDTO"></param>
        /// <param name="spreadsheetName"></param>
        /// <param name="includeFixtureAuthToken"></param>
        /// <param name="worksheetName"></param>
        /// <returns>Get_Google_Sheet_Data activityDTO</returns>
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
                //set spreadsheet name as selected inside SpreadSheetList dropdown
                activityUi.SpreadsheetList.selectedKey = spreadsheetName;
                activityUi.SpreadsheetList.Value = spreadsheetUri;

                crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray())));
            }

            if (!string.IsNullOrEmpty(worksheetName))
            {
                //in case we don't want to use default worksheet, provide a worksheetName to be set into the activity 
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

                    crateStorage.Add(Crate<StandardConfigurationControlsCM>.FromContent(controlsCrate.Label, new StandardConfigurationControlsCM(activityUi.Controls.ToArray())));
                }
            }

            getFromGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", getFromGoogleSheetActivityDTO);

            //add a possibility to use FixtureData for newly created activitis, or to remain the usage of already associated default token from google 
            if (includeFixtureAuthToken)
            {
                getFromGoogleSheetActivityDTO.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            }
            getFromGoogleSheetActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", getFromGoogleSheetActivityDTO);
            return getFromGoogleSheetActivityDTO;
        }

        /// <summary>
        /// Add new Google Activity to a existing plan. Create the activity based on a activity template, set activity ordering and call initial configuration
        /// with associated Google Auth Token
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="ordering"></param>
        /// <param name="activityCategory"></param>
        /// <param name="activityName"></param>
        /// <returns></returns>
        private async Task<ActivityDTO> AddGoogleActivityToPlan(ActivityDTO activity, PlanDTO plan, int ordering, Guid activityCategory, string activityName, bool checkAuthentication = true)
        {
            var googleActivityDTO = activity;
            var activityCategoryParam = activityCategory.ToString();
            var activityTemplates = await _baseHubITest
                .HttpGetAsync<List<WebServiceActivitySetDTO>>(_baseHubITest.GetHubApiBaseUrl() + "webservices?id=" + activityCategoryParam);
            var apmActivityTemplate = activityTemplates
                .SelectMany(a => a.Activities)
                .Select(x => new ActivityTemplateSummaryDTO
                {
                    Name = x.Name,
                    Version = x.Version,
                    TerminalName = x.Terminal.Name,
                    TerminalVersion = x.Terminal.Version
                })
                .Single(a => a.Name == activityName);
            googleActivityDTO.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.SubPlans.FirstOrDefault();
            googleActivityDTO.ParentPlanNodeId = subPlan.SubPlanId;
            googleActivityDTO.RootPlanNodeId = plan.Id;
            googleActivityDTO.Ordering = ordering;

            //call initial configuration to server
            googleActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", googleActivityDTO);
            googleActivityDTO.AuthToken = FixtureData.GetGoogleAuthorizationToken();
            googleActivityDTO = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", googleActivityDTO);
            var initialcrateStorage = _baseHubITest.Crate.FromDto(googleActivityDTO.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaulGoogleAuthTokenExists = stAuthCrate == null;
            if (checkAuthentication)
            {
                Assert.AreEqual(true, defaulGoogleAuthTokenExists, $"{activityName}: GoogleService require authentication. They might be a problem with default authentication tokens and KeyVault authorization mode");
                initialcrateStorage = _baseHubITest.Crate.FromDto(googleActivityDTO.CrateStorage);
                Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), $"{activityName}: Crate StandardConfigurationControlsCM is missing in API response.");
            }
            return googleActivityDTO;
        }
    }
}
