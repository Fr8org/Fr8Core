using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using HealthMonitorUtility;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [SkipLocal]
    public class MonitorAllDocuSignEvents_Tests : BaseHubIntegrationTest
    {
        private const string EnvelopeToSend = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
<DocuSignEnvelopeInformation xmlns = ""http://www.docusign.net/API/3.0"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
   <EnvelopeStatus>
      <RecipientStatuses>
         <RecipientStatus>
            <Type>CertifiedDelivery</Type>
            <Email>hal9000@discovery.com</Email>
            <UserName>HAL-9000</UserName>
            <RoutingOrder>1</RoutingOrder>
            <Sent>2015-09-29T07:38:22.653</Sent>
            <DeclineReason xsi:nil= ""true"" />
            <Status>Sent</Status>
            <RecipientIPAddress/>
            <CustomFields/>
            <AccountStatus>Active</AccountStatus>
            <RecipientId>279a1173-04cc-4902-8039-68b1992639e9</RecipientId>
         </RecipientStatus>
      </RecipientStatuses>
      <TimeGenerated>2015-09-29T07:38:42.7464809</TimeGenerated>
      <EnvelopeID>{0}</EnvelopeID>
      <Subject>Open the Pod bay doors, HAL</Subject>
      <UserName>Dave Bowman</UserName>
      <Email>freight.testing@gmail.com</Email>
      <Status>Sent</Status>
      <Created>2015-09-29T07:37:42.813</Created>
      <Sent>2015-09-29T07:38:22.7</Sent>
      <ACStatus>Original</ACStatus>
      <ACStatusDate>2015-09-29T07:37:42.813</ACStatusDate>
      <ACHolder>Dave Bowman</ACHolder>
      <ACHolderEmail>freight.testing@gmail.com</ACHolderEmail>
      <ACHolderLocation>DocuSign</ACHolderLocation>
      <SigningLocation>Online</SigningLocation>
      <SenderIPAddress>10.103.101.11</SenderIPAddress>
      <EnvelopePDFHash />
      <CustomFields />
      <AutoNavigation>true</AutoNavigation>
      <EnvelopeIdStamping>true</EnvelopeIdStamping>
      <AuthoritativeCopy>false</AuthoritativeCopy>
      <DocumentStatuses>
         <DocumentStatus>
            <ID>85548272</ID>
            <Name>image.jpg</Name>
            <TemplateName />
            <Sequence>1</Sequence>
         </DocumentStatus>
      </DocumentStatuses>
   </EnvelopeStatus>
</DocuSignEnvelopeInformation>";

        private const int AwaitPeriod = 30000;
        private const string DocuSignEmail = "freight.testing@gmail.com";
        private const string DocuSignApiPassword = "I6HmXEbCxN";

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async void Test_MonitorAllDocuSignEvents_Plan()
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testAccount = unitOfWork.UserRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.UserName == this.TestUserEmail);
                
                var docuSignTerminal = unitOfWork.TerminalRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.Name == TerminalName);
                

                if (testAccount == null)
                {
                    throw new ApplicationException(
                        string.Format("No test account found with UserName = {0}", TestUserEmail)
                    );
                }

                if (docuSignTerminal == null)
                {
                    throw new ApplicationException(
                        string.Format("No terminal found with Name = {0}", TerminalName)
                    );
                }
                
                await RecreateDefaultAuthToken(unitOfWork, testAccount, docuSignTerminal);

                var mtDataCountBefore = unitOfWork.MultiTenantObjectRepository
                    .AsQueryable<DocuSignEnvelopeCM>(testAccount.Id.ToString())
                    .Count();

                //await SendDocuSignTestEnvelope();
                
                await HttpPostAsync<string>(GetTerminalEventsUrl(), new StringContent(string.Format(EnvelopeToSend, Guid.NewGuid())));
                
                var result = await Task.Run(async () =>
                {
                    for (int i = 0; i < AwaitPeriod; i += 1000)
                    {
                        await Task.Delay(1000);

                        int mtDataCountAfter;

                        using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                        {
                            mtDataCountAfter = uow.MultiTenantObjectRepository
                                .AsQueryable<DocuSignEnvelopeCM>(testAccount.Id)
                                .Count();
                        }

                        if (mtDataCountAfter > mtDataCountBefore)
                        {
                            return true;
                        }
                    }

                    return false;
                });
                
                Assert.IsTrue(result);
            }
        }

        private async Task RecreateDefaultAuthToken(IUnitOfWork uow,
            Fr8AccountDO account, TerminalDO docuSignTerminal) 
        {
            var tokens = await HttpGetAsync<IEnumerable<ManageAuthToken_Terminal>>(
                _baseUrl + "manageauthtoken/"
            );

            if (tokens != null)
            {
                var docusignTokens = tokens.FirstOrDefault(x => x.Name == "terminalDocuSign");
                if (docusignTokens != null)
                {
                    var existingToken = docusignTokens.AuthTokens
                        .FirstOrDefault(x => x.ExternalAccountName == TestUserEmail);

                    if (existingToken != null)
                    {
                        await HttpPostAsync<string>(
                            _baseUrl + "manageauthtoken/revoke?id=" + existingToken.Id.ToString(),
                            null
                        );
                    }
                }
            }

            var creds = new CredentialsDTO()
            {
                Username = DocuSignEmail,
                Password = DocuSignApiPassword,
                IsDemoAccount = true,
                TerminalId = docuSignTerminal.Id
            };

            var tokenResponse = await HttpPostAsync<CredentialsDTO, JObject>(
                _baseUrl + "authentication/token",
                creds
            );

            var tokenId = Guid.Parse(tokenResponse["authTokenId"].Value<string>());

            AssignAuthTokens(uow, account, tokenId);
        }

        private void AssignAuthTokens(IUnitOfWork uow, Fr8AccountDO account, Guid tokenId)
        {
            var plan = uow.PlanRepository.GetPlanQueryUncached()
                .SingleOrDefault(x => x.Fr8AccountId == account.Id && x.Name == "MonitorAllDocuSignEvents");
            if (plan == null)
            {
                throw new ApplicationException("Could not find MonitorAllDocuSignEvents plan.");
            }

            var queue = new Queue<RouteNodeDO>();
            queue.Enqueue(plan);

            while (queue.Count > 0)
            {
                var routeNode = queue.Dequeue();

                var activity = routeNode as ActivityDO;
                if (activity != null)
                {
                    if (activity.ActivityTemplate.Terminal.Name == TerminalName
                        && !activity.AuthorizationTokenId.HasValue)
                    {
                        activity.AuthorizationTokenId = tokenId;
                    }
                }

                uow.PlanRepository.GetNodesQueryUncached()
                    .Where(x => x.ParentRouteNodeId == routeNode.Id)
                    .ToList()
                    .ForEach(x => queue.Enqueue(x));
            }

            uow.SaveChanges();
        }

        /*private async Task SendDocuSignTestEnvelope()
        {
            var endpoint = CloudConfigurationManager.GetSetting("endpoint");

            var authManager = new DocuSignAuthentication();
            var password = await authManager
                .ObtainOAuthToken(DocuSignEmail, DocuSignApiPassword, endpoint);

            var templateManager = new DocuSignTemplate();
            var template = templateManager
                .GetTemplateNames(
                    DocuSignEmail,
                    password
                )
                .FirstOrDefault(x => x.Name == TemplateName);

            if (template == null)
            {
                throw new ApplicationException(string.Format("Unable to extract {0} template from DocuSign", TemplateName));
            }

            var loginInfo = DocuSignService.Login(
                DocuSignEmail,
                password
            );

            var rolesList = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Tags = "recipientId:72179268",
                    Key = "role name",
                    Value = ToEmail
                },
                new FieldDTO()
                {
                    Tags = "recipientId:72179268",
                    Key = "role email",
                    Value = ToEmail
                }
            };

            var fieldsList = new List<FieldDTO>();
            
            DocuSignService.SendAnEnvelopeFromTemplate(
                loginInfo,
                rolesList,
                fieldsList,
                template.Id
            );
        }*/
    }
}
