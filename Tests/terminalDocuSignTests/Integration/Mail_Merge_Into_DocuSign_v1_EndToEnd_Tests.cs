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
            var solutionCreateUrl = GetHubApiBaseUrl() + "actions/create?solutionName=Mail_Merge_Into_DocuSign";
            await LoginUser(TestUserEmail, TestUserPassword);

            // Create solution
            var routeDto = await HttpGetAsync<RouteFullDTO>(solutionCreateUrl);
        }

        private async Task LoginUser(string email, string password)
        {
            var httpClient = new HttpClient();

            Uri hubBaseUrl = GetHubBaseUrl();
            httpClient.BaseAddress = hubBaseUrl;

            // Get login page and extract request validation token
            var antiFogeryToken = await GetVerificationToken(httpClient);

            // Login user
            string authTicket = await Authenticate(email, password, antiFogeryToken, httpClient);
        }

        private Uri GetHubBaseUrl()
        {
            var hubApiBaseUrl = new Uri(GetHubApiBaseUrl());
            var hubBaseUrl = new Uri(hubApiBaseUrl.Scheme + "://" + hubApiBaseUrl.Host + ":" + hubApiBaseUrl.Port);
            return hubBaseUrl;
        }

        private async Task<string> Authenticate(string email, string password, string verificationToken, HttpClient httpClient)
        {
            var authenticationEndpointUrl = "/dockyardaccount/login";

            var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__RequestVerificationToken", verificationToken),
                    new KeyValuePair<string, string>("Email", email),
                    new KeyValuePair<string, string>("Password", password),

                });

            var response = await httpClient.PostAsync(authenticationEndpointUrl, formContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return "token";
        }

        private async Task<string> GetVerificationToken(HttpClient httpClient)
        {
            var loginFormUrl = "/dockyardaccount";
            var response = await httpClient.GetAsync(loginFormUrl);
            response.EnsureSuccessStatusCode();
            var loginPageText = await response.Content.ReadAsStringAsync();
            var regEx = new System.Text.RegularExpressions.Regex(@"<input\s+name=""__RequestVerificationToken""\s+type=""hidden""\s+value=\""([\w\d-_]+)""\s+\/>");
            var matches = regEx.Match(loginPageText);
            if (matches == null || matches.Groups.Count < 2)
            {
                throw new Exception("Unable to find verification token in the login page HTML code.");
            }
            string formToken = matches.Groups[1].Value;
            return formToken;
        }
    }

    public class AntiFogeryTokens
    {
        public string CookieToken { get; set; }
        public string FormToken { get; set; }
    }
}
