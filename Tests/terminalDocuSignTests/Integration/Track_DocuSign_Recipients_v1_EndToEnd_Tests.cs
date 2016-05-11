using System;
using System.Linq;
using NUnit.Framework;
using HealthMonitor.Utility;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Hub.Managers;
using Newtonsoft.Json.Linq;

namespace terminalDocuSignTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Track_DocuSign_Recipients_v1_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        ActivityDTO _solution;
        ICrateStorage _crateStorage;


        private void ShouldHaveCorrectCrateStructure(ICrateStorage crateStorage)
        {
            Assert.True(crateStorage.CratesOfType<StandardConfigurationControlsCM>().Any(), "Crate StandardConfigurationControlsCM is missing in API response.");
            Assert.True(crateStorage.CratesOfType<FieldDescriptionsCM>().Any(c => c.Label == "AvailableTemplates"), "FieldDescriptionsCM with label \"AvailableTemplates\" is missing in API response.");
            Assert.True(crateStorage.CratesOfType<FieldDescriptionsCM>().Any(c => c.Label == "AvailableHandlers"), "FieldDescriptionsCM with label \"AvailableHandlers\" is missing in API response.");
            Assert.True(crateStorage.CratesOfType<FieldDescriptionsCM>().Any(c => c.Label == "AvailableRecipientEvents"), "FieldDescriptionsCM with label \"AvailableRecipientEvents\" is missing in API response.");

            var templatesCrate = crateStorage.CratesOfType<FieldDescriptionsCM>().Single(c => c.Label == "AvailableTemplates");
            var handlersCrate = crateStorage.CratesOfType<FieldDescriptionsCM>().Single(c => c.Label == "AvailableHandlers");
            var recipientEventsCrate = crateStorage.CratesOfType<FieldDescriptionsCM>().Single(c => c.Label == "AvailableRecipientEvents");

            Assert.True(templatesCrate.Content.Fields.Any(), "There are no fields in AvailableTemplates Crate");
            Assert.True(handlersCrate.Content.Fields.Any(), "There are no fields in AvailableHandlers Crate");
            Assert.True(recipientEventsCrate.Content.Fields.Any(), "There are no fields in AvailableRecipientEvents Crate");
        }

        private async Task PostFakeEvent()
        {
            var docusignTerminalUrl = TerminalUrl;
            //everything seems perfect -> let's fake a docusign event
            var fakeDocuSignEventContent = @"<?xml version=""1.0"" encoding=""utf-8""?><DocuSignEnvelopeInformation xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://www.docusign.net/API/3.0""><EnvelopeStatus><RecipientStatuses><RecipientStatus><Type>Signer</Type><Email>test@fr8.co</Email><UserName>Fr8 Test User</UserName><RoutingOrder>1</RoutingOrder><Sent>2016-02-09T04:19:58.41</Sent><DeclineReason xsi:nil=""true"" /><Status>Sent</Status><RecipientIPAddress /><CustomFields /><TabStatuses><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>189</XPosition><YPosition>326</YPosition><TabLabel>Text 5</TabLabel><TabName>Text</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Text</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>675</XPosition><YPosition>504</YPosition><TabLabel>Text 8</TabLabel><TabName>Text</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Text</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>941</XPosition><YPosition>860</YPosition><TabLabel>Checkbox 1</TabLabel><TabName>Checkbox</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Checkbox</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>1022</XPosition><YPosition>860</YPosition><TabLabel>Checkbox 2</TabLabel><TabName>Checkbox</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Checkbox</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>941</XPosition><YPosition>889</YPosition><TabLabel>Checkbox 3</TabLabel><TabName>Checkbox</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Checkbox</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>1022</XPosition><YPosition>889</YPosition><TabLabel>Checkbox 4</TabLabel><TabName>Checkbox</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Checkbox</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>939</XPosition><YPosition>918</YPosition><TabLabel>Checkbox 5</TabLabel><TabName>Checkbox</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Checkbox</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>1022</XPosition><YPosition>918</YPosition><TabLabel>Checkbox 6</TabLabel><TabName>Checkbox</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Checkbox</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>812</XPosition><YPosition>192</YPosition><TabLabel>DateOfBirth</TabLabel><TabName>Text</TabName><TabValue /><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue /><ValidationPattern /><CustomTabType>Date</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>364</XPosition><YPosition>400</YPosition><TabLabel>Condition</TabLabel><TabName>Text</TabName><TabValue>Marthambles</TabValue><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue>Marthambles</OriginalValue><ValidationPattern /><CustomTabType>Text</CustomTabType></TabStatus><TabStatus><TabType>Custom</TabType><Status>Active</Status><XPosition>181</XPosition><YPosition>239</YPosition><TabLabel>Doctor</TabLabel><TabName>Text</TabName><TabValue>Dohemann</TabValue><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue>Dohemann</OriginalValue><ValidationPattern /><CustomTabType>Text</CustomTabType></TabStatus><TabStatus><TabType>FullName</TabType><Status>Active</Status><XPosition>243</XPosition><YPosition>196</YPosition><TabLabel>Name 1</TabLabel><TabName>Name</TabName><TabValue>Bahadir Bozdag</TabValue><DocumentID>1</DocumentID><PageNumber>1</PageNumber><OriginalValue>Joanna Smith</OriginalValue></TabStatus></TabStatuses><AccountStatus>Active</AccountStatus><RecipientId>3c498fd2-499c-414c-a980-6b3a8a108643</RecipientId></RecipientStatus></RecipientStatuses><TimeGenerated>2016-02-09T04:22:25.6749113</TimeGenerated><EnvelopeID>fffb6908-4c84-4a05-9fb4-e3e94d5aaa1a</EnvelopeID><Subject>Please DocuSign: medical_intake_form.pdf</Subject><UserName>Dockyard Developer</UserName><Email>freight.testing@gmail.com</Email><Status>Sent</Status><Created>2016-02-09T04:19:40.08</Created><Sent>2016-02-09T04:19:58.567</Sent><ACStatus>Original</ACStatus><ACStatusDate>2016-02-09T04:19:40.08</ACStatusDate><ACHolder>Dockyard Developer</ACHolder><ACHolderEmail>freight.testing@gmail.com</ACHolderEmail><ACHolderLocation>DocuSign</ACHolderLocation><SigningLocation>Online</SigningLocation><SenderIPAddress>178.233.137.179</SenderIPAddress><EnvelopePDFHash /><CustomFields /><AutoNavigation>true</AutoNavigation><EnvelopeIdStamping>true</EnvelopeIdStamping><AuthoritativeCopy>false</AuthoritativeCopy><DocumentStatuses><DocumentStatus><ID>1</ID><Name>medical_intake_form.pdf</Name><TemplateName>Medical_Form_v1</TemplateName><Sequence>1</Sequence></DocumentStatus></DocumentStatuses></EnvelopeStatus></DocuSignEnvelopeInformation>";
            var httpContent = new StringContent(fakeDocuSignEventContent, Encoding.UTF8, "application/xml");
            await HttpPostAsync<string>(docusignTerminalUrl + "/terminals/terminalDocuSign/events", httpContent);
        }

        /// <summary>
        /// This is for manual testing only
        /// Please don't delete this method. i (bahadir) use this to quick test docusign events
        /// </summary>
        /// <returns></returns>
        [Test, Ignore("This is for manual testing only. Bahadir uses this to quick test docusign events.")]
        public async Task SendFakeEvent()
        {
            await PostFakeEvent();
        }

        [Test]
        public async Task Track_DocuSign_Recipients_EndToEnd()
        {
            await RevokeTokens();

            string baseUrl = GetHubApiBaseUrl();

            var solutionCreateUrl = baseUrl + "plans/createSolution?solutionName=Track_DocuSign_Recipients";


            //
            // Create solution
            //
            var plan = await HttpPostAsync<string, PlanDTO>(solutionCreateUrl, null);
            var solution = plan.Plan.SubPlans.FirstOrDefault().Activities.FirstOrDefault();

            var planReloadUrl = string.Format(baseUrl + "plans/full/{0}", plan.Plan.Id);

            //
            // Send configuration request without authentication token
            //
            this._solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/configure?id=" + solution.Id, solution);
            _crateStorage = Crate.FromDto(this._solution.CrateStorage);
            var stAuthCrate = _crateStorage.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            bool defaultDocuSignAuthTokenExists = stAuthCrate == null;

            if (!defaultDocuSignAuthTokenExists)
            {
                //
                // Authenticate with DocuSign
                //
                var creds = GetDocuSignCredentials();
                creds.Terminal = solution.ActivityTemplate.Terminal;

                var token = await HttpPostAsync<CredentialsDTO, JObject>(baseUrl + "authentication/token", creds);
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
                await HttpPostAsync<ManageAuthToken_Apply[], string>(baseUrl + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken });
                //let's give it some time to create MonitorDocusignEvents plan
                await Task.Delay(TimeSpan.FromSeconds(15));

                //let's post a fake event to populate MT database
                await PostFakeEvent();
            }




            //
            // Send configuration request with authentication token
            //
            this._solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/configure?id=" + solution.Id, solution);
            _crateStorage = Crate.FromDto(this._solution.CrateStorage);

            ShouldHaveCorrectCrateStructure(_crateStorage);
            Assert.True(this._solution.ChildrenActivities.Length == 0);

            var controlsCrate = _crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var controls = controlsCrate.Content.Controls;

            #region CHECK_CONFIGURATION_CONTROLS

            Assert.AreEqual(5, controls.Count);
            Assert.True(controls.Any(c => c.Type == ControlTypes.DropDownList && c.Name == "NotificationHandler"));
            Assert.True(controls.Any(c => c.Type == ControlTypes.TextBlock && c.Name == "EventInfo"));
            Assert.True(controls.Any(c => c.Type == ControlTypes.DropDownList && c.Name == "RecipientEvent"));
            Assert.True(controls.Any(c => c.Type == ControlTypes.Duration && c.Name == "TimePeriod"));
            Assert.True(controls.Any(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "Track_Which_Envelopes"));

            var radioButtonGroup = (RadioButtonGroup)controls.Single(c => c.Type == ControlTypes.RadioButtonGroup && c.Name == "Track_Which_Envelopes");
            Assert.AreEqual(2, radioButtonGroup.Radios.Count);
            Assert.True(radioButtonGroup.Radios.Any(c => c.Name == "SpecificRecipient"));
            Assert.True(radioButtonGroup.Radios.Any(c => c.Name == "SpecificTemplate"));

            var specificRecipientOption = (RadioButtonOption)radioButtonGroup.Radios.Single(c => c.Name == "SpecificRecipient");
            Assert.AreEqual(1, specificRecipientOption.Controls.Count);
            Assert.True(specificRecipientOption.Controls.Any(c => c.Name == "SpecificRecipient" && c.Type == ControlTypes.TextBox));

            var specificTemplateOption = (RadioButtonOption)radioButtonGroup.Radios.Single(c => c.Name == "SpecificTemplate");
            Assert.AreEqual(1, specificTemplateOption.Controls.Count);
            Assert.True(specificTemplateOption.Controls.Any(c => c.Name == "SpecificTemplate" && c.Type == ControlTypes.DropDownList));

            #endregion

            //let's make some selections and go for re-configure
            //RDN shouldn't update it's child activity structure until we select a notification method
            radioButtonGroup.Radios[0].Selected = true;
            radioButtonGroup.Radios[1].Selected = false;
            specificRecipientOption.Controls[0].Value = "test@fr8.co";

            using (var updater = Crate.GetUpdatableStorage(_solution))
            {
                updater.Remove<StandardConfigurationControlsCM>();
                updater.Add(controlsCrate);
            }

            this._solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/configure?id=" + this._solution.Id, this._solution);
            _crateStorage = Crate.FromDto(this._solution.CrateStorage);
            ShouldHaveCorrectCrateStructure(_crateStorage);
            Assert.True(this._solution.ChildrenActivities.Length == 0);

            //everything seems perfect for now
            //let's force RDN for a followup configuration

            var timePeriod = (Duration)controls.Single(c => c.Type == ControlTypes.Duration && c.Name == "TimePeriod");
            var notificationHandler = (DropDownList)controls.Single(c => c.Type == ControlTypes.DropDownList && c.Name == "NotificationHandler");
            var recipientEvent = (DropDownList)controls.Single(c => c.Type == ControlTypes.DropDownList && c.Name == "RecipientEvent");

            timePeriod.Days = 0;
            timePeriod.Hours = 0;
            timePeriod.Minutes = 0;
            var handlersCrate = _crateStorage.CratesOfType<FieldDescriptionsCM>().Single(c => c.Label == "AvailableHandlers");
            var emailHandler = handlersCrate.Content.Fields.Single(c => c.Key.Contains("Send Email"));
            notificationHandler.Value = emailHandler.Value;
            notificationHandler.selectedKey = emailHandler.Key;
            var recipientEventsCrate = _crateStorage.CratesOfType<FieldDescriptionsCM>().Single(c => c.Label == "AvailableRecipientEvents");
            recipientEvent.Value = recipientEventsCrate.Content.Fields[1].Value;
            recipientEvent.selectedKey = recipientEventsCrate.Content.Fields[1].Key;

            using (var updatableStorage = Crate.GetUpdatableStorage(_solution))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            this._solution = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/configure?id=" + this._solution.Id, this._solution);
            _crateStorage = Crate.FromDto(this._solution.CrateStorage);

            //from now on our solution should have followup crate structure
            Assert.True(this._solution.ChildrenActivities.Length == 4, "Solution child actions failed to create.");

            Assert.True(this._solution.ChildrenActivities.Any(a => a.Name == "Monitor Docusign Envelope Activity" && a.Ordering == 1));
            Assert.True(this._solution.ChildrenActivities.Any(a => a.Name == "Set Delay" && a.Ordering == 2));
            Assert.True(this._solution.ChildrenActivities.Any(a => a.Name == "Query Fr8 Warehouse" && a.Ordering == 3));
            Assert.True(this._solution.ChildrenActivities.Any(a => a.Name == "Test Incoming Data" && a.Ordering == 4));

            plan = await HttpGetAsync<PlanDTO>(planReloadUrl);
            Assert.AreEqual(3, plan.Plan.SubPlans.First().Activities.Count);
            Assert.True(plan.Plan.SubPlans.First().Activities.Any(a => a.Name == "Build a Message" && a.Ordering == 2));
            var emailActivity = plan.Plan.SubPlans.First().Activities.Last();
            Assert.True(emailActivity.Name == notificationHandler.selectedKey);

            //let's configure email settings

            //let's configure this
            emailActivity = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/configure?id=" + emailActivity.Id, emailActivity);
            var emailCrateStorage = Crate.GetStorage(emailActivity);

            var emailControlsCrate = emailCrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var emailAddress = (TextSource)emailControlsCrate.Content.Controls.Single(c => c.Name == "EmailAddress");
            var emailSubject = (TextSource)emailControlsCrate.Content.Controls.Single(c => c.Name == "EmailSubject");
            var emailBody = (TextSource)emailControlsCrate.Content.Controls.Single(c => c.Name == "EmailBody");

            var upstreamFieldDescription = await HttpGetAsync<IncomingCratesDTO>(baseUrl + "plannodes/available_data?id=" + emailActivity.Id);

            Assert.True(upstreamFieldDescription.AvailableFields.Any(y => y.Key == "NotificationMessage"));
            Assert.AreEqual("NotificationMessage", emailBody.Value);

            emailAddress.ValueSource = "specific";
            emailAddress.Value = "freight.testing@gmail.com";
            emailAddress.TextValue = "freight.testing@gmail.com";

            emailSubject.ValueSource = "specific";
            emailSubject.Value = "Fr8-TrackDocuSignRecipientsTest";
            emailSubject.TextValue = "Fr8-TrackDocuSignRecipientsTest";

            emailBody.ValueSource = "specific";
            emailBody.Value = "Fr8-TrackDocuSignRecipientsTest";
            emailBody.TextValue = "Fr8-TrackDocuSignRecipientsTest";

            using (var updatableStorage = Crate.GetUpdatableStorage(emailActivity))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(emailControlsCrate);
            }

            //save changes
            await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "activities/save", emailActivity);

            //
            //Rename plan
            //
            var newName = plan.Plan.Name + " | " + DateTime.UtcNow.ToShortDateString() + " " + DateTime.UtcNow.ToShortTimeString();
            await HttpPostAsync<object, PlanFullDTO>(baseUrl + "plans?id=" + plan.Plan.Id, new { id = plan.Plan.Id, name = newName });

            //let's activate our plan
            await HttpPostAsync<string, string>(baseUrl + "plans/activate?planId=" + plan.Plan.Id, null);


            //everything seems perfect -> let's fake a docusign event
            await PostFakeEvent();

            //let's wait 45 seconds before continuing
            await Task.Delay(TimeSpan.FromSeconds(45));
            //we should have received an email about this operation


            //
            // Deactivate plan
            //
            await HttpPostAsync<string, string>(baseUrl + "plans/deactivate?planid=" + plan.Plan.Id, plan.Plan.Id.ToString());

            //
            // Delete plan
            //
            await HttpDeleteAsync(baseUrl + "plans?id=" + plan.Plan.Id);

            // Verify that test email has been received
            //EmailAssert.EmailReceived("fr8ops@fr8.company", "Fr8-TrackDocuSignRecipientsTest");

        }
    }
}
