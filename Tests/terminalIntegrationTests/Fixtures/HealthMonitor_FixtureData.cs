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
using System.Configuration;

namespace terminalIntegrationTests.Fixtures
{
    public class HealthMonitor_FixtureData
    {
        public static PlanNoChildrenDTO TestPlanNoChildrenDTO()
        {
            return new PlanNoChildrenDTO()
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
                PlanState = PlanState.Executing,
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
            };
            return SubPlanDO;
        }


        public static AuthorizationTokenDTO Google_AuthToken()
        {
            return new AuthorizationTokenDTO
            {
                Token = ConfigurationManager.AppSettings["GoogleTestAccountToken"]
        };
        }

        public static async Task<AuthorizationTokenDTO> Salesforce_AuthToken()
        {
            var auth = new AuthenticationClient();
            await auth.UsernamePasswordAsync(
                       ConfigurationManager.AppSettings["OwnerClientId"],
                       ConfigurationManager.AppSettings["OwnerId"],
                       ConfigurationManager.AppSettings["OwnerEmail"],
                       ConfigurationManager.AppSettings["OwnerPassword"]);

            return new AuthorizationTokenDTO()
            {
                Token = JsonConvert.SerializeObject(new { AccessToken = auth.AccessToken }),
                AdditionalAttributes = string.Format("instance_url={0};api_version={1}", auth.InstanceUrl, auth.ApiVersion)
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
