using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class AsanaOAuthService: IAsanaOAuth
    {
        
        private readonly IRestfulServiceClient _restfulClient;
        private IHubCommunicator _hubCommunicator;

        public OAuthToken OAuthToken { get; set; }
        public AuthorizationToken AuthorizationToken { get; private set; }

        /// <summary>
        /// Indicates whether service object being initialized with AuthorizationToken
        /// </summary>
        public bool IsIntialized { get; private set; } 

        public AsanaOAuthService(IRestfulServiceClient client)
        {
            _restfulClient = client;
            OAuthToken = new OAuthToken();
            IsIntialized = false;
        }

        /// <summary>
        /// Gets UTC time when token will expire.
        /// </summary>
        /// <param name="secondsToExpiration">number of seconds from issue moment</param>
        /// <returns></returns>
        public DateTime CalculateExpirationTime(int secondsToExpiration)
        {
            return DateTime.UtcNow.AddSeconds(secondsToExpiration);
        }

        /// <summary>
        /// Check expiration for internal token
        /// </summary>
        /// <returns></returns>
        public bool IsTokenExpired()
        {
            return IsTokenExpired(this.OAuthToken);
        }

        public bool IsTokenExpired(OAuthToken token)
        {
            return token.ExpirationDate <
                   DateTime.UtcNow.AddMinutes(int.Parse(CloudConfigurationManager.GetSetting("MinutesBeforeTokenRenewal")));
        }

        public async Task<OAuthToken> RefreshTokenIfExpiredAsync()
        {
            if (!this.IsTokenExpired())
                return this.OAuthToken;
            else
                return await RefreshOAuthTokenAsync().ConfigureAwait(false);
        }

        public async Task<OAuthToken> RefreshTokenIfExpiredAsync(OAuthToken token)
        {
            if (!this.IsTokenExpired(token))
                return token;
            else
                return await RefreshOAuthTokenAsync(token).ConfigureAwait(false);       
        }

        /// <summary>
        /// Refreshes internal token object
        /// </summary>
        public async Task<OAuthToken> RefreshOAuthTokenAsync()
        {
            var refreshedToken = await RefreshOAuthTokenAsync(this.OAuthToken).ConfigureAwait(false);
            this.OAuthToken = refreshedToken;

            // replace access_token field on server

            var originalTokenData = JObject.Parse(this.AuthorizationToken.Token);
            originalTokenData["access_token"] = refreshedToken.Token;

            this.AuthorizationToken.AdditionalAttributes = refreshedToken.ExpirationDate.ToString("O");
            this.AuthorizationToken.Token = originalTokenData.ToString();
            try
            {
                var authDTO = Mapper.Map<AuthorizationTokenDTO>(this.AuthorizationToken);
                await _hubCommunicator.RenewToken(authDTO).ConfigureAwait(false);
            }
            catch (Exception exp)
            {
                var t = exp;
            }          
            
            return this.OAuthToken;
        }

        public async  Task<OAuthToken> RefreshOAuthTokenAsync(OAuthToken token)
        {         
            var url = CloudConfigurationManager.GetSetting("AsanaOAuthTokenUrl");
            
            var contentDic = new Dictionary<string, string>()
            {
                {"grant_type", "refresh_token" },
                {"client_id", CloudConfigurationManager.GetSetting("AsanaClientId") },
                {"client_secret", CloudConfigurationManager.GetSetting("AsanaClientSecret") },
                {"refresh_token",this.OAuthToken.RefreshToken}
            };

            var content = new FormUrlEncodedContent(contentDic);

            //sadly it should be done synchronously or we got deadlock. In the end it didn`t work at all so i commentend it ad replace with HttpClient`s call
            var jsonObj = await _restfulClient.PostAsync<JObject>(new Uri(url), content).ConfigureAwait(false);

            //var httpClient = new HttpClient(new HttpClientHandler
            //{
            //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            //})
            //{ BaseAddress = new Uri(url) };
            //httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //httpClient.DefaultRequestHeaders.Add("User-Agent", "oauth2-draft-v10");
            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            //request.Content = content;

            //var result =  httpClient.SendAsync(request).Result;
            //var response =  result.Content.ReadAsStringAsync().Result;
            //if (!result.IsSuccessStatusCode)
            //{
            //    throw new Exception($"Refresh token request to {url} return {result.StatusCode} with data {response}" );
            //}

            //var jsonObj = JsonConvert.DeserializeObject<JObject>(response);

            var refreshedToken = JsonConvert.DeserializeObject<OAuthToken>(jsonObj.ToString());

            // calculate when it ends
            refreshedToken.ExpirationDate = this.CalculateExpirationTime(refreshedToken.ExpiresIn);

            return refreshedToken;
        }

       

        public string CreateAuthUrl(string state)
        {
            var redirectUri = CloudConfigurationManager.GetSetting("AsanaOriginalRedirectUrl");
            var resultUrl = CloudConfigurationManager.GetSetting("AsanaOAuthCodeUrl");
            resultUrl = resultUrl.  Replace("%STATE%", state).
                                    Replace("%REDIRECT_URI%", redirectUri);
            return resultUrl;
        }

        public async Task<JObject> GetOAuthTokenDataAsync(string code)
        {
            var url = CloudConfigurationManager.GetSetting("AsanaOAuthTokenUrl");
            var contentDic = new Dictionary<string, string>()
            {
                {"grant_type", "authorization_code" },
                {"client_id", CloudConfigurationManager.GetSetting("AsanaClientId") },
                {"client_secret", CloudConfigurationManager.GetSetting("AsanaClientSecret") },
                {"redirect_uri", HttpUtility.UrlDecode(CloudConfigurationManager.GetSetting("AsanaOriginalRedirectUrl")) },
                {"code",HttpUtility.UrlDecode(code)}
            };
            
            var content = new FormUrlEncodedContent(contentDic);

            var jsonObj = await _restfulClient.PostAsync<JObject>(new Uri(url), content).ConfigureAwait(false);
            return jsonObj;
        }        

        /// <summary>
        /// Returns itself initialized with values from AuthorizationToken
        /// </summary>
        /// <param name="authorizationToken"></param>
        /// <returns></returns>
        public async Task<IAsanaOAuth> InitializeAsync(AuthorizationToken authorizationToken, IHubCommunicator hubCommunicator)
        {
            try
            {
                this.AuthorizationToken = authorizationToken;
                this._hubCommunicator = hubCommunicator; 

                var tokenData = JObject.Parse(authorizationToken.Token);
                this.OAuthToken.Token = tokenData.Value<string>("access_token");
                this.OAuthToken.RefreshToken = tokenData.Value<string>("refresh_token");

                this.OAuthToken.ExpirationDate = DateTime.Parse(authorizationToken.AdditionalAttributes).ToUniversalTime();

                this.OAuthToken = await this.RefreshTokenIfExpiredAsync().ConfigureAwait(false);

                this.IsIntialized = true;
            }
            catch (Exception exp)
            {
                throw new Exception("Error while initializing AsanaOAuthService, bad AuthorizationToken", exp);
            }
            return this;
        }
    }
}