using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Testing.Integration;
using terminalDocuSign.Services.New_Api;
using Newtonsoft.Json;
using System.Diagnostics;
using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Models;
using System.Configuration;
using Data.Repositories.MultiTenant.Queryable;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [SkipLocal]
    public class MonitorAllDocuSignEvents_Tests : BaseHubIntegrationTest
    {
        // private const string UserAccountName = "y.gnusin@gmail.com";
        private string UserAccountName
        {
            get
            {
                return ConfigurationManager.AppSettings["TestUserAccountName"];
            }
        }
        private string ToEmail // "freight.testing@gmail.com";
        {
            get
            {
                return ConfigurationManager.AppSettings["MADSETestEmail"];
            }
        }

        private string DocuSignEmail // "freight.testing@gmail.com";
        {
            get
            {
                return ConfigurationManager.AppSettings["MADSETestEmail"];
            }
        }

        private string DocuSignApiPassword
        {
            get
            {
                return ConfigurationManager.AppSettings["DocuSignApiPassword"];
            }
        }

        private const int MaxAwaitPeriod = 300000;
        private const int SingleAwaitPeriod = 10000;
        private const int MadseCreationPeriod = 30000;

        private const string templateId = "b0c8eb61-ff16-410d-be0b-6a2feec57f4c"; // "392f63c3-cabb-4b21-b331-52dabf1c2993"; // "SendEnvelopeIntegrationTest" template

        private string ConnectName = "madse-connect";
        private string publishUrl;


        protected override string TestUserEmail
        {
            get { return UserAccountName; }
        }

        /*protected override string TestUserPassword
        {
            get { return ConfigurationManager.AppSettings["DefaultUserPassword"]; }
        }*/


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
                    .SingleOrDefault(x => x.UserName == UserAccountName);

                var docuSignTerminal = unitOfWork.TerminalRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.Name == TerminalName);

                if (testAccount == null)
                {
                    throw new ApplicationException(
                        string.Format("No test account found with UserName = {0}", UserAccountName)
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

                //Set up DS
                var token = await Authenticate();
                var authToken = new AuthorizationToken() { Token = token.Token };
                var authTokenDO = new AuthorizationTokenDO() { Token = token.Token };
                var docuSignManager = new DocuSignManager();
                var loginInfo = docuSignManager.SetUp(authToken);

                //let's wait 10 seconds to ensure that MADSE plan was created/activated by re-authentication
                await Task.Delay(MadseCreationPeriod);

                //send envelope
                SendDocuSignTestEnvelope(docuSignManager, loginInfo, authTokenDO);

                var stopwatch = new Stopwatch();
                stopwatch.Start();


                while (stopwatch.ElapsedMilliseconds <= MaxAwaitPeriod)
                {
                    await Task.Delay(SingleAwaitPeriod);

                    mtDataCountAfter = unitOfWork.MultiTenantObjectRepository
                                                 .AsQueryable<DocuSignEnvelopeCM_v2>(testAccount.Id).MtCount();

                    if (mtDataCountBefore < mtDataCountAfter)
                    {
                        break;
                    }
                }

                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter,
                    $"The number of MtData: ({mtDataCountAfter}) records for user {UserAccountName} remained unchanged within {MaxAwaitPeriod} miliseconds.");
            }
        }

        private async Task RecreateDefaultAuthToken(IUnitOfWork uow,
            Fr8AccountDO account, TerminalDO docuSignTerminal)
        {
            Console.WriteLine($"Reauthorizing tokens for {account.EmailAddress.Address}");
            var tokens = await HttpGetAsync<IEnumerable<AuthenticationTokenTerminalDTO>>(
                _baseUrl + "authentication/tokens"
            );

            if (tokens != null)
            {
                var docusignTokens = tokens.FirstOrDefault(x => x.Name == "terminalDocuSign");
                if (docusignTokens != null)
                {
                    foreach (var token in docusignTokens.AuthTokens)
                    {
                        await HttpPostAsync<string>(
                            _baseUrl + "authentication/tokens/revoke?id=" + token.Id.ToString(),
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
                Terminal = Mapper.Map<TerminalSummaryDTO>(docuSignTerminal)
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

        private async Task<AuthorizationTokenDTO> Authenticate()
        {
            var creds = new CredentialsDTO()
            {
                Username = DocuSignEmail,
                Password = DocuSignApiPassword,
                IsDemoAccount = true
            };

            string endpoint = GetTerminalUrl() + "/authentication/token";
            var jobject = await HttpPostAsync<CredentialsDTO, JObject>(endpoint, creds);
            var docuSignToken = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(jobject.ToString());
            Assert.IsTrue(
                string.IsNullOrEmpty(docuSignToken.Error),
                $"terminalDocuSign call to /authentication/token has failed with following error: {docuSignToken.Error}"
            );

            return docuSignToken;
        }

        private void SendDocuSignTestEnvelope(DocuSignManager docuSignManager, DocuSignApiConfiguration loginInfo, AuthorizationTokenDO authTokenDO)
        {
            var rolesList = new List<KeyValueDTO>()
            {
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key = "role name",
                    Value = ToEmail
                },
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key = "role email",
                    Value = ToEmail
                }
            };

            var fieldsList = new List<KeyValueDTO>()
            {
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key="companyTabs",
                    Value="test"
                },
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key="textTabs",
                    Value="test"
                },
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key="noteTabs",
                    Value="test"
                },
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key="checkboxTabs",
                    Value="Radio 1"
                },
                new KeyValueDTO()
                {
                    Tags = "recipientId:1",
                    Key="listTabs",
                    Value="1"
                }
            };

            docuSignManager.SendAnEnvelopeFromTemplate(
                loginInfo,
                rolesList,
                fieldsList,
                templateId
            );
        }
    }
}
