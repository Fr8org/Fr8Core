using System;
using HealthMonitor.Utility;
using NUnit.Framework;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using System.Web.Http;
using StructureMap;
using Data.Interfaces;
using System.Linq;
using Data.Entities;
using System.Collections.Generic;
using Hub.Managers;
using Data.Interfaces.Manifests;
using Data.Crates;
using Data.Control;
using Newtonsoft.Json;
using System.Diagnostics;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class GetSalesforceData_Into_SendEmail_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test, Ignore("Ignored for now. Should be fixed as a part of FR-2851. Vas is working on it")]
        public async Task GetSalesforceData_Into_SendEmail_EndToEnd()
        {
            //Create the required plan
            var planId = await CreatePlan_GetSalesforceDataIntoSendEmail();

            //get the full plan which is created
            var plan = await HttpGetAsync<PlanDTO>(_baseUrl + "Plans/full?id=" + planId.ToString());
            Debug.WriteLine("Created plan with all activities.");

            //make get salesforce data to get Lead
            var getData = plan.Plan.SubPlans.First().Activities.First();
            using (var updatableStorage = Crate.GetUpdatableStorage(getData))
            {
                //select Lead
                var configControls = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
                (configControls.Content.Controls.Single(c => c.Name.Equals("WhatKindOfData")) as DropDownList).selectedKey = "Lead";

                //give condition
                var conditionQuery = new List<FilterConditionDTO>() { new FilterConditionDTO { Field = "Name", Operator = "eq", Value = "Marty McSorely" } };
                (configControls.Content.Controls.Single(c => c.Name.Equals("SelectedQuery")) as QueryBuilder).Value = JsonConvert.SerializeObject(conditionQuery);
            }

            var authToken = (await HttpGetAsync<List<ManageAuthToken_Terminal>>(_baseUrl + "ManageAuthToken")).Single(a => a.Name.Equals("terminalSalesforce"));
            getData.AuthToken = new AuthorizationTokenDTO { Id = authToken.AuthTokens[0].Id.ToString() };
            getData = await ConfigureActivity(getData);
            Assert.IsTrue(getData.CrateStorage.Crates.Any(c => c.Label.Equals("Salesforce Object Fields")), 
                          "Follow up configuration is not getting any Salesforce Object Fields");
            Debug.WriteLine("Get Lead using condition is successful in the Follow Up Configure");

            //prepare the send email activity controls.
            var sendEmail = plan.Plan.SubPlans.First().Activities.Last();
            using (var updatableStorage = Crate.GetUpdatableStorage(sendEmail))
            {
                var configControls = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().Single();

                var emailAddressControl = (TextSource)configControls.Content.Controls.Single(c => c.Name.Equals("EmailAddress"));
                var emailSubjectControl = (TextSource)configControls.Content.Controls.Single(c => c.Name.Equals("EmailSubject"));
                var emailBodyControl = (TextSource)configControls.Content.Controls.Single(c => c.Name.Equals("EmailBody"));

                emailAddressControl.ValueSource = "specific";
                emailAddressControl.TextValue = "fr8.testing@yahoo.com";

                emailSubjectControl.ValueSource = emailBodyControl.ValueSource = "upstream";
                emailSubjectControl.selectedKey = "Name";
                emailBodyControl.selectedKey = "Phone";
            }
            sendEmail = await ConfigureActivity(sendEmail);
            Debug.WriteLine("Send Email follow up configure is successful.");

            //Run the plan
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + plan.Plan.Id, null);
            Debug.WriteLine("Plan execution is successful.");

            //Verify the email fr8.testing@yahoo.com
            EmailAssert.EmailReceived("fr8ops@fr8.company", "Marty McSorely", true);
        }

        private async Task<Guid> CreatePlan_GetSalesforceDataIntoSendEmail()
        {
            //get required activity templates
            var activityTemplates = await HttpGetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(_baseUrl + "plannodes/available");
            var getData = activityTemplates.Single(at => at.Name.Equals("Receivers")).Activities.Single(a => a.Name.Equals("Get_Data"));
            var sendEmail = activityTemplates.Single(at => at.Name.Equals("Forwarders")).Activities.Single(a => a.Name.Equals("SendEmailViaSendGrid"));
            Assert.IsNotNull(getData, "Get Salesforce Data activity is not available");
            Assert.IsNotNull(sendEmail, "Send Email activity is not available");
            Debug.WriteLine("Got required activity templates.");

            //get salesforce auth token
            var authTokens = await HttpGetAsync<List<ManageAuthToken_Terminal>>(_baseUrl + "ManageAuthToken");

            if (!authTokens.Any(at => at.Name.Equals("terminalSalesforce")))
            {
                var failureMessage = "Authorization token for Salesforce is not found for the integration testing user <integration_test_runner@fr8.company>." +
                                        "Please go to the target instance of fr8 and log in with the integration testing user credentials. " +
                                        "Then add a salesforce action to any plan and be sure to set the 'Use for all Activities' checkbox on the " +
                                        "Authorize Accounts dialog while authenticating.";
                Assert.Fail(failureMessage);
            }

            var salesforceAuthToken = authTokens.Single(a => a.Name.Equals("terminalSalesforce"));

            //create initial plan
            var initialPlan = await HttpPostAsync<PlanEmptyDTO, PlanDTO>(_baseUrl + "plans", new PlanEmptyDTO()
            {
                Name = "GetSalesforceData_Into_SendEmail_EndToEnd_Test"
            });
            Debug.WriteLine("Created initial plan without actions");

            string mainUrl = _baseUrl + "activities/create";
            var postUrl = "?actionTemplateId={0}&createPlan=false";
            var formattedPostUrl = string.Format(postUrl, getData.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.Plan.StartingSubPlanId;
            formattedPostUrl += "&authorizationTokenId=" + salesforceAuthToken.AuthTokens[0].Id;
            formattedPostUrl += "&order=" + 1;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var getDataActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
            Assert.IsNotNull(getDataActivity, "Initial Create and Configure of Get Salesforce Data action is failed.");
            Debug.WriteLine("Create and Initial Configure of Get Salesforce Data activity is successful.");

            formattedPostUrl = string.Format(postUrl, sendEmail.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.Plan.StartingSubPlanId;
            formattedPostUrl += "&order=" + 2;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var sendEmailActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
            Assert.IsNotNull(sendEmailActivity, "Initial Create and Configure of Send Email action is failed.");
            Debug.WriteLine("Create and Initial Configure of Send Email activity is successful.");

            return initialPlan.Plan.Id;
        }
    }
}
