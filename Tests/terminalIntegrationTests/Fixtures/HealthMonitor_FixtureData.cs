using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using StructureMap;
using Salesforce.Common;
using Fr8.Testing.Integration;

namespace terminalIntegrationTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static PlanEmptyDTO TestPlanEmptyDTO()
        {
            return new PlanEmptyDTO()
            {
                Name = "Integratin Test Plan",
                Description = "Create a new Integration test Plan and configure with custom activities"
            };
        }

        public static PlanDO TestPlan_CanCreate(string name)
        {
            var curPlanDO = new PlanDO
            {
                Id = Guid.NewGuid(),
                Description = name,
                Name = name,
                PlanState = PlanState.Running,
            };
            return curPlanDO;
        }

        public static SubplanDO TestSubPlanHealthDemo(Guid parentNodeId)
        {
            var SubPlanDO = new SubplanDO
            {
                Id = Guid.NewGuid(),
                ParentPlanNodeId = parentNodeId,
                RootPlanNodeId = parentNodeId,
                NodeTransitions = "[{'TransitionKey':'true','ProcessNodeId':'2'}]"
            };
            return SubPlanDO;
        }


        public static AuthorizationTokenDTO Google_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token =
                    @"{""AccessToken"":""ya29.qgKKAej9ABzUTVL9y04nEtlo0_Qlpk_dqIBLmg1k7tBo__Dgab0TWvSf-ZgjrjRmUA"",""RefreshToken"":""1/x3T7UajSlqgYQa2BeBozc_49Sa29zCqe-EEvi5eBfFF90RDknAdJa_sgfheVM0XT"",""Expires"":""2017-03-19T13:24:33.2805735+01:00""}"
            };
        }

        public static async Task<AuthorizationTokenDTO> Salesforce_AuthToken()
        {
            var auth = new AuthenticationClient();
            await auth.UsernamePasswordAsync(
                "3MVG9KI2HHAq33RzZO3sQ8KU8JPwmpiZBpe_fka3XktlR5qbCWstH3vbAG.kLmaldx8L1V9OhqoAYUedWAO_e",
                "611998545425677937",
                "alex@dockyard.company",
                "thales@123");

            return new AuthorizationTokenDTO()
            {
                Token = auth.AccessToken,
                AdditionalAttributes = string.Format("refresh_token=;instance_url={0};api_version={1}", auth.InstanceUrl, auth.ApiVersion)
            };
        }

        public static async Task<AuthorizationTokenDO> CreateSalesforceAuthToken()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var emailAddressId = uow.EmailAddressRepository.FindOne(e => e.Address.Equals("integration_test_runner@fr8.company")).Id;
                var userDO = uow.UserRepository.FindOne(u => u.EmailAddressID == emailAddressId);
                var terminalId = uow.TerminalRepository.FindOne(t => t.Name.Equals("terminalSalesforce")).Id;

                var tokenDTO = await Salesforce_AuthToken();

                var tokenDO = new AuthorizationTokenDO()
                {
                    Token = tokenDTO.Token,
                    TerminalID = terminalId,
                    UserID = userDO.Id,
                    AdditionalAttributes = tokenDTO.AdditionalAttributes,
                    ExpiresAt = DateTime.Today.AddMonths(1)
                };

                uow.AuthorizationTokenRepository.Add(tokenDO);
                uow.SaveChanges();

                return tokenDO;
            }
        }
        public static async Task<AuthorizationTokenDTO> DocuSign_AuthToken(BaseIntegrationTest integrationTest)
        {
            var creds = new CredentialsDTO()
            {
                Username = "freight.testing@gmail.com",
                Password = "I6HmXEbCxN",
                IsDemoAccount = true
            };

            string endpoint = integrationTest.GetTerminalUrl() + "/authentication/token";
            var jobject = await integrationTest.HttpPostAsync<CredentialsDTO, JObject>(endpoint, creds);
            var result = JsonConvert.DeserializeObject<AuthorizationTokenDTO>(jobject.ToString());
            Assert.IsNullOrEmpty(result.Error, "Failed to aquire DocuSign auth token");
            return result;
        }
    }
}
