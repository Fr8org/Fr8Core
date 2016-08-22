using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Web;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Services;
using Newtonsoft.Json;
using terminalBox.Infrastructure;

namespace terminalBox.Controllers
{
    [RoutePrefix("authentication")]
    public class AuthenticationController : ApiController
    {
        private readonly IHubLoggerService _loggerService;

        public AuthenticationController(IHubLoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        //https://account.box.com/api/oauth2/authorize?response_type=code&client_id=MY_CLIENT_ID&state=security_token%3DKnhMJatFipTAnM0nHlZA
        //http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse
        [HttpPost]
        [Route("request_url")]
        public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
        {
            try
            {
                var url = CloudConfigurationManager.GetSetting("BoxAuthUrl");
                var clientId = BoxHelpers.ClientId;
                var redirectUri = BoxHelpers.RedirectUri;
                var state = Guid.NewGuid().ToString();

                url = url + $"response_type=code&client_id={clientId}" +
                      $"&redirect_uri={HttpUtility.UrlEncode(redirectUri)}" +
                      $"&state={HttpUtility.UrlEncode(state)}";

                return new ExternalAuthUrlDTO() {Url = url, ExternalStateToken = HttpUtility.UrlEncode(state)};
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error(ex.Message);
                throw ex;
            }
        }

        [HttpPost]
        [Route("token")]
        public async Task<AuthorizationTokenDTO> GenerateOAuthToken(
         ExternalAuthenticationDTO externalAuthDTO)
        {
            try
            {
                var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
                string code = query["code"];
                string state = query["state"];

                string accessUrl = "https://api.box.com/oauth2/token";

                string url = accessUrl;

                string payload = $"grant_type=authorization_code&code={HttpUtility.UrlEncode(code)}" +
                                 $"&client_id={HttpUtility.UrlEncode(BoxHelpers.ClientId)}&" +
                                 $"&client_secret={HttpUtility.UrlEncode(BoxHelpers.Secret)}" +
                                 $"&redirect_uri={HttpUtility.UrlEncode(BoxHelpers.RedirectUri)}";

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
                    DateTime.UtcNow.AddSeconds(jsonObj.Value<int>("expires_in"))
                    );

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
                await _loggerService.ReportTerminalError(ex);
                Logger.GetLogger().Error(ex.Message);
                return await Task.FromResult(
                    new AuthorizationTokenDTO()
                    {
                        Error = "An error occurred while trying to authorize, please try again later."
                    }
                );
            }
        }
    }
}