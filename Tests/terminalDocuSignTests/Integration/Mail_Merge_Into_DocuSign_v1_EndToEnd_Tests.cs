using System;
using NUnit.Framework;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Hub.Managers;
using Data.Interfaces.Manifests;
using terminalDocuSignTests.Fixtures;
using Newtonsoft.Json.Linq;

namespace terminalDocuSignTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalDocuSignTests.Integration")]
    public class Mail_Merge_Into_DocuSign_v1_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async void Mail_Merge_Into_DocuSign_EndToEnd()
        {
            string baseUrl = GetHubApiBaseUrl();
            var solutionCreateUrl = baseUrl + "actions/create?solutionName=Mail_Merge_Into_DocuSign";

            //
            // Create solution
            //
            var routeDto = await HttpPostAsync<string, RouteFullDTO>(solutionCreateUrl, null);
            var solution = routeDto.Subroutes.FirstOrDefault().Activities.FirstOrDefault();

            //
            // Send configuration request without authentication token
            //
            var configResponse1 = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure?id=" + solution.Id, solution);
            var crateStorage1 = _crate.FromDto(configResponse1.CrateStorage);
            var stAuthCrate1 = crateStorage1.CratesOfType<StandardAuthenticationCM>().FirstOrDefault();
            Assert.NotNull(stAuthCrate1, "No standard authentication crate is returned.");
            Assert.AreEqual("RequiresAuthentication", stAuthCrate1.Label, "No StandardAuthenticationCM crate with the label 'RequiresAuthentication' is provided when Configure called without auth token.");

            //
            // Authenticate with DocuSign
            //
            var creds = new CredentialsDTO()
            {
                Username = "freight.testing@gmail.com",
                Password = "I6HmXEbCxN",
                IsDemoAccount = true,
                TerminalId = solution.ActivityTemplate.TerminalId
            };
            var token = await HttpPostAsync<CredentialsDTO, JObject>(baseUrl + "authentication/token", creds);
            Assert.AreEqual(false, String.IsNullOrEmpty(token["authTokenId"].Value<string>()), "AuthTokenId is missing in API response.");
            Guid tokenGuid = Guid.Parse(token["authTokenId"].Value<string>());

            //
            // Asociate token with action
            //
            var applyToken = new ManageAuthToken_Apply()
            {
                ActivityId = solution.Id,
                AuthTokenId = tokenGuid,
                IsMain = false
            };
            await HttpPostAsync<ManageAuthToken_Apply[], string>(baseUrl + "ManageAuthToken/apply", new ManageAuthToken_Apply[] { applyToken });

            //
            // Send configuration request with authentication token
            //
            var configResponse2 = await HttpPostAsync<ActivityDTO, ActivityDTO>(baseUrl + "actions/configure?id=" + solution.Id, solution);
            var crateStorage2 = _crate.FromDto(configResponse2.CrateStorage);
            Assert.True(crateStorage2.CratesOfType<StandardConfigurationControlsCM>().Count() > 0, "Crate StandardConfigurationControlsCM is missing in API response.");
            Assert.True(crateStorage2.CratesOfType<StandardDesignTimeFieldsCM>().Count() > 0, "Crate StandardDesignTimeFieldsCM is missing in API response.");


        }
    }
}
