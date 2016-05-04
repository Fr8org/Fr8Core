using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using NUnit.Framework;

namespace terminalQuickBooksTests.Integration
{
    [Explicit]
    public class Terminal_Authentication_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalQuickBooks"; }
        }

        /// <summary>
        /// Make sure http call fails with invalid authentication
        /// </summary>
        [Test, Category("Integration.Authentication.terminalQuickBooks")]
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
    }
}
