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
        ICrateStorage crateStorage;

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
        public async Task Mail_Merge_Into_DocuSign_EndToEnd()
        {
            try {
                var solutionCreateUrl = _baseUrl + "activities/create?solutionName=Mail_Merge_Into_DocuSign";

                //
                // Create solution
                //
                var plan = await HttpPostAsync<string, RouteFullDTO>(solutionCreateUrl, null);
                var solution = plan.Subroutes.FirstOrDefault().Activities.FirstOrDefault();

                //
                // Send configuration request without authentication token
                //
                this.solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + solution.Id, solution);
                crateStorage = _crateManager.FromDto(this.solution.CrateStorage);
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
                    Assert.AreNotEqual(token["error"].ToString(), "Unable to authenticate in DocuSign service, invalid login name or password.", "DocuSign auth error. Perhaps max number of tokens is exceeded.");
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
                this.solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + solution.Id, solution);
                crateStorage = _crateManager.FromDto(this.solution.CrateStorage);
                Assert.True(crateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), "Crate StandardConfigurationControlsCM is missing in API response.");
                Assert.True(crateStorage.CratesOfType<StandardDesignTimeFieldsCM>().Any(), "Crate StandardDesignTimeFieldsCM is missing in API response.");

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
                using (var crateStorage = _crateManager.GetUpdatableStorage(this.solution))
                {

                    crateStorage.Remove<StandardConfigurationControlsCM>();
                    crateStorage.Add(controlsCrate);
                }
                this.solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + this.solution.Id, this.solution);
                crateStorage = _crateManager.FromDto(this.solution.CrateStorage);
                Assert.AreEqual(2, this.solution.ChildrenActivities.Count(), "Solution child actions failed to create.");

                // Delete Google action 
                await HttpDeleteAsync(_baseUrl + "actions?id=" + this.solution.ChildrenActivities[0].Id);

                // Add Add Payload Manually action
                var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Processors };
                var activityTemplates = await HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(_baseUrl + "webservices/activities", activityCategoryParam);
                var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name == "AddPayloadManually");

                var apmAction = new ActivityDTO()
                {
                    ActivityTemplate = apmActivityTemplate,
                    Label = apmActivityTemplate.Label,
                    ParentRouteNodeId = this.solution.Id,
                    RootRouteNodeId = plan.Id
                };
                apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", apmAction);
                Assert.NotNull(apmAction, "Add Payload Manually action failed to create");
                Assert.IsTrue(apmAction.Id != default(Guid), "Add Payload Manually action failed to create");

                //
                // Configure Add Payload Manually action
                //

                //Add rows to Add Payload Manually action
                apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", apmAction);
                crateStorage = _crateManager.FromDto(apmAction.CrateStorage);
                controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var fieldList = controlsCrate.Content.Controls.OfType<FieldList>().First();
                fieldList.Value = @"[{""Key"":""Doctor"",""Value"":""Doctor1""},{""Key"":""Condition"",""Value"":""Condition1""}]";

                using (var updatableStorage = _crateManager.GetUpdatableStorage(apmAction))
                {
                    updatableStorage.Remove<StandardConfigurationControlsCM>();
                    updatableStorage.Add(controlsCrate);
                }

                // Move Add Payload Manually action to the beginning of the plan
                apmAction.Ordering = 1;
                apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", apmAction);
                apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", apmAction);
                Assert.AreEqual(1, apmAction.Ordering, "Failed to reoder the action Add Payload Manually");

                var fr8CoreLoop = this.solution.ChildrenActivities.Single(a =>  a.Label.Equals("loop", StringComparison.InvariantCultureIgnoreCase));

                fr8CoreLoop = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", fr8CoreLoop);
                //we should update fr8Core loop to loop through manually added payload
                var fr8CoreLoopCrateStorage = _crateManager.FromDto(fr8CoreLoop.CrateStorage);
                var loopConfigCrate = fr8CoreLoopCrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var loopConfigControls = loopConfigCrate.Content.Controls;
                var availableManifests = (DropDownList)loopConfigControls.Single(c => c.Name == "Available_Manifests");
                var availableLabels = (DropDownList)loopConfigControls.Single(c => c.Name == "Available_Labels");
                availableManifests.Value = "Standard Design-Time Fields";
                availableManifests.selectedKey = "Standard Design-Time Fields";
                availableLabels.Value = "ManuallyAddedPayload";
                availableLabels.selectedKey = "ManuallyAddedPayload";
                using (var updatableStorage = _crateManager.GetUpdatableStorage(fr8CoreLoop))
                {
                    updatableStorage.Remove<StandardConfigurationControlsCM>();
                    updatableStorage.Add(loopConfigCrate);
                }

                fr8CoreLoop = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", fr8CoreLoop);

                //
                // Configure Send DocuSign Envelope action
                //
                var sendEnvelopeAction = fr8CoreLoop.ChildrenActivities.Single(a => a.Label == "Send DocuSign Envelope");

                crateStorage = _crateManager.FromDto(sendEnvelopeAction.CrateStorage);
                controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

                var docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
                docuSignTemplate.Value = "9a4d2154-5b18-4316-9824-09432e62f458";
                docuSignTemplate.selectedKey = "Medical_Form_v1";
                docuSignTemplate.ListItems.Add(new ListItem() { Value = "9a4d2154-5b18-4316-9824-09432e62f458", Key = "Medical_Form_v1" });

                var emailField = controlsCrate.Content.Controls.OfType<TextSource>().First();
                emailField.ValueSource = "specific";
                emailField.Value = TestEmail;
                emailField.TextValue = TestEmail;

                using (var updatableStorage = _crateManager.GetUpdatableStorage(sendEnvelopeAction))
                {
                    updatableStorage.Remove<StandardConfigurationControlsCM>();
                    updatableStorage.Add(controlsCrate);
                }
                sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);
                sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", sendEnvelopeAction);

                crateStorage = _crateManager.FromDto(sendEnvelopeAction.CrateStorage);
                controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

                docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
                Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");
                Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");

                emailField = controlsCrate.Content.Controls.OfType<TextSource>().First();
                Assert.AreEqual(TestEmail, emailField.Value, "Email did not save on Send DocuSign Envelope action.");
                Assert.AreEqual(TestEmail, emailField.TextValue, "Email did not save on Send DocuSign Envelope action.");

                //
                // Configure Map Fields action
                //

                //// Reconfigure Map Fields to have it pick up upstream fields
                //var mapFieldsAction = this.solution.ChildrenActions.Single(a => a.Name == "Map Fields");
                //mapFieldsAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/configure", mapFieldsAction);

                //// Configure mappings
                //crateStorage = _crateManager.FromDto(mapFieldsAction.CrateStorage);
                //controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                //var mapping = controlsCrate.Content.Controls.OfType<MappingPane>().First();
                //mapping.Value = @"[{""Key"":""Doctor"",""Value"":""Doctor""},{""Key"":""Condition"",""Value"":""Condition""}]";

                //using (var crateStorage = _crateManager.GetUpdatableStorage(mapFieldsAction))
                //{
                //    crateStorage.Remove<StandardConfigurationControlsCM>();
                //    crateStorage.Add(controlsCrate);
                //}
                //sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "actions/save", mapFieldsAction);

                //
                // Activate and run plan
                //
                await HttpPostAsync<string, string>(_baseUrl + "routes/run?planId=" + plan.Id, null);

                //
                // Deactivate plan
                //
                await HttpPostAsync<string, string>(_baseUrl + "routes/deactivate?planId=" + plan.Id, null);
    
                //
                // Delete plan
                //
                await HttpDeleteAsync(_baseUrl + "routes?id=" + plan.Id);

                // Verify that test email has been received
                //EmailAssert.EmailReceived("dse_demo@docusign.net", "Test Message from Fr8");
            }
            catch (Exception ex)
                {
                throw;
            }
        }
    }
}
