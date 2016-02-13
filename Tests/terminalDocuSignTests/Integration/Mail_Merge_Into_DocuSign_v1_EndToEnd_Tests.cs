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
using Data.Entities;

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

        [Test, Ignore]
        [ExpectedException(typeof(AssertionException))]
        public async Task TestEmail_ShouldBeMissing()
        {
            await Task.Delay(EmailAssert.RecentMsgThreshold); //to avoid false positives from any earlier tests

            // Verify that test email has been received
            // Actually it should not be received and AssertionException 
            // should be thrown
            EmailAssert.EmailReceived("dse_demo@docusign.net", "Test Message from Fr8");
        }

        [Test]
        public async void Mail_Merge_Into_DocuSign_EndToEnd()
        {
            var solutionCreateUrl = _baseUrl + "actions/create?solutionName=Mail_Merge_Into_DocuSign";

            //
            // Create solution
            //
            var plan = await HttpPostAsync<string, RouteFullDTO>(solutionCreateUrl, null);
            var solution = plan.Subroutes.FirstOrDefault().Activities.FirstOrDefault();

            //
            // Send configuration request without authentication token
            //
            this.solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure?id=" + solution.Id, solution);
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
                var token = await HttpPostAsync<CredentialsDTO, JObject>(_baseUrl + "authentication/token", creds);
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
                await HttpPostAsync<ManageAuthToken_Apply[], string>(_baseUrl + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken });
            }

            //
            // Send configuration request with authentication token
            //
            this.solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure?id=" + solution.Id, solution);
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
            //Rename route
            //
            var newName = plan.Name + " | " + DateTime.UtcNow.ToShortDateString() + " " +
                DateTime.UtcNow.ToShortTimeString();
            await HttpPostAsync<object, RouteFullDTO>(_baseUrl + "routes?id=" + plan.Id,
                new { id = plan.Id, name = newName });

            //
            // Configure solution
            //
            using (var updater = _crate.UpdateStorage(this.solution))
            {

                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();
                updater.CrateStorage.Add(controlsCrate);
            }
            this.solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure?id=" + this.solution.Id, this.solution);
            crateStorage = _crate.FromDto(this.solution.CrateStorage);
            Assert.AreEqual(2, this.solution.ChildrenActions.Count(), "Solution child actions failed to create.");

            // Delete Google action 
            await HttpDeleteAsync(_baseUrl + "actions?id=" + this.solution.ChildrenActions[0].Id);

            // Add Add Payload Manually action
            var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Processors };
            var activityTemplates = await HttpPostAsync<ActivityCategory[], List<WebServiceActionSetDTO>>(_baseUrl + "webservices/actions", activityCategoryParam);
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Actions).Single(a => a.Name == "AddPayloadManually");

            var apmAction = new ActivityDTO()
            {
                ActivityTemplate = apmActivityTemplate,
                Label = apmActivityTemplate.Label,
                ParentRouteNodeId = this.solution.Id,
                RootRouteNodeId = plan.Id
            };
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/save", apmAction);
            Assert.NotNull(apmAction, "Add Payload Manually action failed to create");
            Assert.IsTrue(apmAction.Id != default(Guid), "Add Payload Manually action failed to create");

            //
            // Configure Add Payload Manually action
            //

            //Add rows to Add Payload Manually action
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure", apmAction);
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
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/save", apmAction);
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure", apmAction);
            Assert.AreEqual(1, apmAction.Ordering, "Failed to reoder the action Add Payload Manually");

            var fr8CoreLoop = this.solution.ChildrenActions.Single(a => a.Label == "Fr8 Core Loop");
            
            //
            // Configure Send DocuSign Envelope action
            //
            var sendEnvelopeAction = fr8CoreLoop.ChildrenActions.Single(a => a.Label == "Send DocuSign Envelope");

            crateStorage = _crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            var docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            docuSignTemplate.Value = "9a4d2154-5b18-4316-9824-09432e62f458";
            docuSignTemplate.selectedKey = "Medical_Form_v1";
            docuSignTemplate.ListItems.Add(new ListItem() { Value = "9a4d2154-5b18-4316-9824-09432e62f458", Key = "Medical_Form_v1" });

            var emailField = controlsCrate.Content.Controls.OfType<TextSource>().First();
            emailField.ValueSource = "specific";
            emailField.Value = "freight.testing@gmail.com";
            emailField.TextValue = "freight.testing@gmail.com";

            using (var updater = _crate.UpdateStorage(sendEnvelopeAction))
            {
                updater.CrateStorage.Remove<StandardConfigurationControlsCM>();
                updater.CrateStorage.Add(controlsCrate);
            }
            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/save", sendEnvelopeAction);
            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure", sendEnvelopeAction);

            crateStorage = _crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");
            Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");

            emailField = controlsCrate.Content.Controls.OfType<TextSource>().First();
            Assert.AreEqual("freight.testing@gmail.com", emailField.Value, "Email did not save on Send DocuSign Envelope action.");
            Assert.AreEqual("freight.testing@gmail.com", emailField.TextValue, "Email did not save on Send DocuSign Envelope action.");

            //
            // Configure Map Fields action
            //

            //// Reconfigure Map Fields to have it pick up upstream fields
            //var mapFieldsAction = this.solution.ChildrenActions.Single(a => a.Name == "Map Fields");
            //mapFieldsAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure", mapFieldsAction);

            //// Configure mappings
            //crateStorage = _crate.FromDto(mapFieldsAction.CrateStorage);
            //controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            //var mapping = controlsCrate.Content.Controls.OfType<MappingPane>().First();
            //mapping.Value = @"[{""Key"":""Doctor"",""Value"":""Doctor""},{""Key"":""Condition"",""Value"":""Condition""}]";

            //using (var updater = _crate.UpdateStorage(mapFieldsAction))
            //{
            //    updater.CrateStorage.Remove<StandardConfigurationControlsCM>();
            //    updater.CrateStorage.Add(controlsCrate);
            //}
            //sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/save", mapFieldsAction);

            //
            // Activate and run plan
            //
            await HttpPostAsync<string, string>(_baseUrl + "routes/run?planId=" + plan.Id, null);

            //
            // Deactivate plan
            //
            //await HttpPostAsync<string, string>(_baseUrl + "routes/deactivate?planId=" + plan.Id, null);

            //
            // Delete plan
            //
            await HttpDeleteAsync(_baseUrl + "routes?id=" + plan.Id);

            // Verify that test email has been received
            EmailAssert.EmailReceived("dse_demo@docusign.net", "Test Message from Fr8");
        }
    }
}
