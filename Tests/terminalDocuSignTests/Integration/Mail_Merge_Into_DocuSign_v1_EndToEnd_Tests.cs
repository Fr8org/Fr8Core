using System;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Hub.Managers;
using Data.Interfaces.Manifests;
using terminalDocuSignTests.Fixtures;
using Newtonsoft.Json.Linq;
using Data.Crates;
using Data.Control;
using Data.States;

namespace terminalDocuSignTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Mail_Merge_Into_DocuSign_v1_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        ActivityDTO solution;
        CrateStorage crateStorage;

        [Test]
        public void Mail_Merge_Into_DocuSign_EndToEnd()
        {
            string baseUrl = GetHubApiBaseUrl();
            var solutionCreateUrl = baseUrl + "actions/create?solutionName=Mail_Merge_Into_DocuSign";

            //
            // Create solution
            //
            var plan = HttpPostAsync<string, RouteFullDTO>(solutionCreateUrl, null).Result;
            var solution = plan.Subroutes.FirstOrDefault().Activities.FirstOrDefault();

            //
            // Send configuration request without authentication token
            //
            this.solution = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure?id=" + solution.Id, solution).Result;
            crateStorage = _crate.FromDto(this.solution.CrateStorage);
            var stAuthCrate = crateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaultDocuSignAuthTokenExists = stAuthCrate == null;

            if (!defaultDocuSignAuthTokenExists)
            {
                //
                // Authenticate with DocuSign
                //
                var creds = new CredentialsDTO()
                {
                    Username = "freight.testing@gmail.com",
                    Password = "I6HmXEbCxN",
                    IsDemoAccount = true,
                    TerminalId = solution.ActivityTemplate.TerminalId
                };
                var token = HttpPostAsync<CredentialsDTO, JObject>(baseUrl + "authentication/token", creds).Result;
                Assert.AreEqual(false, String.IsNullOrEmpty(token["authTokenId"].Value<string>()), "AuthTokenId is missing in API response.");
                Guid tokenGuid = Guid.Parse(token["authTokenId"].Value<string>());

                //
                // Asociate token with action
                //
                var applyToken = new ManageAuthToken_Apply()
                {
                    ActivityId = solution.Id,
                    AuthTokenId = tokenGuid,
                    IsMain = true
                };
                HttpPostAsync<ManageAuthToken_Apply[], string>(baseUrl + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken }).Wait();
            }

            //
            // Send configuration request with authentication token
            //
            this.solution = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure?id=" + solution.Id, solution).Result;
            crateStorage = _crate.FromDto(this.solution.CrateStorage);
            Assert.True(crateStorage.CratesOfType<StandardConfigurationControlsCM>().Count() > 0, "Crate StandardConfigurationControlsCM is missing in API response.");
            Assert.True(crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Count() > 0, "Crate StandardDesignTimeFieldsCM is missing in API response.");

            var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var controls = controlsCrate.Content.Controls;
            var dataSource = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "DataSource");
            dataSource.Value = "Get_Google_Sheet_Data";
            dataSource.selectedKey = "Get Google Sheet Data";
            var template = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "DocuSignTemplate");
            template.Value = "58521204-58af-4e65-8a77-4f4b51fef626";
            template.selectedKey = "Medical_Form_v1";
            template.ListItems.Add(new ListItem() { Value = "58521204-58af-4e65-8a77-4f4b51fef626", Key = "Medical_Form_v1" });
            var button = controls.OfType<Button>().FirstOrDefault();
            button.Clicked = true;

            //
            // Configure solution
            //
            using (var updater = _crate.UpdateStorage(this.solution))
            {

                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();
                updater.CrateStorage.Add(controlsCrate);
            }
            this.solution = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure?id=" + this.solution.Id, this.solution).Result;
            crateStorage = _crate.FromDto(this.solution.CrateStorage);
            Assert.AreEqual(3, this.solution.ChildrenActions.Count(), "Solution child actions failed to create.");

            // Delete Google action 
            HttpDeleteAsync(baseUrl + "actions?id=" + this.solution.ChildrenActions[0].Id).Wait();

            // Add Add Payload Manually action
            var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Processors };
            var activityTemplates = HttpPostAsync<ActivityCategory[], List<WebServiceActionSetDTO>>(baseUrl + "webservices/actions", activityCategoryParam).Result;
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Actions).Single(a => a.Name == "AddPayloadManually");

            var apmAction = new ActivityDTO()
            {
                ActivityTemplate = apmActivityTemplate,
                ActivityTemplateId = apmActivityTemplate.Id,
                Label = apmActivityTemplate.Label,
                Name = apmActivityTemplate.Name,
                ParentRouteNodeId = this.solution.Id,
                RootRouteNodeId = plan.Id,
                IsTempId = true
            };
            apmAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/save", apmAction).Result;
            Assert.NotNull(apmAction, "Add Payload Manually action failed to create");
            Assert.IsTrue(apmAction.Id != default(Guid), "Add Payload Manually action failed to create");

            //
            // Configure individual actions
            //

            // Add rows to Add Payload Manually action
            apmAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure", apmAction).Result;
            crateStorage = _crate.FromDto(apmAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var fieldList = controlsCrate.Content.Controls.OfType<FieldList>().First();
            fieldList.Value = @"[{""Key"":""Doctor"",""Value"":""Doctor1""},{""Key"":""Condition"",""Value"":""Condition1""}]";

            using (var updater = _crate.UpdateStorage(apmAction))
            {
                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();
                updater.CrateStorage.Add(controlsCrate);
            }

            // Move Add Payload Manually action to the beginning of the plan
            apmAction.Ordering = 1;
            apmAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/save", apmAction).Result;
            apmAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure", apmAction).Result;

            // Rename route
            var newName = plan.Name + " | " + DateTime.UtcNow.ToShortDateString() + " " +
                DateTime.UtcNow.ToShortTimeString();
            HttpPostAsync<object, RouteFullDTO>(baseUrl + "routes?id=" + plan.Id, 
                new { id=plan.Id, name = newName }).Wait();

            // Reconfigure Send DocuSign Envelope action
            var sendEnvelopeAction = this.solution.ChildrenActions.Single(a => a.Name == "Send DocuSign Envelope");

            crateStorage = _crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            docuSignTemplate.Value = "58521204-58af-4e65-8a77-4f4b51fef626";
            docuSignTemplate.selectedKey = "Medical_Form_v1";
            docuSignTemplate.ListItems.Add(new ListItem() { Value = "58521204-58af-4e65-8a77-4f4b51fef626", Key = "Medical_Form_v1" });
            
            using (var updater = _crate.UpdateStorage(sendEnvelopeAction))
            {
                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();
                updater.CrateStorage.Add(controlsCrate);
            }
            sendEnvelopeAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/save", sendEnvelopeAction).Result;
            sendEnvelopeAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure", sendEnvelopeAction).Result;
            
            // Reconfigure Map Fields to have it pick up upstream fields
            var mapFieldsAction = this.solution.ChildrenActions.Single(a => a.Name == "Map Fields");
            mapFieldsAction = HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure", mapFieldsAction).Result;

            // Activate route
            HttpPostAsync<string, string>(baseUrl + "routes/run?planId=" + plan.Id, null).Wait();
        }
    }
}
