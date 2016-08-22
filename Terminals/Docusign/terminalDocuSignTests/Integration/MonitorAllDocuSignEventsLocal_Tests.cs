using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using StructureMap;
using Data.Interfaces;
using Fr8.Testing.Integration;
using System.Diagnostics;
using System.Net.Http;
using AutoMapper;
using Data.Entities;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Newtonsoft.Json.Linq;
using Data.Repositories.MultiTenant.Queryable;
using System.Configuration;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class MonitorAllDocuSignEventsLocal_Tests : BaseHubIntegrationTest
    {
        private string RecipientId
        {
            get
            {
                return ConfigurationManager.AppSettings["RecipientId"];
            }
        }
        private string DocuSignEmail // "freight.testing@gmail.com";
        {
            get
            {
                return ConfigurationManager.AppSettings["MADSETestEmail"];
            }
        }
        private string DocuSignApiPassword // "freight.testing@gmail.com";
        {
            get
            {
                return ConfigurationManager.AppSettings["DocuSignApiPassword"];
            }
        }

        private string EnvelopeToSend
        {
            get
            {
                return @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
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
                                     <RecipientId>" + RecipientId + @"</RecipientId>
                                  </RecipientStatus>
                               </RecipientStatuses>
                               <TimeGenerated>2015-09-29T07:38:42.7464809</TimeGenerated>
                               <EnvelopeID>{0}</EnvelopeID>
                               <Subject>Open the Pod bay doors, HAL</Subject>
                               <UserName>Dave Bowman</UserName>
                               <Email>"+DocuSignEmail+ @"</Email>
                               <Status>Sent</Status>
                               <Created>2015-09-29T07:37:42.813</Created>
                               <Sent>2015-09-29T07:38:22.7</Sent>
                               <ACStatus>Original</ACStatus>
                               <ACStatusDate>2015-09-29T07:37:42.813</ACStatusDate>
                               <ACHolder>Dave Bowman</ACHolder>
                               <ACHolderEmail>" + DocuSignEmail + @"</ACHolderEmail>
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
            }
        }


        private const int MaxAwaitPeriod = 300000;
        private const int SingleAwaitPeriod = 10000;
        private const int MadseCreationPeriod = 30000;

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
                    .AsQueryable<DocuSignEnvelopeCM_v2>(testAccount.Id).MtCount();

                int mtDataCountAfter = mtDataCountBefore;


                Debug.WriteLine("Waiting for MADSE plan to be created");
                //let's wait 10 seconds to ensure that MADSE plan was created/activated by re-authentication
                await Task.Delay(MadseCreationPeriod);


                Debug.WriteLine("Sending test event");
                string response = 
                    await HttpPostAsync<string>(GetTerminalEventsUrl(), new StringContent(string.Format(EnvelopeToSend, Guid.NewGuid())));

                Debug.WriteLine($"Received {GetTerminalEventsUrl()} response {response}");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                
                while (stopwatch.ElapsedMilliseconds <= MaxAwaitPeriod)
                {
                    await Task.Delay(SingleAwaitPeriod);

                    Debug.WriteLine($"Querying MT objects...");

                    mtDataCountAfter = unitOfWork.MultiTenantObjectRepository
                        .AsQueryable<DocuSignEnvelopeCM_v2>(testAccount.Id).MtCount();

                    if (mtDataCountBefore < mtDataCountAfter)
                    {
                        break;
                    }

                    Debug.WriteLine($"Number of objects stays unchanged: {mtDataCountBefore}");
                }

                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter,
                    $"The number of Local MtData ({mtDataCountAfter}) records for user {TestUserEmail} remained unchanged within {MaxAwaitPeriod} miliseconds.");
            }

        }

        private async Task RecreateDefaultAuthToken(IUnitOfWork uow,
           Fr8AccountDO account, TerminalDO docuSignTerminal)
        {
            Debug.WriteLine($"Reauthorizing tokens for {account.EmailAddress.Address}");
            var tokens = await HttpGetAsync<IEnumerable<AuthenticationTokenTerminalDTO>>(
                _baseUrl + "authentication/tokens"
            );

            var docusignTokens = tokens?.FirstOrDefault(x => x.Name == "terminalDocuSign");

            if (docusignTokens != null)
            {
                foreach (var token in docusignTokens.AuthTokens)
                {
                    await HttpPostAsync<string>(
                        _baseUrl + "authentication/tokens/revoke?id=" + token.Id,
                        null
                        );
                }
            }

            var creds = new CredentialsDTO()
            {
                Username = DocuSignEmail,
                Password = DocuSignApiPassword,
                IsDemoAccount = true,
                Terminal = Mapper.Map<TerminalSummaryDTO>(docuSignTerminal)
            };

            var tokenResponse = await HttpPostAsync<CredentialsDTO, JObject>(
                _baseUrl + "authentication/token",
                creds
            );

            Debug.WriteLine("Received new tokens.");

            Assert.NotNull(
                tokenResponse["authTokenId"],
                "AuthTokenId is missing in API response."
            );
        }
    }
}
