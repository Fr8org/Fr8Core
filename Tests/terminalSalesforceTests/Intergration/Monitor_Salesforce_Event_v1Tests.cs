using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Fr8.Testing.Integration;
using Fr8.Testing.Integration.Tools.Plans;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    public class Monitor_Salesforce_Event_v1Tests : BaseHubIntegrationTest
    {
        public override string TerminalName => "terminalSalesforce";

        private readonly IntegrationTestTools _plansHelper;

        public Monitor_Salesforce_Event_v1Tests()
        {
            _plansHelper = new IntegrationTestTools(this);
        }

        [Test]
        public async void Monitor_Salesforce_Event_Local_Payload_Processed()
        {
            PlanDTO plan = null;
            try
            {
                var authToken = await Fixtures.HealthMonitor_FixtureData.CreateSalesforceAuthToken();
                plan = await CreateMonitoringPlan(authToken.Id);
            }
            finally
            {
                if (plan != null)
                {
                    await HttpDeleteAsync(GetHubApiBaseUrl() + "/plans?id=" + plan.Plan.Id.ToString());
                }
            }
        }

        private async Task<PlanDTO> CreateMonitoringPlan(Guid authTokenId)
        {
            PlanDTO plan = null;
            try
            {
                plan = await _plansHelper.CreateNewPlan();
                var activityTemplate = await ExtractActivityTemplate();
                await AddActivityToPlan(authTokenId, plan, activityTemplate);


                return plan;
            }
            catch
            {
                if (plan != null)
                {
                    await HttpDeleteAsync(GetHubApiBaseUrl() + "/plans?id=" + plan.Plan.Id.ToString());
                }

                throw;
            }
        }

        private async Task<ActivityTemplateDTO> ExtractActivityTemplate()
        {
            var activityTemplates = await HttpGetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(_baseUrl + "activity_templates/by_categories");
            var monitorActivityTemplate = activityTemplates
                .Where(x => x.Name.ToUpper().Contains("Salesforce".ToUpper()))
                .SelectMany(x => x.Activities)
                .Where(x => x.Name.ToUpper().Contains("Monitor_Salesforce_Event".ToUpper()) && x.Version == "1")
                .FirstOrDefault();

            Assert.NotNull(monitorActivityTemplate, "Unable to find Monitor_Salesforce_Event_v1 ActivityTemplate");

            return monitorActivityTemplate;
        }

        private async Task<ActivityDTO> AddActivityToPlan(
            Guid authTokenId, PlanDTO plan, ActivityTemplateDTO activityTemplate)
        {
            var createActivityUrl = _baseUrl + "activities/create"
                + "?activityTemplateId=" + activityTemplate.Id.ToString()
                + "&createPlan=false"
                + "&parentNodeId=" + plan.Plan.StartingSubPlanId.ToString()
                + "&authorizationTokenId=" + authTokenId.ToString()
                + "&order=1";

            var monitorActivity = await HttpPostAsync<ActivityDTO>(createActivityUrl, null);
            Assert.IsNotNull(monitorActivity, "Initial Create and Configure of Monitor_Salesforce_Event activity is failed.");

            return monitorActivity;
        }

        private async Task<ActivityDTO> ConfigureMonitorActivity(ActivityDTO activity)
        {
            return activity;
        }
    }
}
