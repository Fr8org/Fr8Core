using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Integration;
using Fr8.Testing.Integration.Tools.Plans;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Managers;
using System.Configuration;

namespace terminalSalesforceTests.Intergration
{
    [Explicit]
    public class Monitor_Salesforce_Event_v1Tests : BaseHubIntegrationTest
    {
        private const long MaxAwaitTime = 180000;
        private const int PeriodAwaitTime = 10000;

        private string SalesforcePayload
        {
            get
            {
                return @"<?xml version=""1.0"" encoding=""UTF-8""?>
                         <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	                         <soapenv:Body>
		                         <notifications xmlns = ""http://soap.sforce.com/2005/09/outbound"">
                                     <OrganizationId> 00Di0000000ilESEAY</OrganizationId>
			                         <ActionId>04ki0000000PEkxAAG</ActionId>
			                         <SessionId>00Di0000000ilES!AQcAQFp66.3msePbVxrvLDhSnV5vQNjsif7SUbseyjj5zD0RDr5NJFX8.8rmu88O02z5CK636aBa81Okx1fS4JVFL8jAhnJr</SessionId>
			                         <EnterpriseUrl>https://na15.salesforce.com/services/Soap/c/37.0/00Di0000000ilES</EnterpriseUrl>
			                         <PartnerUrl>https://na15.salesforce.com/services/Soap/u/37.0/00Di0000000ilES</PartnerUrl>
			                         <Notification>
			             	            <Id>04li000000AzmHSAAZ</Id>
			             	            <sObject xsi:type=""sf:Lead"" xmlns:sf=""urn:sobject.enterprise.soap.sforce.com"">
			             		            <sf:Id>003i000000W8jx3AAB</sf:Id>
			             		            <sf:Email>salesforce-payload @fr8.com</sf:Email>
			             		            <sf:LastName>Fr8-Test-User</sf:LastName>
                                             <sf:OwnerId>" + ConfigurationManager.AppSettings["OwnerId"] + @"</sf:OwnerId>
			             	            </sObject>
			                         </Notification>
		                         </notifications>
	                         </soapenv:Body>
                         </soapenv:Envelope>";
            }
        }

        public override string TerminalName => "terminalSalesforce";

        private readonly IntegrationTestTools _plansHelper;

        public Monitor_Salesforce_Event_v1Tests()
        {
            _plansHelper = new IntegrationTestTools(this);
        }

        [Test]
        public async void Monitor_Salesforce_Event_Local_Payload_Processed()
        {
            var objectTypes = new[]
            {
                "Account",
                "Case",
                "Contact",
                "Contract",
                "Document",
                "Lead",
                "Opportunity",
                "Product2"
            };

            foreach (var objectType in objectTypes)
            {
                await RunTest(objectType);
            }
        }

        private async Task RunTest(string objectType)
        {
            Debug.WriteLine("Testing monitoring for ObjectType = " + objectType);

            PlanDTO plan = null;
            try
            {
                var authToken = await Fixtures.HealthMonitor_FixtureData.CreateSalesforceAuthToken();
                plan = await CreateMonitoringPlan(authToken.Id);
                await _plansHelper.RunPlan(plan.Id);

                await HttpPostAsync<string>(GetTerminalEventsUrl(), new StringContent(string.Format(SalesforcePayload, objectType)));

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (stopwatch.ElapsedMilliseconds < MaxAwaitTime)
                {
                    await Task.Delay(PeriodAwaitTime);
                    Debug.WriteLine("Awaiting for monitor container to run: " + stopwatch.ElapsedMilliseconds.ToString() + " msec");

                    if (IsContainerAvailable(plan.Id))
                    {
                        Debug.WriteLine("Container successfully executed");
                        return;
                    }
                }

                Assert.Fail("No container found for specified plan.");
            }
            finally
            {
                if (plan != null)
                {
                    await HttpDeleteAsync(GetHubApiBaseUrl() + "/plans?id=" + plan.Id.ToString());
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
                var activity = await AddActivityToPlan(authTokenId, plan, activityTemplate);
                await ConfigureMonitorActivity(activity);

                return plan;
            }
            catch
            {
                if (plan != null)
                {
                    await HttpDeleteAsync(GetHubApiBaseUrl() + "/plans?id=" + plan.Id.ToString());
                }

                throw;
            }
        }

        private async Task<ActivityTemplateDTO> ExtractActivityTemplate()
        {
            var activityTemplates = await HttpGetAsync<IEnumerable<ActivityTemplateCategoryDTO>>(_baseUrl + "activity_templates");
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
                + "&parentNodeId=" + plan.StartingSubPlanId.ToString()
                + "&authorizationTokenId=" + authTokenId.ToString()
                + "&order=1";

            var monitorActivity = await HttpPostAsync<ActivityDTO>(createActivityUrl, null);
            Assert.IsNotNull(monitorActivity, "Initial Create and Configure of Monitor_Salesforce_Event activity is failed.");

            return monitorActivity;
        }

        private async Task<ActivityDTO> ConfigureMonitorActivity(ActivityDTO activity)
        {
            using (var updater = Crate.GetUpdatableStorage(activity))
            {
                var cm = updater.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();
                Assert.NotNull(cm, "Unable to find StandardConfigurationControlsCM crate");

                // Selected ObjectType.
                var ddl = cm.FindByName<DropDownList>("SalesforceObjectList");
                Assert.NotNull(ddl, "Unable to find SalesforceObjectList DropDownList in ConfigurationControls");

                var leadListItem = ddl.ListItems.FirstOrDefault(x => x.Key == "Lead");
                Assert.NotNull(leadListItem, "Unable to find Lead list item in SalesforceObjectList items");

                ddl.SelectByValue(leadListItem.Value);

                // Check Created check-box.
                var createdCheckBox = cm.FindByName<CheckBox>("Created");
                Assert.NotNull(createdCheckBox, "Unable to find Created CheckBox in ConfigurationControls");

                createdCheckBox.Selected = true;

                // Check Updated check-box.
                var updatedCheckBox = cm.FindByName<CheckBox>("Updated");
                Assert.NotNull(updatedCheckBox, "Unable to find Updated CheckBox in ConfigurationControls");

                updatedCheckBox.Selected = true;
            }

            activity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", activity);
            activity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + activity.Id.ToString(), activity);

            return activity;
        }

        private bool IsContainerAvailable(Guid planId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var container = uow.ContainerRepository
                    .GetQuery()
                    .Where(x => x.PlanId == planId)
                    .FirstOrDefault();

                if (container != null)
                {
                    var crateStorage = Crate.GetStorage(container.CrateStorage);
                    var monitorActivityResult = crateStorage
                        .CrateContentsOfType<StandardTableDataCM>(x => x.Label == "Lead Created/Updated on Salesforce.com")
                        .FirstOrDefault();

                    if (monitorActivityResult != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
