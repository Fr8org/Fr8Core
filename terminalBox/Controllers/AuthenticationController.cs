using System;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.BaseClasses;
using Utilities.Configuration.Azure;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using terminalBox.Infrastructure;

namespace terminalBox.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : BaseTerminalController
    {
        private const string curTerminal = "terminalBox";
        //https://account.box.com/api/oauth2/authorize?response_type=code&client_id=MY_CLIENT_ID&state=security_token%3DKnhMJatFipTAnM0nHlZA
        //http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse
        [HttpPost]
        [Route("initial_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            var url = CloudConfigurationManager.GetSetting("BoxAuthUrl");
            var clientId = BoxHelpers.ClientId;
            var redirectUri = BoxHelpers.RedirectUri;
            var state = Guid.NewGuid().ToString();

            url = url + string.Format("response_type=code&client_id={0}&redirect_uri={1}&state={2}",
                clientId, System.Web.HttpUtility.UrlEncode(redirectUri),
               System.Web.HttpUtility.UrlEncode(state));

            return new ExternalAuthUrlDTO() { Url = url, ExternalStateToken = System.Web.HttpUtility.UrlEncode(state) };
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(
         ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                string code;
                string state;
                ParseCodeAndState(externalAuthDTO.RequestQueryString, out code, out state);


                string accessUrl = "https://api.box.com/oauth2/token";

                string url = accessUrl;

                string payload = string.Format("grant_type=authorization_code&code={0}&client_id={1}&client_secret={2}&redirect_uri={3}",
               System.Web.HttpUtility.UrlEncode(code),
               System.Web.HttpUtility.UrlEncode(BoxHelpers.ClientId),
               System.Web.HttpUtility.UrlEncode(BoxHelpers.Secret),
               System.Web.HttpUtility.UrlEncode(BoxHelpers.RedirectUri));

                var httpClient = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                })
                { BaseAddress = new Uri(url) };
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("User-Agent", "oauth2-draft-v10");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, accessUrl);
                request.Content = new StringContent(payload, Encoding.UTF8,
                                                    "application/x-www-form-urlencoded");

                var result = await httpClient.SendAsync(request);
                var response = await result.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<JObject>(response);

                var token = new BoxAuthTokenDO(
                    jsonObj.Value<string>("access_token"),
                    jsonObj.Value<string>("refresh_token"),
                    DateTime.UtcNow.AddSeconds(jsonObj.Value<int>("expires_in")));

                var userId = await new BoxService(token).GetCurrentUserLogin();

                return new AuthorizationTokenDTO()
                {
                    Token = JsonConvert.SerializeObject(token),
                    ExternalStateToken = state,
                    ExternalAccountId = userId
                };
            }
            catch (Exception ex)
            {
                ReportTerminalError(curTerminal, ex);
                return await Task.FromResult(
                    new AuthorizationTokenDTO()
                    {
                        Error = "An error occurred while trying to authorize, please try again later."
                    }
                );
            }
        }

        private void ParseCodeAndState(string queryString, out string code, out string state)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                throw new ApplicationException("QueryString is empty.");
            }
            code = null;
            state = null;
            var tokens = queryString.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                var nameValueTokens = token.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameValueTokens.Length < 2)
                {
                    continue;
                }

                if (nameValueTokens[0] == "code")
                {
                    code = nameValueTokens[1];
                }
                else if (nameValueTokens[0] == "state")
                {
                    state = nameValueTokens[1];
                }
            }
        }
    }
}