using Data.Entities;
using HealthMonitor.Utility;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using terminalSalesforce.Actions;
using terminalSalesforce.Infrastructure;
using terminalSalesforce.Services;
using Fr8Data.Managers;
using TerminalBase.Models;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    [Category("terminalSalesforceTests.Integration")]
    public class SaveToSalesforce_And_GetSalesforceData_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalSalesforce"; }
        }

        [Test]
        public async Task SaveToSalesforce_And_GetSalesforceData_EndToEnd()
        {
            AuthorizationTokenDO authTokenDO = null;
            AuthorizationToken authorizationToken = null;
            Guid initialPlanId = Guid.Empty;
            string newLeadId = string.Empty;
            try
            {
                authTokenDO = await Fixtures.HealthMonitor_FixtureData.CreateSalesforceAuthToken();
                authorizationToken = new AuthorizationToken
                {
                    Token = authTokenDO.Token,
                    Id = authTokenDO.Id.ToString()
                };
                //Create the required plan with all initial activities initial config
                initialPlanId = await CreatePlan_SaveAndGetDataFromSalesforce(authTokenDO);

                //get the full plan which is created
                var plan = await HttpGetAsync<PlanDTO>(_baseUrl + "Plans/full?id=" + initialPlanId.ToString());
                Debug.WriteLine("Created plan with all activities.");

                //prepare the two activities with their follow up config
                await PrepareSaveToLead(plan);
                await PrepareGetData(plan);

                //execute the plan
                Debug.WriteLine("Executing plan " + plan.Plan.Name);
                var container = await HttpPostAsync<string, ContainerDTO>(_baseUrl + "plans/run?planId=" + plan.Plan.Id.ToString(), null);
                Debug.WriteLine("Executing plan " + plan.Plan.Name + " successful.");

                //get the payload of the executed plan
                var payload = await HttpGetAsync<PayloadDTO>(_baseUrl + "Containers/payload?id=" + container.Id.ToString());
                Debug.WriteLine("Got the payload for the container " + container.Name);

                //Assert
                Debug.WriteLine("Asserting initial payload.");
                var payloadList = Crate.GetUpdatableStorage(payload).CratesOfType<StandardPayloadDataCM>().ToList();
                Assert.AreEqual(2, payloadList.Count, "The payload does not contian all activities payload");
                Assert.IsTrue(payloadList.Any(pl => pl.Label.Equals("Lead is saved in Salesforce.com")), "Save Data is Failed to save the lead.");

                Debug.WriteLine("Asserting Save To Salesforce payload.");
                var savePayload = payloadList[0].Content.PayloadObjects[0].PayloadObject;
                Assert.AreEqual(1, savePayload.Count, "Save data payload contains some unwanted fields.");
                Assert.IsTrue(savePayload.Any(f => f.Key.Equals("LeadID")), "The newly created lead ID is not populated in the run time.");

                newLeadId = savePayload.Single(f => f.Key.Equals("LeadID")).Value;
                Debug.WriteLine("Newly created Lead ID is " + newLeadId);

                Debug.WriteLine("Deleting newly created lead with " + newLeadId);
                var isDeleted = await new SalesforceManager().Delete(SalesforceObjectType.Lead, newLeadId, authorizationToken);
                Assert.IsTrue(isDeleted, "The newly created Lead for integration test purpose is not deleted.");
                newLeadId = string.Empty;

                Debug.WriteLine("Cleaning up.");
                await CleanUp(authorizationToken, initialPlanId, newLeadId);
                Debug.WriteLine("Cleaning up Successful.");
            }
            finally
            {
                await CleanUp(authorizationToken, initialPlanId, newLeadId);
            }
        }

        private async Task<Guid> CreatePlan_SaveAndGetDataFromSalesforce(AuthorizationTokenDO authToken)
        {
            //get required activity templates
            var activityTemplates = await HttpGetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(_baseUrl + "plannodes/available");
            var atSave = activityTemplates.Single(at => at.Name.Equals("Forwarders")).Activities.Single(a => a.Name.Equals("Save_To_SalesforceDotCom"));
            var atGet = activityTemplates.Single(at => at.Name.Equals("Receivers")).Activities.Single(a => a.Name.Equals("Get_Data"));
            Assert.IsNotNull(atSave, "Save to Salesforce.com activity is not available");
            Assert.IsNotNull(atGet, "Get Salesforce Data activity is not available");
            Debug.WriteLine("Got required activity templates.");

            //create initial plan
            var initialPlan = await HttpPostAsync<PlanEmptyDTO, PlanDTO>(_baseUrl + "plans", new PlanEmptyDTO()
            {
                Name = "SaveToAndGetFromSalesforce"
            });
            Debug.WriteLine("Created initial plan without actions");

            string mainUrl = _baseUrl + "activities/create";
            var postUrl = "?activityTemplateId={0}&createPlan=false";
            var formattedPostUrl = string.Format(postUrl, atSave.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.Plan.StartingSubPlanId;
            formattedPostUrl += "&authorizationTokenId=" + authToken.Id.ToString();
            formattedPostUrl += "&order=" + 1;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var saveToSfActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
            Assert.IsNotNull(saveToSfActivity, "Initial Create and Configure of Save to Salesforce activity is failed.");
            Debug.WriteLine("Create and Initial Configure of Save to Salesforce activity is successful.");

            mainUrl = _baseUrl + "activities/create";
            postUrl = "?activityTemplateId={0}&createPlan=false";
            formattedPostUrl = string.Format(postUrl, atGet.Id);
            formattedPostUrl += "&parentNodeId=" + initialPlan.Plan.StartingSubPlanId;
            formattedPostUrl += "&authorizationTokenId=" + authToken.Id.ToString();
            formattedPostUrl += "&order=" + 2;
            formattedPostUrl = mainUrl + formattedPostUrl;
            var getDataActivity = await HttpPostAsync<ActivityDTO>(formattedPostUrl, null);
            Assert.IsNotNull(getDataActivity, "Initial Create and Configure of Get Data activity is failed.");
            Debug.WriteLine("Create and Initial Configure of Get Data activity is successful.");

            return initialPlan.Plan.Id;
        }

        private async Task CleanUp(AuthorizationToken authToken, Guid initialPlanId, string newLeadId)
        {
            if(!string.IsNullOrEmpty(newLeadId))
            {
                var isDeleted = await new SalesforceManager().Delete(SalesforceObjectType.Lead, newLeadId, authToken);
                Assert.IsTrue(isDeleted, "The newly created Lead for integration test purpose is not deleted.");
            }

            if (initialPlanId != Guid.Empty)
            {
                await HttpDeleteAsync(_baseUrl + "Plans/Delete?id=" + initialPlanId);
            }

            if (authToken != null)
            {
                await HttpPostAsync<string>(_baseUrl + "manageauthtoken/revoke?id=" + authToken.Id, null);
            }
        }

        private async Task PrepareSaveToLead(PlanDTO plan)
        {
            var saveActivity = plan.Plan.SubPlans.First().Activities.First();

            //set lead and do the follow up config
            using (var updatableStorage = Crate.GetUpdatableStorage(saveActivity))
            {
                var ddlb = (DropDownList)updatableStorage.CratesOfType<StandardConfigurationControlsCM>().Single().Content.Controls[0];
                ddlb.selectedKey = "Lead";
            }
            saveActivity = await ConfigureActivity(saveActivity);
            Debug.WriteLine("Save to Salesforce Follow up config is successfull with Lead selected.");

            //set the lead required fields.
            using (var updatableStorage = Crate.GetUpdatableStorage(saveActivity))
            {
                var requiredControls = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().Single().Content.Controls;

                var lastNameControl = (TextSource)requiredControls.Single(c => c.Name.Equals("LastName"));
                lastNameControl.ValueSource = "specific";
                lastNameControl.TextValue = "Unit";

                var companyControl = (TextSource)requiredControls.Single(c => c.Name.Equals("Company"));
                companyControl.ValueSource = "specific";
                companyControl.TextValue = "Test";
            }
            saveActivity = await ConfigureActivity(saveActivity);
            Debug.WriteLine("Save to Salesforce Follow up config is successfull with required fields set.");

            plan.Plan.SubPlans.First().Activities[0] = saveActivity;
        }

        private async Task PrepareGetData(PlanDTO plan)
        {
            var getDataActivity = plan.Plan.SubPlans.First().Activities.Last();
            //set lead and do the follow up config
            var crateStorage = Crate.GetStorage(getDataActivity);
            crateStorage.UpdateControls<Get_Data_v1.ActivityUi>(x =>
            {
                x.SalesforceObjectSelector.selectedKey = SalesforceObjectType.Lead.ToString();
            });
            getDataActivity = await ConfigureActivity(getDataActivity);
            Debug.WriteLine("Get Data Follow up config is successfull with Lead selected");
            //set the lead required fields.
            crateStorage.UpdateControls<Get_Data_v1.ActivityUi>(x =>
            {
                x.SalesforceObjectFilter.Value = JsonConvert.SerializeObject(new List <FilterConditionDTO> { new FilterConditionDTO { Field = "LastName", Operator = "eq", Value = "Unit" } });
            });
            getDataActivity = await ConfigureActivity(getDataActivity);
            Debug.WriteLine("Get Data Follow up config is successfull with selection query fields set.");
            plan.Plan.SubPlans.First().Activities[1] = getDataActivity;
        }
    }
}
