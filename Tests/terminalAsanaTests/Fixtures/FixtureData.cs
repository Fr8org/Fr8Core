using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
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
            return @"{
                      ""access_token"": ""eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdXRob3JpemF0aW9uIjoxNDgyOTk3Njk4NDQ3MzQsInNjb3BlIjoiIiwiaWF0IjoxNDY2ODY0NTA5LCJleHAiOjE0NjY4NjgxMDl9.txkQ5SbfascnSfIOK9lq3ifKxgt_iexEGUIVF_4H--U"",
                      ""token_type"": ""bearer"",
                      ""expires_in"": 3600,
                      ""data"": {
                        ""id"": 145598938536201,
                        ""name"": ""Asana Fr8 Dev"",
                        ""email"": ""asana-dev@fr8.co""
                      },
                      ""refresh_token"": ""0/269f8ed894731bcd111e7e337d2d2453""
                    }";
        }

        public static AuthorizationToken SampleAuthorizationToken()
        {
            return new AuthorizationToken()
            {
                Token = SampleAsanaOauthTokenResponse(),
                ExternalAccountId = "145598938536201",
                AdditionalAttributes = DateTime.UtcNow.AddSeconds(3600).ToString("O"),
                ExpiresAt = DateTime.UtcNow.AddSeconds(3600)
            };
        }

        public static Fr8DataDTO Get_Tasks_v1_InitialConfiguration_Fr8DataDTO()
        {
            var token = SampleAuthorizationToken();

            var activityTemplate = new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Get_Tasks_TEST",
                Version = "1"
            };

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Post To Timeline",
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
