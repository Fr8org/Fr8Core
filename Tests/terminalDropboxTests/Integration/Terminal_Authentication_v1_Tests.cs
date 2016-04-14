using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Microsoft.Owin.Hosting;
using NUnit.Framework;
using terminalDropbox;

namespace terminalDropboxTests.Integration
{
    [Explicit]
    public class Terminal_Authentication_v1_Tests : BaseTerminalIntegrationTest
    {
        public override string TerminalName => "terminalDropbox";

        private const string Host = "http://localhost:19760";
        private IDisposable _app;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            try
            {
                _app = WebApp.Start<Startup>(Host);
            }
            catch
            {
                /* Ignored
                We need this empty exception handling when terminal already started.
                So, if you already start terminal manually (or it started on build server),
                there is no need to use self-hosted Owin server
                */
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _app?.Dispose();
        }

        /// <summary>
        /// Make sure http call fails with invalid authentication
        /// </summary>
        [Test, Category("Integration.Authentication.terminalDropbox")]
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
