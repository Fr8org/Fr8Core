using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using HealthMonitor.Utility;
using System.Diagnostics;
using System.Net.Http;
using AutoMapper;
using Data.Entities;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Newtonsoft.Json.Linq;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class MonitorAllDocuSignEventsLocal_Tests : BaseHubIntegrationTest
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
      <Email>fr8.madse.testing@gmail.com</Email>
      <Status>Sent</Status>
      <Created>2015-09-29T07:37:42.813</Created>
      <Sent>2015-09-29T07:38:22.7</Sent>
      <ACStatus>Original</ACStatus>
      <ACStatusDate>2015-09-29T07:37:42.813</ACStatusDate>
      <ACHolder>Dave Bowman</ACHolder>
      <ACHolderEmail>fr8.madse.testing@gmail.com</ACHolderEmail>
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


        private const int MaxAwaitPeriod = 400000;
        private const int SingleAwaitPeriod = 50000;
        private const string DocuSignEmail = "fr8.madse.testing@gmail.com"; // "freight.testing@gmail.com";
        private const string DocuSignApiPassword = "I6HmXEbCxN";

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async void Test_MonitorAllDocuSignEventsLocal_Plan()
        {
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var testAccount = unitOfWork.UserRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.UserName == TestUserEmail);

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
                    .AsQueryable<DocuSignEnvelopeCM_v2>(testAccount.Id)
                    .Count();


                //let's wait 10 seconds to ensure that MADSE plan was created/activated by re-authentication
                await Task.Delay(SingleAwaitPeriod);

                string response = 
                    await HttpPostAsync<string>(GetTerminalEventsUrl(), new StringContent(string.Format(EnvelopeToSend, Guid.NewGuid())));

                Debug.WriteLine($"Received {GetTerminalEventsUrl()} response {response}");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                int mtDataCountAfter = mtDataCountBefore;
                while (stopwatch.ElapsedMilliseconds <= MaxAwaitPeriod)
                {
                    await Task.Delay(SingleAwaitPeriod);

                    mtDataCountAfter = unitOfWork.MultiTenantObjectRepository
                        .AsQueryable<DocuSignEnvelopeCM_v2>(testAccount.Id.ToString()).Count();

                    if (mtDataCountBefore < mtDataCountAfter)
                    {
                        break;
                    }
                }

                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter,
                    $"The number of Local MtData ({mtDataCountAfter}) records for user {TestUserEmail} remained unchanged within {MaxAwaitPeriod} miliseconds.");
            }

        }

        private async Task RecreateDefaultAuthToken(IUnitOfWork uow,
           Fr8AccountDO account, TerminalDO docuSignTerminal)
        {
            Debug.WriteLine($"Reauthorizing tokens for {account.EmailAddress.Address}");
            var tokens = await HttpGetAsync<IEnumerable<ManageAuthToken_Terminal>>(
                _baseUrl + "manageauthtoken/"
            );

            var docusignTokens = tokens?.FirstOrDefault(x => x.Name == "terminalDocuSign");

            if (docusignTokens != null)
            {
                foreach (var token in docusignTokens.AuthTokens)
                {
                    await HttpPostAsync<string>(
                        _baseUrl + "manageauthtoken/revoke?id=" + token.Id,
                        null
                        );
                }
            }

            var creds = new CredentialsDTO()
            {
                Username = DocuSignEmail,
                Password = DocuSignApiPassword,
                IsDemoAccount = true,
                Terminal = Mapper.Map<TerminalDTO>(docuSignTerminal)
            };

            var tokenResponse = await HttpPostAsync<CredentialsDTO, JObject>(
                _baseUrl + "authentication/token",
                creds
            );

            Assert.NotNull(
                tokenResponse["authTokenId"],
                "AuthTokenId is missing in API response."
            );
        }
    }
}
