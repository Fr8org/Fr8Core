using Data.Entities;
using Data.Interfaces;
using Salesforce.Common;
using StructureMap;
using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Moq;
using Newtonsoft.Json;

namespace terminalSalesforceTests.Fixtures
{
    public static class HealthMonitor_FixtureData
    {
        private static readonly CrateManager CrateManager = new CrateManager();
        public static async Task<AuthorizationTokenDTO> Salesforce_AuthToken()
        {
            var auth = new AuthenticationClient();

            return new AuthorizationTokenDTO()
            {
                Token = JsonConvert.SerializeObject(new { AccessToken = auth.AccessToken }),
                AdditionalAttributes = string.Format("instance_url={0};api_version={1}", auth.InstanceUrl, auth.ApiVersion)
            };                                                                                                                            
        }
        public static void ConfigureHubToReturnEmptyPayload()
        {
            var result = new PayloadDTO(Guid.Empty);
            using (var storage = CrateManager.GetUpdatableStorage(result))
            {
                storage.Add(Crate.FromContent(string.Empty, new OperationalStateCM()));
            }
            ObjectFactory.Container.GetInstance<Mock<IHubCommunicator>>().Setup(x => x.GetPayload(It.IsAny<Guid>()))
                               .Returns(Task.FromResult(result));
        }

        public static async Task<AuthorizationTokenDO> CreateSalesforceAuthToken()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var emailAddressId = uow.EmailAddressRepository.FindOne(e => e.Address.Equals("integration_test_runner@fr8.company")).Id;
                var userDO = uow.UserRepository.FindOne(u => u.EmailAddressID == emailAddressId);
                var terminalId = uow.TerminalRepository.FindOne(t => t.Name.Equals("terminalSalesforce")).Id;

                var tokenDTO = await terminalIntegrationTests.Fixtures.HealthMonitor_FixtureData.Salesforce_AuthToken();

                var tokenDO = new AuthorizationTokenDO()
                {
                    ExternalAccountId = tokenDTO.ExternalAccountId,
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

        public static ActivityTemplateSummaryDTO Get_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Version = "1",
                Name = "Get_Data_TEST"
            };
        }

        public static ActivityTemplateSummaryDTO Post_To_Chatter_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Version = "1",
                Name = "Post_To_Chatter_TEST"
            };
        }

        public static Fr8DataDTO Get_Data_v1_InitialConfiguration_ActivityDTO()
        {
            var activityTemplate = Get_Data_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Data from Salesforce.com",
                AuthToken = terminalIntegrationTests.Fixtures.HealthMonitor_FixtureData.Salesforce_AuthToken().Result,
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Post_To_Chatter_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Post_To_Chatter_v1_ActivityTemplate();
            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Post To Chatter",
                AuthToken = terminalIntegrationTests.Fixtures.HealthMonitor_FixtureData.Salesforce_AuthToken().Result,
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
