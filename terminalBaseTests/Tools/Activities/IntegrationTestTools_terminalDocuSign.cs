using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class IntegrationTestTools_terminalDocuSign
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalDocuSign(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        public async Task<ActivityDTO> AddAndConfigure_QueryDocuSign(PlanDTO plan, int ordering)
        {
            var queryDocuSignActivity = FixtureData.Query_DocuSign_v1_InitialConfiguration();

            var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Receivers };
            var activityTemplates = await _baseHubITest.HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(_baseHubITest.GetHubApiBaseUrl() + "webservices/activities", activityCategoryParam);
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name == "Query_DocuSign");

            queryDocuSignActivity.ActivityTemplate = apmActivityTemplate;

            //connect current activity with a plan
            var subPlan = plan.Plan.SubPlans.FirstOrDefault();
            queryDocuSignActivity.ParentPlanNodeId = subPlan.SubPlanId;
            queryDocuSignActivity.RootPlanNodeId = plan.Plan.Id;
            queryDocuSignActivity.Ordering = ordering;

            //call initial configuration to server
            queryDocuSignActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", queryDocuSignActivity);
            //this call is without authtoken
            queryDocuSignActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", queryDocuSignActivity);

            var initialcrateStorage = _baseHubITest.Crate.FromDto(queryDocuSignActivity.CrateStorage);

            var stAuthCrate = initialcrateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaultDocuSignAuthTokenExists = stAuthCrate == null;

            //if (!defaultDocuSignAuthTokenExists)
            //{
            var terminalDocuSignTools = new Terminals.IntegrationTestTools_terminalDocuSign(_baseHubITest);
            queryDocuSignActivity.AuthToken = await terminalDocuSignTools.GenerateAuthToken("fr8test@gmail.com", "fr8mesomething", queryDocuSignActivity.ActivityTemplate.TerminalId);

            var applyToken = new ManageAuthToken_Apply()
            {
                ActivityId = queryDocuSignActivity.Id,
                AuthTokenId = Guid.Parse(queryDocuSignActivity.AuthToken.Token),
            };
            await _baseHubITest.HttpPostAsync<ManageAuthToken_Apply[], string>( _baseHubITest.GetHubApiBaseUrl() + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken });
            //}

            //send configure with the auth token
            queryDocuSignActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", queryDocuSignActivity);
            queryDocuSignActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", queryDocuSignActivity);

            initialcrateStorage = _baseHubITest.Crate.FromDto(queryDocuSignActivity.CrateStorage);

            Assert.True(initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(),
                "Query_DocuSign: Crate StandardConfigurationControlsCM is missing in API response.");

            var controlsCrate = initialcrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            //set the value of folder to drafts and 
            var controls = controlsCrate.Content.Controls;
            var folderControl = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "Folder");
            Assert.IsNotNull(folderControl, "Query_DocuSign: DropDownList control for Folder value selection was not found");
            folderControl.Value = "Draft";
            folderControl.selectedKey = "Draft";

            //set the value of status to any
            var statusControl = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "Status");
            Assert.IsNotNull(folderControl, "Query_DocuSign: DropDownList control for Status value selection was not found");
            statusControl.Value = null;
            statusControl.selectedKey = null;

            //call followup configuration
            using (var crateStorage = _baseHubITest.Crate.GetUpdatableStorage(queryDocuSignActivity))
            {
                crateStorage.Remove<StandardConfigurationControlsCM>();
                crateStorage.Add(controlsCrate);
            }
            queryDocuSignActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/save", queryDocuSignActivity);
            queryDocuSignActivity = await _baseHubITest.HttpPostAsync<ActivityDTO, ActivityDTO>(_baseHubITest.GetHubApiBaseUrl() + "activities/configure", queryDocuSignActivity);

            return await Task.FromResult(queryDocuSignActivity);
        }
    }
}
