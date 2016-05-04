using System;
using NUnit.Framework;
using HealthMonitor.Utility;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Hub.Managers;
using Data.States;
using DocuSign.eSign.Api;
using Fr8Data.Constants;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminaBaselTests.Tools.Activities;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using UtilitiesTesting.Fixtures;

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
        #region Properties

        private ActivityDTO solution;
        private ICrateStorage crateStorage;
        private terminaBaselTests.Tools.Terminals.IntegrationTestTools_terminalDocuSign _terminalDocuSignTestTools;
        private IntegrationTestTools_terminalDocuSign _docuSignActivitiesTestTools;

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        #endregion

        public Mail_Merge_Into_DocuSign_v1_EndToEnd_Tests()
        {
            _terminalDocuSignTestTools = new terminaBaselTests.Tools.Terminals.IntegrationTestTools_terminalDocuSign(this);
            _docuSignActivitiesTestTools = new IntegrationTestTools_terminalDocuSign(this);
        }

        [Test]
        public async Task Mail_Merge_Into_DocuSign_EndToEnd()
        {
            await RevokeTokens();

            var googleAuthTokenId = await ExtractGoogleDefaultToken();

            //
            // Create solution
            //
            var parameters = await _docuSignActivitiesTestTools.CreateAndConfigure_MailMergeIntoDocuSign_Solution("Get_Google_Sheet_Data", "Get Google Sheet Data", "9a4d2154-5b18-4316-9824-09432e62f458", "Medical_Form_v1", true);
            this.solution = parameters.Item1;
            var plan = parameters.Item2;

            // Assert Loop activity has CrateChooser with assigned manifest types.
            var loopActivity = this.solution.ChildrenActivities[1];
            using (var loopCrateStorage = Crate.GetUpdatableStorage(loopActivity))
            {
                var loopControlsCrate = loopCrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
                var loopControls = loopControlsCrate.Content.Controls;

                var loopCrateChooser = loopControls
                    .SingleOrDefault(x => x.Type == ControlTypes.CrateChooser && x.Name == "Available_Crates") as CrateChooser;

                Assert.NotNull(loopCrateChooser, "Unable to find CrateChooser control from loop activity");
                Assert.AreEqual(1, loopCrateChooser.CrateDescriptions.Count, "Selected crate count is not equal to 1 on loopActivity CrateChooser");
                Assert.AreEqual("Standard Table Data", loopCrateChooser.CrateDescriptions[0].ManifestType, "Selected crate on CrateChooser doesn't have ManifestType: Standard Table Data");
                Assert.AreEqual("Table Generated From Google Sheet Data", loopCrateChooser.CrateDescriptions[0].Label, "Selected crate on CrateChooser doesn't have label: Table Generated From Google Sheet Data");

                loopCrateChooser.CrateDescriptions = new List<CrateDescriptionDTO>();
            }

            // Delete Google action 
            await HttpDeleteAsync(_baseUrl + "activities?id=" + this.solution.ChildrenActivities[0].Id);

            // Add Add Payload Manually action
            var activityCategoryParam = new ActivityCategory[] { ActivityCategory.Processors };
            var activityTemplates = await HttpPostAsync<ActivityCategory[], List<WebServiceActivitySetDTO>>(_baseUrl + "webservices/activities", activityCategoryParam);
            var apmActivityTemplate = activityTemplates.SelectMany(a => a.Activities).Single(a => a.Name == "AddPayloadManually");

            var apmAction = new ActivityDTO()
            {
                ActivityTemplate = apmActivityTemplate,
                Label = apmActivityTemplate.Label,
                ParentPlanNodeId = this.solution.Id,
                RootPlanNodeId = plan.Plan.Id
            };
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", apmAction);
            Assert.NotNull(apmAction, "Add Payload Manually action failed to create");
            Assert.IsTrue(apmAction.Id != default(Guid), "Add Payload Manually action failed to create");

            //
            // Configure Add Payload Manually action
            //

            //Add rows to Add Payload Manually action
            apmAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", apmAction);
            crateStorage = Crate.FromDto(apmAction.CrateStorage);
            var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
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
            Assert.AreEqual(1, apmAction.Ordering, "Failed to reoder the action Add Payload Manually");

            var fr8CoreLoop = this.solution.ChildrenActivities.Single(a => a.Label.Equals("loop", StringComparison.InvariantCultureIgnoreCase));

            using (var updatableStorage = Crate.UpdateStorage(() => fr8CoreLoop.CrateStorage))
            {
                var chooser = (CrateChooser)updatableStorage.CrateContentsOfType<StandardConfigurationControlsCM>().First().Controls.FirstOrDefault(c => c.Name == "Available_Crates");

                if (chooser?.CrateDescriptions != null)
                {
                    chooser.CrateDescriptions = new List<CrateDescriptionDTO>();
                }
            }
            
            fr8CoreLoop = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", fr8CoreLoop);
            //we should update fr8Core loop to loop through manually added payload
            var fr8CoreLoopCrateStorage = Crate.FromDto(fr8CoreLoop.CrateStorage);
            var loopConfigCrate = fr8CoreLoopCrateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var loopConfigControls = loopConfigCrate.Content.Controls;
            var crateChooser = (CrateChooser)loopConfigControls.FirstOrDefault(c => c.Name == "Available_Crates");

            Assert.NotNull(crateChooser, "Crate chooser was not found");

            var payloadDataCrate = crateChooser.CrateDescriptions.SingleOrDefault(c => c.ManifestId == (int)MT.StandardPayloadData);

            Assert.NotNull(payloadDataCrate, "StandardPayloadData was not found in rateChooser.CrateDescriptions. Available crate descriptions are: " + string.Join("\n", crateChooser.CrateDescriptions.Select(x=> $"{x.Label} of type {x.ManifestType}")));

            payloadDataCrate.Selected = true;
            using (var updatableStorage = Crate.GetUpdatableStorage(fr8CoreLoop))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(loopConfigCrate);
            }

            fr8CoreLoop = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", fr8CoreLoop);

            //
            // Configure Send DocuSign Envelope action
            //

            // Initial Configuration
            var sendEnvelopeAction = fr8CoreLoop.ChildrenActivities.Single(a => a.Label == "Send DocuSign Envelope");

            crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            var docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
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
            crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
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

            crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            Assert.AreEqual("9a4d2154-5b18-4316-9824-09432e62f458", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");
            Assert.AreEqual("Medical_Form_v1", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");

            emailField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingfreight testing role email");
            Assert.AreEqual(TestEmail, emailField.Value, "Email did not save on Send DocuSign Envelope action.");
            Assert.AreEqual(TestEmail, emailField.TextValue, "Email did not save on Send DocuSign Envelope action.");

            emailNameField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingfreight testing role name");
            Assert.AreEqual(TestEmailName, emailNameField.Value, "Email Name did not save on Send DocuSign Envelope action.");
            Assert.AreEqual(TestEmailName, emailNameField.TextValue, "Email Name did not save on Send DocuSign Envelope action.");

            //
            // Activate and run plan
            //
            var container = await HttpPostAsync<string, ContainerDTO>(_baseUrl + "plans/run?planId=" + plan.Plan.Id, null);
            Assert.AreEqual(container.State, State.Completed, "Container state is not equal to completed on Mail_Merge e2e test");

            //
            // Deactivate plan
            //
            await HttpPostAsync<string, string>(_baseUrl + "plans/deactivate?planId=" + plan.Plan.Id, null);

            // Verify that test email has been received
            EmailAssert.EmailReceived("dse_demo@docusign.net", "Test Message from Fr8");

            //
            // Delete plan
            //
            //await HttpDeleteAsync(_baseUrl + "plans?id=" + plan.Plan.Id);
        }

        [Test]
        public async Task Mail_Merge_Into_DocuSign_EndToEnd_Upstream_Values_From_Google_Check_Tabs()
        {
            //
            //Setup Test
            //
            await RevokeTokens();

            var terminalGoogleTestTools = new terminaBaselTests.Tools.Terminals.IntegrationTestTools_terminalGoogle(this);
            var googleActivityTestTools = new terminaBaselTests.Tools.Activities.IntegrationTestTools_terminalGoogle(this);
            var googleAuthTokenId = await terminalGoogleTestTools.ExtractGoogleDefaultToken();

            string spreadsheetName = Guid.NewGuid().ToString();
            string spreadsheetKeyWord = Guid.NewGuid().ToString();
            string worksheetName = "TestSheet";

            //create new excel spreadsheet inside google and insert one row of data inside the spreadsheet
            //spreadsheetKeyWord is an identifier that will help up later in the test to easily identify specific envelope
            var tableFixtureData = FixtureData.TestStandardTableData(TestEmail, spreadsheetKeyWord);
            string spreadsheetId = await terminalGoogleTestTools.CreateNewSpreadsheet(googleAuthTokenId, spreadsheetName, worksheetName, tableFixtureData);

            //
            // Create solution
            //
            var parameters = await _docuSignActivitiesTestTools.CreateAndConfigure_MailMergeIntoDocuSign_Solution("Get_Google_Sheet_Data",
                    "Get Google Sheet Data", "a439cedc-92a8-49ad-ab31-e2ee7964b468", "Fr8 Fromentum Registration Form",false);
            this.solution = parameters.Item1;
            var plan = parameters.Item2;
            var tokenGuid = parameters.Item3;
            
            //
            // configure Get_Google_Sheet_Data activity
            //
            var googleSheetActivity = this.solution.ChildrenActivities.Single(a=>a.Label.Equals("get google sheet data", StringComparison.InvariantCultureIgnoreCase));
            await googleActivityTestTools.ConfigureGetFromGoogleSheetActivity(googleSheetActivity, spreadsheetName, false, worksheetName);
            
            //
            // configure Loop activity
            //
            var loopActivity = this.solution.ChildrenActivities.Single(a => a.Label.Equals("loop", StringComparison.InvariantCultureIgnoreCase));
            var terminalFr8CoreTools = new IntegrationTestTools_terminalFr8(this);
            loopActivity = await terminalFr8CoreTools.ConfigureLoopActivity(loopActivity, "Standard Table Data", "Table Generated From Google Sheet Data");

            //
            // Configure Send DocuSign Envelope action
            //

            //
            // Initial Configuration
            //
            var sendEnvelopeAction = loopActivity.ChildrenActivities.Single(a => a.Label == "Send DocuSign Envelope");

            crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            var controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            var docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            docuSignTemplate.Value = "a439cedc-92a8-49ad-ab31-e2ee7964b468";
            docuSignTemplate.selectedKey = "Fr8 Fromentum Registration Form";
  
            using (var updatableStorage = Crate.GetUpdatableStorage(sendEnvelopeAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);
            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/configure", sendEnvelopeAction);

            //
            // Follow-up Configuration
            //
            //chosen "Fr8 Fromentum Registration Form" contains 7 specific DocuSign tabs that will be configured with upstream values 
            crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();
            var emailField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingLead role email");
            emailField.ValueSource = "upstream";
            emailField.Value = "emailaddress";
            emailField.selectedKey= "emailaddress";

            var emailNameField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "RolesMappingLead role name");
            emailNameField.ValueSource = "upstream";
            emailNameField.Value = "name";
            emailNameField.selectedKey = "name";

            var phoneField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "MappingPhone(Lead)");
            phoneField.ValueSource = "upstream";
            phoneField.Value = "phone";
            phoneField.selectedKey = "phone";

            var titleField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "MappingTitle(Lead)");
            titleField.ValueSource = "upstream";
            titleField.Value = "title";
            titleField.selectedKey = "title";

            var companyField = controlsCrate.Content.Controls.OfType<TextSource>().First(f => f.Name == "MappingCompany(Lead)");
            companyField.ValueSource = "upstream";
            companyField.Value = "companyname";
            companyField.selectedKey = "companyname";

            var radioGroup = controlsCrate.Content.Controls.OfType<RadioButtonGroup>().First(f=>f.GroupName == "RadioGroupMappingRegistration Type(Lead)");
            foreach (var radios in radioGroup.Radios)
            {
                //reset all preselected radioButtons
                radios.Selected = false;
            }
            var radioButton = radioGroup.Radios.FirstOrDefault(x => x.Name == "Buy 2, Get 3rd Free");
            radioButton.Selected = true;

            var checkboxField = controlsCrate.Content.Controls.OfType<CheckBox>().First(f => f.Name == "CheckBoxMappingGovernmentEntity?(Lead)");
            checkboxField.Selected = true;

            var dropdownField = controlsCrate.Content.Controls.OfType<DropDownList>().First(f => f.Name == "DropDownMappingSize of Company(Lead)");
            dropdownField.Value = "Medium (51-250)";
            dropdownField.selectedKey = "Medium (51-250)";

            using (var updatableStorage = Crate.GetUpdatableStorage(sendEnvelopeAction))
            {
                updatableStorage.Remove<StandardConfigurationControlsCM>();
                updatableStorage.Add(controlsCrate);
            }

            sendEnvelopeAction = await HttpPostAsync<ActivityDTO, ActivityDTO>(_baseUrl + "activities/save", sendEnvelopeAction);

            crateStorage = Crate.FromDto(sendEnvelopeAction.CrateStorage);
            controlsCrate = crateStorage.CratesOfType<StandardConfigurationControlsCM>().First();

            docuSignTemplate = controlsCrate.Content.Controls.OfType<DropDownList>().First();
            Assert.AreEqual("a439cedc-92a8-49ad-ab31-e2ee7964b468", docuSignTemplate.Value, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");
            Assert.AreEqual("Fr8 Fromentum Registration Form", docuSignTemplate.selectedKey, "Selected DocuSign Template did not save on Send DocuSign Envelope action.");

            //
            // Activate and run plan
            //
            var container = await HttpPostAsync<string, ContainerDTO>(_baseUrl + "plans/run?planId=" + plan.Plan.Id, null);
            Assert.AreEqual(container.State, State.Completed, "Container state is not equal to completed on Mail_Merge e2e test");
            
            //
            // Assert 
            //

            var configuration =  new DocuSignManager().SetUp(_terminalDocuSignTestTools.GetDocuSignAuthToken(tokenGuid));
            //find the envelope on the Docusign Account
            var folderItems = DocuSignFolders.GetFolderItems(configuration, new DocusignQuery()
            {       
                Status = "sent",
                SearchText = spreadsheetKeyWord
            });
            
            var envelope = folderItems.FirstOrDefault();
            Assert.IsNotNull(envelope, "Cannot find created Envelope in sent folder of DocuSign Account");
            var envelopeApi = new EnvelopesApi(configuration.Configuration);
            //get the recipient that receive this sent envelope
            var envelopeSigner = envelopeApi.ListRecipients(configuration.AccountId, envelope.EnvelopeId).Signers.FirstOrDefault();
            Assert.IsNotNull(envelopeSigner, "Envelope does not contain signer as recipient. Send_DocuSign_Envelope activity failed to provide any signers");
            //get the tabs for the envelope that this recipient received
            var tabs = envelopeApi.ListTabs(configuration.AccountId, envelope.EnvelopeId, envelopeSigner.RecipientId);
            Assert.IsNotNull(tabs, "Envelope does not contain any tabs. Check for problems in DocuSignManager and HandleTemplateData");
            
            //check all tabs and their values for received envelope, and compare them to those from the google sheet configured into Mail_Merge_Into_Docusign solution 
            var titleRecipientTab = tabs.TextTabs.FirstOrDefault(x => x.TabLabel == "Title");
            Assert.IsNotNull(titleRecipientTab, "Envelope does not contain Title tab. Check for problems in DocuSignManager and HandleTemplateData");
            Assert.AreEqual(tableFixtureData.Table[1].Row.FirstOrDefault(x=>x.Cell.Key == "title").Cell.Value, titleRecipientTab.Value, "Provided value for Title in document for recipient after finishing mail merge plan is incorrect");

            var companyRecipientTab = tabs.TextTabs.FirstOrDefault(x => x.TabLabel == "Company");
            Assert.IsNotNull(companyRecipientTab, "Envelope does not contain Company tab. Check for problems in DocuSignManager and HandleTemplateData");
            Assert.AreEqual(tableFixtureData.Table[1].Row.FirstOrDefault(x => x.Cell.Key == "companyname").Cell.Value, companyRecipientTab.Value, "Provided value for CompanyName in document for recipient after finishing mail merge plan is incorrect");

            var phoneRecipientTab = tabs.TextTabs.FirstOrDefault(x => x.TabLabel == "Phone");
            Assert.IsNotNull(phoneRecipientTab, "Envelope does not contain phone tab. Check for problems in DocuSignManager and HandleTemplateData");
            Assert.AreEqual(tableFixtureData.Table[1].Row.FirstOrDefault(x => x.Cell.Key == "phone").Cell.Value, phoneRecipientTab.Value, "Provided value for phone in document for recipient after finishing mail merge plan is incorrect");

            var listRecipientTab = tabs.ListTabs.FirstOrDefault(x => x.TabLabel == "Size of Company");
            Assert.IsNotNull(listRecipientTab, "Envelope does not contain List Tab for Size of Company tab. Check for problems in DocuSignManager and HandleTemplateData");
            Assert.AreEqual("Medium (51-250)", listRecipientTab.Value, "Provided value for Size of Company(Lead) in document for recipient after finishing mail merge plan is incorrect");

            var checkboxRecipientTab = tabs.CheckboxTabs.FirstOrDefault(x => x.TabLabel == "GovernmentEntity?");
            Assert.IsNotNull(checkboxRecipientTab, "Envelope does not contain Checkbox for Goverment Entity tab. Check for problems in DocuSignManager and HandleTemplateData");
            Assert.AreEqual("true", checkboxRecipientTab.Selected, "Provided value for GovernmentEntity? in document for recipient after finishing mail merge plan is incorrect");
            
            var radioButtonGroupTab = tabs.RadioGroupTabs.FirstOrDefault(x => x.GroupName == "Registration Type");
            Assert.IsNotNull(radioButtonGroupTab, "Envelope does not contain RadioGroup tab for registration. Check for problems in DocuSignManager and HandleTemplateData");
            Assert.AreEqual("Buy 2, Get 3rd Free", radioButtonGroupTab.Radios.FirstOrDefault(x=>x.Selected == "true").Value, "Provided value for Registration Type in document for recipient after finishing mail merge plan is incorrect");

            // Verify that test email has been received
            EmailAssert.EmailReceived("dse_demo@docusign.net", "Test Message from Fr8");

            //
            // Cleanup
            //

            //delete spreadsheet
            await terminalGoogleTestTools.DeleteSpreadSheet(googleAuthTokenId, spreadsheetId);
            
            //
            // Deactivate plan
            //
            await HttpPostAsync<string, string>(_baseUrl + "plans/deactivate?planId=" + plan.Plan.Id, null);

            //
            // Delete plan
            //
            //await HttpDeleteAsync(_baseUrl + "plans?id=" + plan.Plan.Id);
        }

        private async Task<Guid> ExtractGoogleDefaultToken()
        {
            var errorMessage = $"Authorization token for Google is not found for the integration testing user {TestUserEmail}. Please go to the target instance of fr8 and log in with the integration testing user credentials. Then add a Google action to any plan and be sure to set the \"Use for all Activities\" checkbox on the Authorize Accounts dialog while authenticating. Reason: ";

            var tokens = await HttpGetAsync<IEnumerable<ManageAuthToken_Terminal>>(
                _baseUrl + "manageauthtoken/"
            );

            Assert.NotNull(tokens, errorMessage + "No auth-tokens found");

            var terminal = tokens.FirstOrDefault(x => x.Name == "terminalGoogle");
            Assert.NotNull(terminal, errorMessage + "No auth-tokens found for terminalGoogle");

            var token = terminal.AuthTokens.FirstOrDefault(x => x.IsMain);
            Assert.NotNull(token, errorMessage + "No Main auth-token found for terminalGoogle");

            return token.Id;
        }
    }
}
