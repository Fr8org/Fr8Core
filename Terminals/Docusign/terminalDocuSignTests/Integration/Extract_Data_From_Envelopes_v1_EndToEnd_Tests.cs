using Fr8.Testing.Integration;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

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
            await RevokeTokens();

            string baseUrl = GetHubApiBaseUrl();

            var solutionCreateUrl = baseUrl + "plans?solutionName=Extract_Data_From_Envelopes";

            //
            // Create solution
            //
            var plan = await HttpPostAsync<string, PlanDTO>(solutionCreateUrl, null);
            var solution = plan.SubPlans.FirstOrDefault().Activities.FirstOrDefault();

            //
            // Send configuration request without authentication token
            //
            _solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure?id=" + solution.Id, solution);
            _crateStorage = Crate.FromDto(_solution.CrateStorage);
            var authTokenId = await ResolveAuth(_solution, _crateStorage);

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
            Assert.True(_solution.ChildrenActivities.Any(a => a.ActivityTemplate.Name == "Monitor_DocuSign_Envelope_Activity" && a.Ordering == 1),
                "Failed to detect Monitor DocuSign Envelope Activity as the first child activity");
            Assert.True(_solution.ChildrenActivities.Any(a => a.ActivityTemplate.Name == "Send_DocuSign_Envelope" && a.Ordering == 2),
                "Failed to detect Send DocuSign Envelope as the second child activity");


            var monitorDocuSignEnvelopeActivity = _solution.ChildrenActivities
                .Single(x => x.ActivityTemplate.Name == "Monitor_DocuSign_Envelope_Activity");

            //
            // Apply auth-token to child MonitorDocuSignEvnelope activity.
            //
            
            var applyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = monitorDocuSignEnvelopeActivity.Id,
                AuthTokenId = authTokenId,
                IsMain = false
            };

            await HttpPostAsync<AuthenticationTokenGrantDTO[], string>(
                _baseUrl + "authentication/tokens/grant",
                new AuthenticationTokenGrantDTO[] { applyToken }
            );
            

            monitorDocuSignEnvelopeActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseUrl + "activities/configure?id=" + monitorDocuSignEnvelopeActivity.Id,
                monitorDocuSignEnvelopeActivity
            );

            //
            // Rename route
            //
            var newName = plan.Name + " | " + DateTime.UtcNow.ToShortDateString() + " " +
                DateTime.UtcNow.ToShortTimeString();
            await HttpPostAsync<object, PlanDTO>(_baseUrl + "plans?id=" + plan.Id,
                new { id = plan.Id, name = newName });

            //
            // Configure Monitor DocuSign Envelope action
            //
            _crateStorage = Crate.FromDto(monitorDocuSignEnvelopeActivity.CrateStorage);

            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            var checkbox = (CheckBox)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.CheckBox && c.Name == "EnvelopeSent");
            checkbox.Selected = true;

            var radioButtonGroup = (RadioButtonGroup)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "TemplateRecipientPicker");
            radioButtonGroup.Radios[1].Selected = true;

            using (var updatableStorage = Crate.GetUpdatableStorage(monitorDocuSignEnvelopeActivity))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            monitorDocuSignEnvelopeActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", monitorDocuSignEnvelopeActivity);
            monitorDocuSignEnvelopeActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", monitorDocuSignEnvelopeActivity);

            radioButtonGroup = (RadioButtonGroup)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "TemplateRecipientPicker");
            var docuSignTemplate = radioButtonGroup.Radios[1].Controls.OfType<DropDownList>().First();
            docuSignTemplate.Value = "9a4d2154-5b18-4316-9824-09432e62f458";
            docuSignTemplate.selectedKey = "Medical_Form_v1";
            docuSignTemplate.ListItems.Add(new ListItem() { Value = "9a4d2154-5b18-4316-9824-09432e62f458", Key = "Medical_Form_v1" });

            using (var updatableStorage = Crate.GetUpdatableStorage(monitorDocuSignEnvelopeActivity))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            monitorDocuSignEnvelopeActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", monitorDocuSignEnvelopeActivity);
            monitorDocuSignEnvelopeActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", monitorDocuSignEnvelopeActivity);

            _crateStorage = Crate.FromDto(monitorDocuSignEnvelopeActivity.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            radioButtonGroup = (RadioButtonGroup)controlsCrate.Content.Controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "TemplateRecipientPicker");
            docuSignTemplate = radioButtonGroup.Radios[1].Controls.OfType<DropDownList>().First();

            Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope activity.");
            Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope activity.");

            //
            // Configure Send DocuSign Envelope action
            //
            var sendEnvelopeAction = _solution.ChildrenActivities.Single(a => a.ActivityTemplate.Name == "Send_DocuSign_Envelope");

            var sendEnvelopeApplyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = sendEnvelopeAction.Id,
                AuthTokenId = authTokenId,
                IsMain = false
            };

            await HttpPostAsync<AuthenticationTokenGrantDTO[], string>(
                _baseUrl + "authentication/tokens/grant",
                new AuthenticationTokenGrantDTO[] { sendEnvelopeApplyToken }
            );

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(
                _baseUrl + "activities/configure?id=" + sendEnvelopeAction.Id,
                sendEnvelopeAction
            );
           

            using (var updatableStorage = Crate.GetUpdatableStorage(sendEnvelopeAction))
            {
                controlsCrate = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().First();

                docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
                docuSignTemplate.Value = "9a4d2154-5b18-4316-9824-09432e62f458";
                docuSignTemplate.selectedKey = "Medical_Form_v1";
                docuSignTemplate.ListItems.Add(new ListItem() { Value = "9a4d2154-5b18-4316-9824-09432e62f458", Key = "Medical_Form_v1" });
            }

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);
            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", sendEnvelopeAction);

            // Follow-up Configuration

            TextSource emailField;
            TextSource emailNameField;

            using (var updatableStorage = Crate.GetUpdatableStorage(sendEnvelopeAction))
            {
                controlsCrate = updatableStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                emailField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.InitialLabel == "freight testing role email");
                emailField.ValueSource = "specific";
                emailField.Value = TestEmail;
                emailField.TextValue = TestEmail;

                emailNameField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.InitialLabel == "freight testing role name");
                emailNameField.ValueSource = "specific";
                emailNameField.Value = TestEmailName;
                emailNameField.TextValue = TestEmailName;
            }

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);

            _crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");
            Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");

            emailField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.InitialLabel == "freight testing role email");
            Assert.AreEqual(TestEmail, emailField.Value, "Email did not save on Send DocuSign Envelope action.");
            Assert.AreEqual(TestEmail, emailField.TextValue, "Email did not save on Send DocuSign Envelope action.");

            emailNameField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.InitialLabel == "freight testing role name");
            Assert.AreEqual(TestEmailName, emailNameField.Value, "Email Name did not save on Send DocuSign Envelope action.");
            Assert.AreEqual(TestEmailName, emailNameField.TextValue, "Email Name did not save on Send DocuSign Envelope action.");

            // Delete Monitor action 
            await HttpDeleteAsync(_baseUrl + "activities?id=" + _solution.ChildrenActivities[0].Id);

            // Add Add Payload Manually action
            var activityCategoryParam = ActivityCategories.ProcessId.ToString();
            var activityTemplates = await HttpGetAsync<List<WebServiceActivitySetDTO>>(_baseUrl + "webservices?id=" + activityCategoryParam);
            var apmActivityTemplate = activityTemplates
                .SelectMany(a => a.Activities)
                .Single(a => a.Name == "Add_Payload_Manually");
            var activityTemplateSummary = new ActivityTemplateSummaryDTO
                                        {
                                            Name = apmActivityTemplate.Name,
                                            Version = apmActivityTemplate.Version,
                                            TerminalName = apmActivityTemplate.Terminal.Name,
                                            TerminalVersion = apmActivityTemplate.Terminal.Version
                                        };
            var apmAction = new ActivityDTO()
            {
                ActivityTemplate = activityTemplateSummary,
                ParentPlanNodeId = _solution.Id,
                RootPlanNodeId = plan.Id
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
            await HttpPostAsync<string, ContainerDTO>(_baseUrl + "plans/run?planId=" + plan.Id, null);

            //
            // Deactivate plan
            //
            await HttpPostAsync<string, string>(_baseUrl + "plans/deactivate?planId=" + plan.Id, null);

            //
            // Delete plan
            //
            //await HttpDeleteAsync(_baseUrl + "plans?id=" + plan.Id);
        }

        private async Task<Guid> ResolveAuth(ActivityDTO solution, ICrateStorage crateStorage)
        {
            Guid? tokenGuid = null;

            var stAuthCrate = crateStorage
                .CratesOfType<StandardAuthenticationCM>()
                .FirstOrDefault();

            if (stAuthCrate != null)
            {
                var terminalsAndTokens =
                    await HttpGetAsync<AuthenticationTokenTerminalDTO[]>(
                        _baseUrl + "authentication/tokens"
                    );

                var terminalDocuSign = terminalsAndTokens
                    .SingleOrDefault(x => x.Name == "terminalDocuSign");

                if (terminalDocuSign != null)
                {
                    var token = terminalDocuSign.AuthTokens.FirstOrDefault(x => x.IsMain);
                    if (token == null)
                    {
                        token = terminalDocuSign.AuthTokens.FirstOrDefault();
                    }

                    Assert.NotNull(token, "Failed to get the auth token for Docusign terminal. ");
                    tokenGuid = token.Id;
                }

                if (!tokenGuid.HasValue)
                {
                    var creds = GetDocuSignCredentials();
                    creds.Terminal = new TerminalSummaryDTO
                    {
                        Name = solution.ActivityTemplate.TerminalName,
                        Version = solution.ActivityTemplate.TerminalVersion
                    };

                    var token = await HttpPostAsync<CredentialsDTO, JObject>(
                        _baseUrl + "authentication/token",
                        creds
                    );

                    Assert.AreEqual(
                        false,
                        string.IsNullOrEmpty(token["authTokenId"].Value<string>()),
                        "AuthTokenId is missing in API response."
                    );

                    tokenGuid = Guid.Parse(token["authTokenId"].Value<string>());
                }
            }

            var applyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = solution.Id,
                AuthTokenId = tokenGuid.Value,
                IsMain = false
            };

            await HttpPostAsync<AuthenticationTokenGrantDTO[], string>(
                _baseUrl + "authentication/tokens/grant",
                new AuthenticationTokenGrantDTO[] { applyToken }
            );

            return tokenGuid.Value;
        }
    }
}
