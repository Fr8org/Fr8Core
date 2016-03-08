using Data.Control;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Hub.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.States;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    class Extract_Data_From_Envelopes_v1_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        ActivityDTO _solution;
        ICrateStorage _crateStorage;

        [Test]
        public async Task Extract_Data_From_Envelopes_EndToEnd()
        {
            string baseUrl = GetHubApiBaseUrl();

            var solutionCreateUrl = baseUrl + "activities/create?solutionName=Extract_Data_From_Envelopes";

            //
            // Create solution
            //
            var plan = await HttpPostAsync<string, RouteFullDTO>(solutionCreateUrl, null);
            var solution = plan.Subroutes.FirstOrDefault().Activities.FirstOrDefault();

            //
            // Send configuration request without authentication token
            //
            _solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + solution.Id, solution);
            _crateStorage = Crate.FromDto(_solution.CrateStorage);
            await ResolveAuth(_solution, _crateStorage);

            //
            // Send configuration request with authentication token
            //
            _solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + _solution.Id, _solution);
            _crateStorage = Crate.FromDto(_solution.CrateStorage);
            Assert.True(_crateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), "Crate StandardConfigurationControlsCM is missing in API response.");
            
            var controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var controls = controlsCrate.Content.Controls;

            //let's make some selections and go for re-configure
            var dataSource = controls.OfType<DropDownList>().FirstOrDefault(c => c.Name == "FinalActionsList");
            dataSource.Value = "Send_DocuSign_Envelope";
            dataSource.selectedKey = "Send DocuSign Envelope";

            using (var updater = Crate.GetUpdatableStorage(_solution))
            {
                updater.Remove<StandardConfigurationControlsCM>();
                updater.Add(controlsCrate);
            }

            _solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/configure?id=" + _solution.Id, _solution);
            _crateStorage = Crate.FromDto(_solution.CrateStorage);
            
            Assert.AreEqual(2, _solution.ChildrenActivities.Count(), "Solution child activities failed to create.");
            Assert.True(_solution.ChildrenActivities.Any(a => a.Label == "Monitor DocuSign Envelope Activity" && a.Ordering == 1));
            Assert.True(_solution.ChildrenActivities.Any(a => a.Label == "Send DocuSign Envelope" && a.Ordering == 2));

            //
            //Rename route
            //
            var newName = plan.Name + " | " + DateTime.UtcNow.ToShortDateString() + " " +
                DateTime.UtcNow.ToShortTimeString();
            await HttpPostAsync<object, RouteFullDTO>(_baseUrl + "plans?id=" + plan.Id,
                new { id = plan.Id, name = newName });

            //
            // Configure Monitor DocuSign Envelope action
            //
            var monitorDocuSignAction = _solution.ChildrenActivities.Single(a => a.Label == "Monitor DocuSign Envelope Activity");
            _crateStorage = Crate.FromDto(monitorDocuSignAction.CrateStorage);
                 
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            
            var checkbox = (CheckBox)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.CheckBox && c.Name == "Event_Envelope_Sent");
            checkbox.Selected = true;

            var radioButtonGroup = (RadioButtonGroup)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "TemplateRecipientPicker");
            radioButtonGroup.Radios[1].Selected = true;
            
            using (var updatableStorage = Crate.GetUpdatableStorage(monitorDocuSignAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            monitorDocuSignAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", monitorDocuSignAction);
            monitorDocuSignAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", monitorDocuSignAction);

            radioButtonGroup = (RadioButtonGroup)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "TemplateRecipientPicker");
            var docuSignTemplate = radioButtonGroup.Radios[1].Controls.OfType<DropDownList>().First();
            docuSignTemplate.Value = "9a4d2154-5b18-4316-9824-09432e62f458";
            docuSignTemplate.selectedKey = "Medical_Form_v1";
            docuSignTemplate.ListItems.Add(new ListItem() { Value = "9a4d2154-5b18-4316-9824-09432e62f458", Key = "Medical_Form_v1" });

            using (var updatableStorage = Crate.GetUpdatableStorage(monitorDocuSignAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            monitorDocuSignAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", monitorDocuSignAction);
            monitorDocuSignAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", monitorDocuSignAction);

            _crateStorage = Crate.FromDto(monitorDocuSignAction.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            radioButtonGroup = (RadioButtonGroup)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "TemplateRecipientPicker");
            docuSignTemplate = radioButtonGroup.Radios[1].Controls.OfType<DropDownList>().First();
            
            Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope activity.");
            Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope activity.");

            //
            // Configure Send DocuSign Envelope action
            //
            var sendEnvelopeAction = _solution.ChildrenActivities.Single(a => a.Label == "Send DocuSign Envelope");

            _crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            docuSignTemplate.Value = "9a4d2154-5b18-4316-9824-09432e62f458";
            docuSignTemplate.selectedKey = "Medical_Form_v1";
            docuSignTemplate.ListItems.Add(new ListItem() { Value = "9a4d2154-5b18-4316-9824-09432e62f458", Key = "Medical_Form_v1" });

            using (var updatableStorage = Crate.GetUpdatableStorage(sendEnvelopeAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);
            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", sendEnvelopeAction);
            
            // Follow-up Configuration
            _crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var emailField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingfreight testing role email");
            emailField.ValueSource = "specific";
            emailField.Value = TestEmail;
            emailField.TextValue = TestEmail;

            var emailNameField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingfreight testing role name");
            emailNameField.ValueSource = "specific";
            emailNameField.Value = TestEmailName;
            emailNameField.TextValue = TestEmailName;

            using (var updatableStorage = Crate.GetUpdatableStorage(sendEnvelopeAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);

            _crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");
            Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");

            emailField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingfreight testing role email");
            Assert.AreEqual(TestEmail, emailField.Value, "Email did not save on Send DocuSign Envelope action.");
            Assert.AreEqual(TestEmail, emailField.TextValue, "Email did not save on Send DocuSign Envelope action.");

            emailNameField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingfreight testing role name");
            Assert.AreEqual(TestEmailName, emailNameField.Value, "Email Name did not save on Send DocuSign Envelope action.");
            Assert.AreEqual(TestEmailName, emailNameField.TextValue, "Email Name did not save on Send DocuSign Envelope action.");

            // Delete Monitor action 
            await HttpDeleteAsync(_baseUrl + "activities?id=" + _solution.ChildrenActivities[0].Id);

            // Add Add Payload Manually action
            var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Processors };
            var activityTemplates = await HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(_baseUrl + "webservices/activities", activityCategoryParam);
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name == "AddPayloadManually");

            var apmAction = new ActivityDTO()
            {
                ActivityTemplate = apmActivityTemplate,
                Label = apmActivityTemplate.Label,
                ParentRouteNodeId = _solution.Id,
                RootRouteNodeId = plan.Id
            };
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", apmAction);
            Assert.NotNull(apmAction, "Add Payload Manually action failed to create");
            Assert.IsTrue(apmAction.Id != default(Guid), "Add Payload Manually activity failed to create");

            //Add rows to Add Payload Manually action
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", apmAction);
            _crateStorage = Crate.FromDto(apmAction.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var fieldList = controlsCrate.Content.Controls.OfType<FieldList>().First();
            fieldList.Value = @"[{""Key"":""Doctor"",""Value"":""Doctor1""},{""Key"":""Condition"",""Value"":""Condition1""}]";

            using (var updatableStorage = Crate.GetUpdatableStorage(apmAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            // Move Add Payload Manually action to the beginning of the plan
            apmAction.Ordering = 1;
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", apmAction);
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", apmAction);
            Assert.AreEqual(1, apmAction.Ordering, "Failed to reoder the activity Add Payload Manually");

            //
            // Activate and run plan
            //
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + plan.Id, null);

            //
            // Deactivate plan
            //
            await HttpPostAsync<string, string>(_baseUrl + "plans/deactivate?planId=" + plan.Id, null);

            //
            // Delete plan
            //
            await HttpDeleteAsync(_baseUrl + "plans?id=" + plan.Id);

        }

        private async Task ResolveAuth(ActivityDTO solution, ICrateStorage crateStorage)
        {
            var stAuthCrate = crateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            var defaultDocuSignAuthTokenExists = stAuthCrate == null;
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
        }
    }
}
