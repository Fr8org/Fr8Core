using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Models;

namespace terminalAsanaTests.Fixtures
{
    public static partial class FixtureData
    {
        public static string SampleTasksDataResponse()
        {
            return @"{
                      ""data"": [{
                        ""id"": 145598938536201,
                        ""name"": ""Asana Fr8 Dev"",
                      }]
                    }";
        }

        public static string SampleUsersDataResponse()
        {
            return @"{
                      ""data"": [{
                        ""id"": 145598938536201,
                        ""name"": ""Asana Fr8 Dev"",
                        ""email"": ""asana-dev@fr8.co""
                      }]
                    }";
        }

        public static string SampleAsanaOauthTokenResponse()
        {
            return CloudConfigurationManager.GetSetting("AsanaTokenResponse");
        }

        public static AuthorizationToken SampleAuthorizationToken()
        {
            return new AuthorizationToken()
            {
                Token = SampleAsanaOauthTokenResponse(),
                ExternalAccountId = "145598938536201",
                AdditionalAttributes = DateTime.UtcNow.AddSeconds(3600).ToString("O"),
                ExpiresAt = DateTime.UtcNow
            };
        }

        public static Fr8DataDTO Get_Tasks_v1_InitialConfiguration_Fr8DataDTO()
        {
            var token = SampleAuthorizationToken();

            var activityTemplate = new ActivityTemplateSummaryDTO()
            {
                TerminalName = "terminalAsana",
                TerminalVersion = "1",
                Name = "Get_Tasks_TEST",
                Version = "1"
            };

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get something",
                ActivityTemplate = activityTemplate,
                AuthToken = new AuthorizationTokenDTO()
                {
                    Token = token.Token,
                    ExpiresAt = token.ExpiresAt,
                    ExternalAccountId = token.ExternalAccountId,
                    ExternalAccountName = "Asana Fr8 Dev"
                }
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public static Fr8DataDTO Post_Comment_v1_InitialConfiguration_Fr8DataDTO()
        {
            var token = SampleAuthorizationToken();

            var activityTemplate = new ActivityTemplateSummaryDTO()
            {
                TerminalName = "terminalAsana",
                TerminalVersion = "1",
                Name = "Post_Comment_TEST",
                Version = "1"
            };

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Post something",
                ActivityTemplate = activityTemplate,
                AuthToken = new AuthorizationTokenDTO()
                {
                    Token = token.Token,
                    ExpiresAt = token.ExpiresAt,
                    ExternalAccountId = token.ExternalAccountId,
                    ExternalAccountName = "Asana Fr8 Dev"
                }
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }
    }
}
