using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using terminalIntegrationTests.Fixtures;
using terminalSalesforce.Services;
using Data.Entities;
using terminalSalesforce.Infrastructure;
using Fr8.Testing.Integration;
using terminalSalesforce.Actions;
using Fr8.Testing.Integration.Tools.Terminals;
using Data.States;
using terminalDocuSign.Services.New_Api;
using terminalDocuSign.Services;
using DocuSign.eSign.Api;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Helpers;
using Fr8.TerminalBase.Models;
using StructureMap;
using terminalDocuSign.Actions;
using Fr8.TerminalBase.Interfaces;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class MailMergeFromSalesforceTests : BaseHubIntegrationTest
    {
        private readonly IntegrationTestTools_terminalDocuSign _docuSignTestTools;
        private readonly IContainer _container;

        public MailMergeFromSalesforceTests()
        {
            _docuSignTestTools = new IntegrationTestTools_terminalDocuSign(this);
            Mapper.CreateMap<AuthorizationTokenDO, AuthorizationToken>();

            _container = ObjectFactory.Container.CreateChildContainer();
            _container.Configure(MockedHubCommunicatorConfiguration);
        }

        public static void MockedHubCommunicatorConfiguration(ConfigurationExpression configuration)
        {
            configuration.AddRegistry<MockedHubCommunicatorRegistry>();
        }

        public override string TerminalName
        {
            get
            {
                return "terminalSalesforce";
            }
        }
        
        [Test, Category("Integration.terminalSalesforce")]
        public async Task MailMergeFromSalesforceEndToEnd()
        {
          
            await RevokeTokens("terminalDocuSign");
            var salesforceAuthToken = await HealthMonitor_FixtureData.CreateSalesforceAuthToken();
            //Create Case object in Salesforce
            var caseIdAndName = await CreateCase(salesforceAuthToken);
            PlanDTO plan = null;
            try
            {
                plan = await CreatePlan();
                var solution = plan.SubPlans.First().Activities.Single();
                await ApplyAuthTokenToSolution(solution, salesforceAuthToken);
                //Initial configuration
                solution = await Configure(solution);
                //Folowup configuration
                solution = solution.UpdateControls<Mail_Merge_From_Salesforce_v1.ActivityUi>(x =>
                {
                    x.SalesforceObjectSelector.selectedKey = "Case";
                    x.SalesforceObjectSelector.Value = "Case";
                });
                //This call will make solution to load specified Salesforce object properties and clear filter
                solution = await Configure(solution);
                //This call will run generation of child activities
                solution = solution.UpdateControls<Mail_Merge_From_Salesforce_v1.ActivityUi>(x =>
                {
                    x.SalesforceObjectFilter.Value = $"[{{\"field\":\"SuppliedName\",\"operator\":\"eq\",\"value\":\"{caseIdAndName.Item2}\"}}]";
                    var sendDocuSignItem = x.MailSenderActivitySelector.ListItems.FirstOrDefault(y => y.Key == "Send DocuSign Envelope");
                    Assert.IsNotNull(sendDocuSignItem, $"Send DocuSign Envelope activity is not marked with '{Tags.EmailDeliverer}' tag");
                    x.MailSenderActivitySelector.selectedKey = sendDocuSignItem.Key;
                    x.MailSenderActivitySelector.Value = sendDocuSignItem.Value;
                    x.RunMailMergeButton.Clicked = true;
                });
                solution = await Configure(solution);
                Assert.AreEqual(2, solution.ChildrenActivities.Length, "Child activities were not generated after mail merge was requested");
                //Configure Send DocuSign Envelope activity to use proper upstream values
                var docuSignActivity = solution.ChildrenActivities[1].ChildrenActivities[0];
                var docusSignAuthAndConfig = await AuthorizeAndConfigureDocuSignActivity(docuSignActivity);
                docuSignActivity = docusSignAuthAndConfig.Item1;
                //Run plan
                var container = await Run(plan);
                Assert.AreEqual(State.Completed, container.State, "Container state is not equal to Completed");
                // Deactivate plan
                await Deactivate(plan);
                //Verify contents of envelope
                AssertEnvelopeContents(docusSignAuthAndConfig.Item2, caseIdAndName.Item2);
                // Verify that test email has been received
                EmailAssert.EmailReceived("dse_demo@docusign.net", "Test Message from Fr8");
            }
            finally
            {
                var caseWasDeleted = await DeleteCase(caseIdAndName.Item1, salesforceAuthToken);
                Assert.IsTrue(caseWasDeleted, "Case created for test purposes failed to be deleted");
                //if (plan != null)
                //{
                //    await HttpDeleteAsync($"{_baseUrl}plans?id={plan.Id}");
                //}
            }
            
        }

        private void AssertEnvelopeContents(Guid docuSignTokenId, string expectedName)
        {
            var token = Mapper.Map<AuthorizationToken>(_docuSignTestTools.GetDocuSignAuthToken(docuSignTokenId));
            var configuration = new DocuSignManager().SetUp(token);
            //find the envelope on the Docusign Account
            var folderItems = DocuSignFolders.GetFolderItems(configuration, new DocuSignQuery()
            {
                Status = "sent",
                SearchText = expectedName
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
        }

        private async Task<Tuple<ActivityDTO, Guid>> AuthorizeAndConfigureDocuSignActivity(ActivityDTO docuSignActivity)
        {
            var crateStorage = Crate.GetStorage(docuSignActivity);
            var authenticationRequired = crateStorage.CratesOfType<StandardAuthenticationCM>().Any();
            var tokenId = Guid.Empty;
            if (authenticationRequired)
            {
                var terminalSummaryDTO = new TerminalSummaryDTO
                {
                    Name = docuSignActivity.ActivityTemplate.TerminalName,
                    Version = docuSignActivity.ActivityTemplate.TerminalVersion
                };
                // Authenticate with DocuSign
                tokenId = await _docuSignTestTools.AuthenticateDocuSignAndAssociateTokenWithAction(docuSignActivity.Id, GetDocuSignCredentials(), terminalSummaryDTO);
                docuSignActivity = await Configure(docuSignActivity);
            }
            docuSignActivity.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x => x.TemplateSelector.SelectByKey("SendEnvelopeTestTemplate"));
            //This configuration call will generate text source fields for selected template properties
            docuSignActivity = await Configure(docuSignActivity);
            docuSignActivity.UpdateControls<Send_DocuSign_Envelope_v2.ActivityUi>(x =>
            {
                                                                                      var roleEmailControl = x.RolesFields.First(y => y.Name == "TestSigner role email");
                                                                                      roleEmailControl.ValueSource = TextSource.UpstreamValueSrouce;
                                                                                      roleEmailControl.selectedKey = "SuppliedEmail";
                                                                                      roleEmailControl.Value = "SuppliedEmail";

                                                                                      var roleNameControl = x.RolesFields.First(y => y.Name == "TestSigner role name");
                                                                                      roleNameControl.ValueSource = TextSource.UpstreamValueSrouce;
                                                                                      roleNameControl.selectedKey = "SuppliedName";
                                                                                      roleNameControl.Value = "SuppliedName";
                                                                                  });
            return new Tuple<ActivityDTO, Guid>(await Save(docuSignActivity), tokenId);
        }

        private async Task<ActivityDTO> Save(ActivityDTO activity)
        {
            return await HttpPostAsync<ActivityDTO, ActivityDTO>($"{_baseUrl}activities/save", activity);
        }

        private async Task<ActivityDTO> Configure(ActivityDTO activity)
        {
            return await HttpPostAsync<ActivityDTO, ActivityDTO>($"{_baseUrl}activities/configure?id={activity.Id}", activity);
        }

        private async Task<ContainerDTO> Run(PlanDTO plan)
        {
            return await HttpPostAsync<string, ContainerDTO>($"{_baseUrl}plans/run?planId={plan.Id}", null);
        }

        private async Task<string> Deactivate(PlanDTO plan)
        {
            return await HttpPostAsync<string, string>($"{_baseUrl}plans/deactivate?planId={plan.Id}", null);
        }

        private async Task ApplyAuthTokenToSolution(ActivityDTO solution, AuthorizationTokenDO salesforceAuthToken)
        {
            var applyToken = new AuthenticationTokenGrantDTO()
            {
                ActivityId = solution.Id,
                AuthTokenId = salesforceAuthToken.Id,
                IsMain = true
            };
            await HttpPostAsync<AuthenticationTokenGrantDTO[], string>(GetHubApiBaseUrl() + "authentication/tokens/grant", new[] { applyToken });
        }

        private async Task<PlanDTO> CreatePlan()
        {
            var solutionCreateUrl = GetHubApiBaseUrl() + "plans?solutionName=Mail_Merge_From_Salesforce";
            return await HttpPostAsync<string, PlanDTO>(solutionCreateUrl, null);
        }

        private async Task<bool> DeleteCase(string caseId, AuthorizationTokenDO authToken)
        {
            var token = Mapper.Map<AuthorizationToken>(authToken);
            return await _container.GetInstance<SalesforceManager>().Delete(SalesforceObjectType.Case, caseId, token);
        }

        private async Task<Tuple<string, string>> CreateCase(AuthorizationTokenDO authToken)
        {
            var token = Mapper.Map<AuthorizationToken>(authToken);
            var manager = _container.GetInstance<SalesforceManager>();
            var name = Guid.NewGuid().ToString();
            var data = new Dictionary<string, object> { { "SuppliedEmail", TestEmail }, { "SuppliedName", name } };
            return new Tuple<string, string>(await manager.Create(SalesforceObjectType.Case, data, token), name);
        }

        public class MockedHubCommunicatorRegistry : StructureMap.Configuration.DSL.Registry
        {
            public MockedHubCommunicatorRegistry()
            {
                For<IHubCommunicator>().Use(new Moq.Mock<IHubCommunicator>(Moq.MockBehavior.Default).Object);
            }
        }
    }
}
