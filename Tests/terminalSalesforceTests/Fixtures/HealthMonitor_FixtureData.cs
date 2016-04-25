
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Salesforce.Common;
using StructureMap;
using System;
using System.Threading.Tasks;
using terminalSalesforce.Infrastructure;

namespace terminalSalesforceTests.Fixtures
{
    public static class HealthMonitor_FixtureData
    {
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
                    UserDO = userDO,
                    AdditionalAttributes = tokenDTO.AdditionalAttributes,
                    ExpiresAt = DateTime.Today.AddMonths(1)
                };

                uow.AuthorizationTokenRepository.Add(tokenDO);
                uow.SaveChanges();

                return tokenDO;
            }
        }

        public static ActivityTemplateDTO Get_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Get_Data_TEST",
                Label = "Get Data from Salesforce",
                NeedsAuthentication = true
            };
        }

        public static ActivityTemplateDTO Post_To_Chatter_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Version = "1",
                Name = "Post_To_Chatter_TEST",
                Label = "Post To Chatter",
                NeedsAuthentication = true
            };
        }

        public static Fr8DataDTO Get_Data_v1_InitialConfiguration_ActivityDTO()
        {
            var activityTemplate = Get_Data_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Data from Salesforce.com",
                AuthToken = Salesforce_AuthToken().Result,
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
                AuthToken = Salesforce_AuthToken().Result,
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
