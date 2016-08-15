using System;
using Fr8.Testing.Integration;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;
using terminalSalesforce.Actions;
using Data.Entities;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using System.Configuration;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    [Category("terminalSalesforceTests.Integration")]
    public class GetSalesforceData_Into_SendEmail_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test, Ignore("Vas is working on fixing this test.")]
        public async Task GetSalesforceData_Into_SendEmail_EndToEnd()
        {
            AuthorizationTokenDO authTokenDO = null;
            Guid initialPlanId = Guid.Empty;
            try
            {
                authTokenDO = await Fixtures.HealthMonitor_FixtureData.CreateSalesforceAuthToken();

                //Create the required plan
                initialPlanId = await CreatePlan_GetSalesforceDataIntoSendEmail(authTokenDO);

                //get the full plan which is created
                var plan = await HttpGetAsync<PlanDTO>(_baseUrl + "Plans?include_children=true&id=" + initialPlanId.ToString());
                Debug.WriteLine("Created plan with all activities.");

                //make get salesforce data to get Lead
                var getData = plan.SubPlans.First().Activities.First();
                using (var updatableStorage = Crate.GetUpdatableStorage(getData))
                {
                    //select Lead
                    var configControls = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().Single();
                (configControls.Content.Controls.Single(c => c.Name.Equals(nameof(Get_Data_v1.ActivityUi.SalesforceObjectSelector))) as DropDownList).selectedKey = "Lead";

                    //give condition
                    var conditionQuery = new List<FilterConditionDTO>() { new FilterConditionDTO { Field = "LastName", Operator = "eq", Value = "McSorely" } };
                (configControls.Content.Controls.Single(c => c.Name.Equals(nameof(Get_Data_v1.ActivityUi.SalesforceObjectFilter))) as QueryBuilder).Value = JsonConvert.SerializeObject(conditionQuery);
                }
                getData = await ConfigureActivity(getData);
                Debug.WriteLine("Get Lead using condition is successful in the Follow Up Configure");

                //prepare the send email activity controls.
                var sendEmail = plan.SubPlans.First().Activities.Last();
                using (var updatableStorage = Crate.GetUpdatableStorage(sendEmail))
                {
                    var configControls = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().Single();

                    var emailAddressControl = (TextSource)configControls.Content.Controls.Single(c => c.Name.Equals("EmailAddress"));
                    var emailSubjectControl = (TextSource)configControls.Content.Controls.Single(c => c.Name.Equals("EmailSubject"));
                    var emailBodyControl = (TextSource)configControls.Content.Controls.Single(c => c.Name.Equals("EmailBody"));

                    emailAddressControl.ValueSource = "specific";
                    emailAddressControl.TextValue = ConfigurationManager.AppSettings["TestEmailYahoo"];

                    emailSubjectControl.ValueSource = emailBodyControl.ValueSource = "upstream";
                    emailSubjectControl.selectedKey = "Name";
                    emailBodyControl.selectedKey = "Phone";
                }
                sendEmail = await ConfigureActivity(sendEmail);
                Debug.WriteLine("Send Email follow up configure is successful.");

                //Run the plan
                await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + plan.Id, null);
                Debug.WriteLine("Plan execution is successful.");

                await CleanUp(authTokenDO, initialPlanId);

                //Verify the email
                EmailAssert.EmailReceived(ConfigurationManager.AppSettings["OpsEmail"], "Marty McSorely", true);
            }
            finally
            {
                await CleanUp(authTokenDO, initialPlanId);
            }
        }

        private async Task<Guid> CreatePlan_GetSalesforceDataIntoSendEmail(AuthorizationTokenDO authToken)
        {
            //get required activity templates
            var activityTemplates = await HttpGetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(_baseUrl + "activity_templates");
            var getData = activityTemplates.Single(at => at.Name.Equals("Receivers")).Activities.Single(a => a.Name.Equals("Get_Data"));
            var sendEmail = activityTemplates.Single(at => at.Name.Equals("Forwarders")).Activities.Single(a => a.Name.Equals("Send_Email_Via_SendGrid"));
            Assert.IsNotNull(getData, "Get Salesforce Data activity is not available");
            Assert.IsNotNull(sendEmail, "Send Email activity is not available");
            Debug.WriteLine("Got required activity templates.");

            //create initial plan
            var initialPlan = await HttpPostAsync<PlanNoChildrenDTO, PlanDTO>(_baseUrl + "plans", new PlanNoChildrenDTO()
            {
                Name = "GetSalesforceData_Into_SendEmail_EndToEnd_Test"
            });
            Debug.WriteLine("Created initial plan without actions");

            string mainUrl = _baseUrl + "activities/create";
            var postUrl = "?activityTemplateId={0}&createPlan=false";
            var formattedPostUrl = string.Format(postUrl, getData.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.StartingSubPlanId;
            formattedPostUrl += "&authorizationTokenId=" + authToken.Id.ToString();
            formattedPostUrl += "&order=" + 1;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var getDataActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
            Assert.IsNotNull(getDataActivity, "Initial Create and Configure of Get Salesforce Data action is failed.");
            Debug.WriteLine("Create and Initial Configure of Get Salesforce Data activity is successful.");

            formattedPostUrl = string.Format(postUrl, sendEmail.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.StartingSubPlanId;
            formattedPostUrl += "&order=" + 2;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var sendEmailActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
            Assert.IsNotNull(sendEmailActivity, "Initial Create and Configure of Send Email action is failed.");
            Debug.WriteLine("Create and Initial Configure of Send Email activity is successful.");

            return initialPlan.Id;
        }

        private async Task CleanUp(AuthorizationTokenDO authTokenDO, Guid initialPlanId)
        {
            if (initialPlanId != Guid.Empty)
            {
               // await HttpDeleteAsync(_baseUrl + "Plans/Delete?id=" + initialPlanId.ToString());
            }

            if (authTokenDO != null)
            {
                await HttpPostAsync<string>(_baseUrl + "authentication/tokens/revoke?id=" + authTokenDO.Id.ToString(), null);
            }
        }
    }
}
