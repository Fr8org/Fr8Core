using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;

namespace terminalAsanaTests.Fixtures
{
    public static partial class FixtureData
    {
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
                AdditionalAttributes = DateTime.UtcNow.AddSeconds(3600).ToString("O")
            };
        }
    }
}
