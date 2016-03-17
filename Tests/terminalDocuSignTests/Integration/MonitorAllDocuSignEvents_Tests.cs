using System;
using System.Collections.Generic;
using System.Linq;
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
using Hub.Services;
using terminalDocuSign.Services;
using terminalDocuSign.Services.New_Api;
using Utilities.Configuration.Azure;
using terminalDocuSignTests.Fixtures;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    [SkipLocal]
    public class MonitorAllDocuSignEvents_Tests : BaseHubIntegrationTest
    {
        // private const string UserAccountName = "y.gnusin@gmail.com";
        private const string UserAccountName = "IntegrationTestUser1";
        private const int AwaitPeriod = 120000;

        private const string templateId = "392f63c3-cabb-4b21-b331-52dabf1c2993"; // "SendEnvelopeIntegrationTest" template

        private const string ToEmail = "freight.testing@gmail.com";
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
                    .AsQueryable<DocuSignEnvelopeCM>(testAccount.Id.ToString())
                    .Count();

                await SendDocuSignTestEnvelope();

                await Task.Delay(AwaitPeriod);

                var mtDataCountAfter = unitOfWork.MultiTenantObjectRepository
                    .AsQueryable<DocuSignEnvelopeCM>(testAccount.Id.ToString())
                    .Count();

                Assert.IsTrue(mtDataCountBefore < mtDataCountAfter);
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
                        .FirstOrDefault(x => x.ExternalAccountName == DocuSignEmail);

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

            Assert.NotNull(
                tokenResponse["authTokenId"],
                "AuthTokenId is missing in API response."
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

            var queue = new Queue<PlanNodeDO>();
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
                    .Where(x => x.ParentPlanNodeId == routeNode.Id)
                    .ToList()
                    .ForEach(x => queue.Enqueue(x));
            }

            uow.SaveChanges();
        }

        private async Task SendDocuSignTestEnvelope()
        {
            //var endpoint = CloudConfigurationManager.GetSetting("endpoint");

            //var authManager = new DocuSignAuthentication();
            //var password = await authManager
            //    .ObtainOAuthToken(DocuSignEmail, DocuSignApiPassword, endpoint);

            var authToken = HealthMonitor_FixtureData.DocuSign_AuthToken(this);
            var authTokenDO = new AuthorizationTokenDO() { Token = authToken.Token };
            var docuSignManager = new DocuSignManager();

            var loginInfo = docuSignManager.SetUp(authTokenDO);

            var password = JsonConvert.DeserializeObject<DocuSignAuthTokenDTO>(authTokenDO.Token).ApiPassword;



            var rolesList = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Tags = "recipientId:1",
                    Key = "role name",
                    Value = ToEmail
                },
                new FieldDTO()
                {
                    Tags = "recipientId:1",
                    Key = "role email",
                    Value = ToEmail
                }
            };

            var fieldsList = new List<FieldDTO>()
            {
                new FieldDTO()
                {
                    Tags = "recipientId:1",
                    Key="companyTabs",
                    Value="test"
                },
                new FieldDTO()
                {
                    Tags = "recipientId:1",
                    Key="textTabs",
                    Value="test"
                },
                new FieldDTO()
                {
                    Tags = "recipientId:1",
                    Key="noteTabs",
                    Value="test"
                },
                new FieldDTO()
                {
                    Tags = "recipientId:1",
                    Key="checkboxTabs",
                    Value="Radio 1"
                },
                new FieldDTO()
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
