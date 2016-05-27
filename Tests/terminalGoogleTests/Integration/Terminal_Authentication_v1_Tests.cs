using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using Fr8Infrastructure.Communication;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services.Authorization;

namespace terminalGoogleTests.Integration
{
    [Explicit]
    [Category("Integration.Authentication.terminalGoogle")]
    public class Terminal_Authentication_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalGoogle";

        /// <summary>
        /// Make sure http call fails with invalid authentication
        /// </summary>
        [Test]
        [ExpectedException(
            ExpectedException = typeof(RestfulServiceException),
            ExpectedMessage = @"Authorization has been denied for this request.",
            MatchType = MessageMatch.Contains
        )]
        public async Task Should_Fail_WithAuthorizationError()
        {
            //Arrange
            var configureUrl = GetTerminalConfigureUrl();

            var uri = new Uri(configureUrl);
            var hmacHeader = new Dictionary<string, string>()
            {
                { System.Net.HttpRequestHeader.Authorization.ToString(), "hmac test:2:3:4" }
            };
            //lets modify hmacHeader
            await RestfulServiceClient.PostAsync<string, ActivityDTO>(uri, "testContent", null, hmacHeader);
        }

        [Test]
        [TestCase("foo", "bar", "12/31/9999")]
        [TestCase("valid_token", "refresh_token", "01/20/2001")]
        public async Task ShouldReturnFalse_WhenTokenInvalid(string accessToken, string refreshToken, DateTime expires)
        {
            // Arrange
            var invalidToken = new GoogleAuthDTO
            {
                 AccessToken= accessToken,
                 RefreshToken = refreshToken,
                 Expires = expires
            };
            var sut = new GoogleIntegration();
            
            // Act
            var result = await sut.IsTokenInfoValid(invalidToken);
            // Assert
            Assert.False(result);
        }
    }
}
