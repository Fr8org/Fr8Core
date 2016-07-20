
### Ok, if you feel yourself warmed up enough, do the second attempt. 
## Second terminal: Asana.com - helps you organize your todo list into projects.
If external service has SDK (and NuGet packages) it will be much easier to create Activities and handle authentication. But if not, you should do all work by yourself.
This terminal is little bit complicated, so most work will be done inside services using interfaces. You can mimic the codebase but always free to implement all the steps in way you like.

## Step 1 - Same as in previous terminal, create new terminal project

We recommend using the Fr8 Visual Studio Project Template. Add *New Project* and type *Fr8* in search box, you should see online template

 ![Fr8 Terminal Template](./Images/2_tdg_projectTemplate.PNG "Fr8 Terminal Template")

Enter a name of the terminal you want to build (it could look like terminal%ServiceName%). This will generate:

![Fr8 terminal](./Images/3_tdg_terminalProject.PNG "Fr8 terminal")

## Step 2 - Fill terminal information
Here we will have one difference from previous terminal - *AuthenticationType* property is set to *External*. That means **hub** will handle authentication callbacks and going to interact with our terminal during that process.
 
 ---
    namespace terminalAsana
    {
        public static class TerminalData
        {
            public static WebServiceDTO WebServiceDTO = new WebServiceDTO
            {
                Name = "Asana",
                IconPath = "https://asana.com/favicon.ico"
            };

            public static TerminalDTO TerminalDTO = new TerminalDTO
            {
                Endpoint = CloudConfigurationManager.GetSetting("terminalAsana.TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                AuthenticationType = AuthenticationType.External,
                Name = "terminalAsana",
                Label = "Asana",
                Version = "1"
            };
        }
    }
 ---

## Step 3 - Implement authentication

#### a) Interaction with external service.
Asana uses OAuth 2.0 spec so we reflect it in *IAsanaOAuth* interface
    
---
    namespace terminalAsana.Interfaces
    {
        public interface IAsanaOAuth
        {
            OAuthToken          OAuthToken { get; set; }

            event               RefreshTokenEventHandler RefreshTokenEvent;
            
            bool                IsTokenExpired(OAuthToken token);

            Task<OAuthToken>    RefreshOAuthTokenAsync(OAuthToken token);
            Task<OAuthToken>    RefreshTokenIfExpiredAsync(OAuthToken token);

            DateTime            CalculateExpirationTime(int secondsToExpiration);
            string              CreateAuthUrl(string state);
            Task<JObject>       GetOAuthTokenDataAsync(string code);

            bool                IsIntialized { get; }
            Task<IAsanaOAuth>   InitializeAsync(OAuthToken authorizationToken);
        }
    }
 ---

*IsIntialized* property and *InitializeAsync* method appears here because OAuthToken will be passed to Activity after object creation.
We need expose event for *token refresh* to be able notify **hub** in case it happens.  
*OAuthToken* class reflects meaningfull for us fields of token response:

 ---
    namespace terminalAsana.Asana
    {
        public class OAuthToken
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            /// <summary>
            /// The type of token, in our case: bearer
            /// </summary>
            [JsonProperty("token_type")]
            public string TokenType { get; set; }
            
            /// <summary>
            /// Seconds to expiration, usually 3600
            /// </summary>
            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            // oAuth returns seconds till expiration, so we need do calculate absolute DataTime value
            public DateTime ExpirationDate { get; set; }
        }
    } 
 ---

Add delegate and class for event arguments:

 ---
    namespace terminalAsana.Asana.Services
    {
        public delegate void RefreshTokenEventHandler(object sender, AsanaRefreshTokenEventArgs e);

        public class AsanaRefreshTokenEventArgs
        {
            public AsanaRefreshTokenEventArgs(OAuthToken token)
            {
                RefreshedToken = token;
            }
            public OAuthToken RefreshedToken { get; set; }
        }
    }
 ---

Implementation of IAsanaOAuth:

---

    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using Fr8.Infrastructure.Interfaces;
    using Fr8.Infrastructure.Utilities.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Asana.Services
    {
        public class AsanaOAuthService: IAsanaOAuth
        {
            public event RefreshTokenEventHandler RefreshTokenEvent;

            private IRestfulServiceClient _restfulClient;
            private IAsanaParameters _parameters;

            public OAuthToken OAuthToken { get; set; }
            
            public bool IsIntialized { get; private set; } 

            public AsanaOAuthService(IRestfulServiceClient client, IAsanaParameters parameters)
            {
                _restfulClient = client;
                _parameters = parameters;
                OAuthToken = new OAuthToken();
                IsIntialized = false;
            }

            public DateTime CalculateExpirationTime(int secondsToExpiration)
            {
                return DateTime.UtcNow.AddSeconds(secondsToExpiration);
            }

            public bool IsTokenExpired()
            {
                return IsTokenExpired(this.OAuthToken);
            }

            public bool IsTokenExpired(OAuthToken token)
            {
                return token.ExpirationDate <
                    DateTime.UtcNow.AddMinutes(int.Parse(_parameters.MinutesBeforeTokenRenewal));
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

            public async Task<OAuthToken> RefreshOAuthTokenAsync()
            {
                var refreshedToken = await RefreshOAuthTokenAsync(this.OAuthToken).ConfigureAwait(false);
                this.OAuthToken = refreshedToken;

                // replace access_token field on server
                RefreshTokenEvent?.Invoke(this, new AsanaRefreshTokenEventArgs(refreshedToken));         
                
                return this.OAuthToken;
            }

            public async  Task<OAuthToken> RefreshOAuthTokenAsync(OAuthToken token)
            {         
                var url = CloudConfigurationManager.GetSetting("AsanaOAuthTokenUrl");
                
                var contentDic = new Dictionary<string, string>()
                {
                    {"grant_type", "refresh_token" },
                    {"client_id", _parameters.AsanaClientId },
                    {"client_secret", _parameters.AsanaClientSecret},
                    {"refresh_token",this.OAuthToken.RefreshToken}
                };

                var content = new FormUrlEncodedContent(contentDic);
                var jsonObj = await _restfulClient.PostAsync<JObject>(new Uri(url), content).ConfigureAwait(false);
                var refreshedToken = JsonConvert.DeserializeObject<OAuthToken>(jsonObj.ToString());
                refreshedToken.ExpirationDate = this.CalculateExpirationTime(refreshedToken.ExpiresIn);

                return refreshedToken;
            }

            public string CreateAuthUrl(string state)
            {
                var redirectUri = _parameters.AsanaOriginalRedirectUrl;
                var resultUrl = _parameters.AsanaOAuthCodeUrl;
                resultUrl = resultUrl.  Replace("%STATE%", state).
                                        Replace("%REDIRECT_URI%", redirectUri);
                return resultUrl;
            }

            public async Task<JObject> GetOAuthTokenDataAsync(string code)
            {
                var url = _parameters.AsanaOAuthTokenUrl;
                var contentDic = new Dictionary<string, string>()
                {
                    {"grant_type", "authorization_code" },
                    {"client_id", _parameters.AsanaClientId },
                    {"client_secret", _parameters.AsanaClientSecret },
                    {"redirect_uri", HttpUtility.UrlDecode(_parameters.AsanaOriginalRedirectUrl) },
                    {"code",HttpUtility.UrlDecode(code)}
                };
                
                var content = new FormUrlEncodedContent(contentDic);
                var jsonObj = await _restfulClient.PostAsync<JObject>(new Uri(url), content).ConfigureAwait(false);

                return jsonObj;
            }        

            public async Task<IAsanaOAuth> InitializeAsync(OAuthToken authorizationToken)
            {
                try
                {
                    this.OAuthToken = authorizationToken;
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
---

Also we add couple helpers methods like *public bool IsTokenExpired()* which duplicates interface method, but uses internal token object. Constructor takes interfaces for parameters and REST client. 
The parameters implimentation will contatin all neccessary constatns and variables meaningful for asana interaction.

---
    using Fr8.Infrastructure.Utilities.Configuration;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Interfaces
    {
        public interface IAsanaParameters
        {
            string ApiVersion           { get; }
            string DomainName           { get; }
            string ApiEndpoint          { get; }
            string AsanaClientSecret    { get; }
            string AsanaClientId        { get; }

            /// <summary>
            /// The number of objects to return per page. The value must be between 1 and 100.
            /// </summary>
            string Limit                { get; }

            /// <summary>
            /// Example eyJ0eXAiOJiKV1iQLCJhbGciOiJIUzI1NiJ9
            /// An offset to the next page returned by the API.A pagination request will return an offset token, which can be used as an input parameter to the next request.If an offset is not passed in, the API will return the first page of results.
            /// Note: You can only pass in an offset that was returned to you via a previously paginated request.
            /// </summary>
            string Offset               { get; }

            string MinutesBeforeTokenRenewal{ get; }
            string AsanaOriginalRedirectUrl { get; }
            string AsanaOAuthCodeUrl        { get; }
            string AsanaOAuthTokenUrl       { get; } 
                    
            string WorkspacesUrl        { get; }
            string TasksUrl             { get; }
            string UsersUrl             { get; }
            string UsersInWorkspaceUrl  { get; }
            string UsersMeUrl           { get; }
            string StoriesUrl           { get; }
            string StoryUrl             { get; }
            string ProjectsUrl          { get; }
            string ProjectUrl           { get; }
            string ProjectTasksUrl      { get; }
            string ProjectSectionsUrl   { get; }
        }
    }

    namespace terminalAsana.Asana.Services
    {
        public class AsanaParametersService : IAsanaParameters
        {
            public string AsanaClientSecret     { get; set; }
            public string AsanaClientId         { get; set; }

            public string ApiVersion            { get; set; }
            public string DomainName            { get; set; }
            public string ApiEndpoint => this.DomainName + this.ApiVersion;

            public string Limit                 { get; set; }
            public string Offset                { get; }

            //<!--OAuth section-->
            public string MinutesBeforeTokenRenewal { get; set; }
            public string AsanaOriginalRedirectUrl  { get; set; }
            public string AsanaOAuthCodeUrl         { get; set; }
            public string AsanaOAuthTokenUrl        { get; set; }

            //<!--API URL`s-->
            public string WorkspacesUrl => this.ApiEndpoint + "/workspaces";
            public string TasksUrl => this.ApiEndpoint + "/tasks";
            public string UsersUrl => this.ApiEndpoint + "/users/{user-id}";
            public string UsersInWorkspaceUrl => this.ApiEndpoint + "/workspaces/{workspace-id}/users";
            public string UsersMeUrl => this.ApiEndpoint + "/users/me";
            public string StoriesUrl => this.ApiEndpoint + "/tasks/{task-id}/stories";
            public string StoryUrl => this.ApiEndpoint + "/stories/{story-id}";
            public string ProjectsUrl => this.ApiEndpoint + "/projects";
            public string ProjectUrl => this.ApiEndpoint + "/projects/{project-id}";
            public string ProjectTasksUrl => this.ApiEndpoint + "/projects/{project-id}/tasks";
            public string ProjectSectionsUrl => this.ApiEndpoint + "/projects/{project-id}/sections";

            public AsanaParametersService()
            {
                ApiVersion = "1.0";
                DomainName = "https://app.asana.com/api/";

                Limit = CloudConfigurationManager.GetSetting("AsanaNumberOfObjectsLimit");

                AsanaClientSecret = CloudConfigurationManager.GetSetting("AsanaClientSecret");
                AsanaClientId = CloudConfigurationManager.GetSetting("AsanaClientId");

                MinutesBeforeTokenRenewal = CloudConfigurationManager.GetSetting("MinutesBeforeTokenRenewal");
                AsanaOriginalRedirectUrl = CloudConfigurationManager.GetSetting("CoreWebServerUrl") + CloudConfigurationManager.GetSetting("AsanaOriginalRedirectUrl");
                AsanaOAuthCodeUrl = CloudConfigurationManager.GetSetting("AsanaOAuthCodeUrl").Replace("%ASANA_CLIENT_ID%",AsanaClientId);
                AsanaOAuthTokenUrl = CloudConfigurationManager.GetSetting("AsanaOAuthTokenUrl");
            }
        }
    }
---

As *IRestfulServiceClient* implementation we will use *RestfulServiceClient* from Fr8.Infrastructure. 
This classes are all we need for authrization.

#### b) Interaction between terminal and hub.
Let`s look at *AuthenticationController* inside our terminal *Controllers* folder

![AuthenticationController](./Images/11_tdg2_AuthCtrlr.PNG "AuthenticationController")

All user's interaction happens with **hub** interface, so it stores and recives user secrets from third party services, Asana is not exceptin here. First the hub ask our terminal about url where it can aquire a code which will be exchanged for an access token. For this purpose we have *GenerateOAuthInitiationURL()* method which matches `/authentication/request_url` terminal endpoint.




The **hub** shows user a window with recived *url* where user can confirm access privelages for application.

#### с) External service application
- After you registered Asana account, navigate to profile settings

	![Asana profile settings](./Images/12_tdg2_AsanaAccount.PNG "Asana profile settings")

- Then go to Apps tab and "Manage Developer Apps" at the bottom, there you will see list of applications. Add your new application.

    ![Asana app](./Images/13_tdg2_AsanaApp.PNG "Asana app")

In order to proceed successful request we need to pass redirect url. In the example picture we pass 
`https://dev.fr8.co/AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalAsana&terminalVersion=1`
but for local development usually using hub at *http://localhost:30643/*
Hub's AuthenticationCallback controller determine terminal and it's version and passes returned query string to our terminal

#### d) Process returned token values
So, user saw our asana app window requested access, confirmed it. Hub recived request with token data in query string and going to pass it to our terminal.
We should prepare enpoint to proceed the data and finally get the access token.  
This work will be done by *GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)* method at `/authentication/token` endpoint.
It uses received *code* to exchange it to token, then fills and sends data for the hub.  

Whole code of Authentification controller will look like this:
    
---
    using System;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using Fr8.Infrastructure.Data.DataTransferObjects;
    using Fr8.TerminalBase.Services;
    using Newtonsoft.Json.Linq;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Controllers
    {
        [RoutePrefix("authentication")]
        public class AuthenticationController : ApiController
        {
            private readonly IAsanaOAuth _asanaOAuth;
            private readonly IHubLoggerService _loggerService;

            public AuthenticationController(IAsanaOAuth asanaOAuth, IHubLoggerService loggerService)
            {               
                _asanaOAuth = asanaOAuth;
                _loggerService = loggerService;
            }

            [HttpPost]
            [Route("request_url")]
            public ExternalAuthUrlDTO GenerateOAuthInitiationURL()
            {
                var externalStateToken = Guid.NewGuid().ToString();
                var url = _asanaOAuth.CreateAuthUrl(externalStateToken);

                var externalAuthUrlDTO = new ExternalAuthUrlDTO()
                {
                    ExternalStateToken = externalStateToken,
                    Url = url
                };

                return externalAuthUrlDTO;
            }

            [HttpPost]
            [Route("token")]
            public async Task<AuthorizationTokenDTO> GenerateOAuthToken(ExternalAuthenticationDTO externalAuthDTO)
            {
                try
                {
                    var query = HttpUtility.ParseQueryString(externalAuthDTO.RequestQueryString);
                    string code = query["code"];
                    string state = query["state"];

                    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                    {
                        throw new ApplicationException("Code or State is empty.");
                    }

                    var oauthTokenData = await _asanaOAuth.GetOAuthTokenDataAsync(code);
                    var userInfo = oauthTokenData.Value<JObject>("data");
                    var secondsToExpiration = oauthTokenData.Value<int>("expires_in");
                    var expirationDate = _asanaOAuth.CalculateExpirationTime(secondsToExpiration);

                    return new AuthorizationTokenDTO
                    {
                        Token = oauthTokenData.ToString(),
                        ExternalAccountId = userInfo.Value<string>("id"),
                        ExternalAccountName = userInfo.Value<string>("name"),
                        ExternalStateToken = state,
                        ExpiresAt = expirationDate,
                        AdditionalAttributes = expirationDate.ToString("O")
                    };
                }
                catch (Exception ex)
                {
                    await _loggerService.ReportTerminalError(ex, externalAuthDTO.Fr8UserId);

                    return new AuthorizationTokenDTO()
                    {
                        Error = "An error occurred while trying to authorize, please try again later."
                    };
                }
            }
        }
    }
---

## Step 4 - Create SDK for external service
This step leaves freedom for your fantasy. Use any classes and any approach you like. And of course it will be good if service already has SDK for .Net platform, then we can skip this step.

To interact with Asana using OAuth without thinking about token refreshing we will crate *IAsanaOAuthCommunicator* based on *IRestClientService*

---
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Linq;
    using System.Threading.Tasks;
    using Fr8.Infrastructure.Interfaces;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Interfaces
    {
        public interface IAsanaOAuthCommunicator 
        {
            IAsanaOAuth OAuthService { get; set; }
            Task<Dictionary<string, string>> PrepareHeader(Dictionary<string, string> existingHeaders);

            //what was left from IRestClientService
            Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null);
            Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null,Dictionary<string, string> headers = null);
            Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null);
        }
    }

    namespace terminalAsana.Asana.Services
    {
        
        public class AsanaCommunicatorService : IAsanaOAuthCommunicator
        {
            public IAsanaOAuth OAuthService { get; set;}

            private IRestfulServiceClient _restfulClient;

            public AsanaCommunicatorService(IAsanaOAuth oauth, IRestfulServiceClient client)
            {
                OAuthService = oauth;
                _restfulClient = client;
            }

            /// <summary>
            /// Add OAuth access_token to headers
            /// </summary>
            /// <param name="currentHeader"></param>
            /// <returns></returns>
            public async Task<Dictionary<string,string>> PrepareHeader(Dictionary<string,string> currentHeader)
            {
                var token = await OAuthService.RefreshTokenIfExpiredAsync();
                var headers = new Dictionary<string, string>()
                {
                    {"Authorization", $"Bearer {token.AccessToken}"},
                };
                
                var combinedHeaders = currentHeader?.Concat(headers).ToDictionary(k => k.Key, v => v.Value) ?? headers;
                return combinedHeaders;
            }

            public async Task<TResponse> GetAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                var header = await PrepareHeader(headers);             
                var response = await _restfulClient.GetAsync<TResponse>(requestUri, CorrelationId, header);
                return response;
            }

            public async Task<TResponse> PostAsync<TResponse>(Uri requestUri, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                var header = await PrepareHeader(headers);
                var response = await _restfulClient.PostAsync<TResponse>(requestUri, CorrelationId, header);
                return response;
            }

            public async Task<TResponse> PostAsync<TResponse>(Uri requestUri, HttpContent content, string CorrelationId = null, Dictionary<string, string> headers = null)
            {
                var header = await PrepareHeader(headers);
                var response = await _restfulClient.PostAsync<TResponse>(requestUri, content, CorrelationId, header);
                return response;
            }
        }
    }
---

We will create couple key interfaces and plain class objects for Asana entities. 
The Activity we are going to create named 'Get tasks' so you can implement all primary entities that will be used here, they can pull bunch of all other entities so you just can skip them or define as simple string values.

---
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Asana.Entities
    {
        public class AsanaWorkspace 
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            
            [JsonProperty("name")]
            public string Name { get; set; }
            
            [JsonProperty("is_organization")]
            public bool IsOrganization { get; set; }
        }
        
        public class AsanaUser 
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("photo")]
            public IDictionary<string,string> Photo{ get; set; }

            [JsonProperty("workspaces")]
            public IEnumerable<AsanaWorkspace> Workspaces { get; set; }
        }
        
        public class AsanaTask 
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("assignee")]
            public AsanaUser Assignee { get; set; }

            [JsonProperty("assignee_status")]
            public string AssigneeStatus { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonProperty("completed")]
            public bool Completed { get; set; }

            [JsonProperty("completed_at")]
            public DateTime? CompletedAt { get; set; }

            [JsonProperty("due_on")]
            public DateTime? DueOn { get; set; }

            [JsonProperty("due_at")]
            public DateTime? DueAt { get; set; }

            [JsonProperty("external")]
            public string External { get; set; }

            [JsonProperty("followers")]
            public IEnumerable<AsanaUser> Followers { get; set; }

            [JsonProperty("hearted")]
            public bool Hearted { get; set; }

            [JsonProperty("hearts")]
            public IEnumerable<AsanaUser> Hearts  { get; set; }
            
            [JsonProperty("modified_at")]
            public DateTime ModifiedAt { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("notes")]
            public string Notes { get; set; }

            [JsonProperty("num_hearts")]
            public int NumHearts { get; set; }

            [JsonProperty("parent")]
            public AsanaTask Parent { get; set; }

            [JsonProperty("workspace")]
            public AsanaWorkspace Workspace { get; set; }

            //[JsonProperty("projects")]
            //public IEnumerable<AsanaProject> Projects { get; set; }

            //[JsonProperty("memberships")]
            //public IEnumerable<AsanaMembership> Memberships { get; set; }

            //[JsonProperty("tags")]
            //public IEnumerable<AsanaTag> Tags { get; set; }
        }
    }
---

Those entities will be used by services which represent Asana API.  

---
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Newtonsoft.Json;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Interfaces
    {
        public interface IAsanaUsers
        {
            Task<AsanaUser>                 MeAsync();
            Task<AsanaUser>                 GetUserAsync(string id);
            Task<IEnumerable<AsanaUser>>    GetUsersAsync(string workspaceId);
        }

        public interface IAsanaWorkspaces
        {

            //Task<bool>                      UpdateWorkspaceAsync(AsanaWorkspace workspace);
            //Task<AsanaWorkspace>            GetAsync(int id);
            
            Task<IEnumerable<AsanaWorkspace>> GetAsync();
            
            //Task<AsanaWorkspace>            UpdateAsync(AsanaWorkspace workspace);
            //Task<IEnumerable<string>>       SearchAsync(WorkspaceSearchQuery query);
            //Task<AsanaUser>                 AddUserAsync(AsanaUser user);
            //Task<bool>                      RemoveUserAsync(AsanaUser user);
        }

        public interface IAsanaTasks
        {
            //Task<AsanaTask>                 CreateAsync(AsanaTask task);
            //Task<AsanaTask>                 GetAsync(string id);
            //Task<AsanaTask>                 UpdateAsync(AsanaTask task);
            //Task                            DeleteAsync(AsanaTask task);

            Task<IEnumerable<AsanaTask>>      GetAsync(AsanaTaskQuery query);

            //Task<IEnumerable<AsanaTask>>    GetAllSubtasksAsync(string taskId);
            //Task<AsanaTask>                 CreateSubTaskAsync(AsanaTask task);
            //Task                            SetParentTaskAsync(AsanaTask task);
            //Task<IEnumerable<AsanaStory>>   GetStoriesAsync(string taskId);
            //Task<IEnumerable<AsanaProject>> GetProjectsAsync(string taskId);
            //Task                            AddToProjectAsync(AsanaProjectInsertion query);
            //Task                            RemoveFromProject(string taskId, string projectId);
            //Task<IEnumerable<AsanaTag>>     GetTags(string taskId);
            //Task                            AddTag(string taskId, string tagId);
            //Task                            RemoveTag(string taskId, string tagId);
            //Task<AsanaTask>                 AddFollowers(string taskId, IEnumerable<AsanaUser> followers);
            //Task<AsanaTask>                 RemoveFollowers(string taskId, IEnumerable<AsanaUser> followers);
        }
    }
---

We don`t need all those methods right now, so functionallity which won't be use is commented out.

---
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fr8.Infrastructure.Utilities.Logging;
    using Newtonsoft.Json.Linq;
    using terminalAsana.Asana.Entities;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Asana
    {
        public class Users : IAsanaUsers
        {
            private readonly IAsanaOAuthCommunicator _restfulClient;
            private readonly IAsanaParameters _asanaParams;

            public Users(IAsanaOAuthCommunicator client, IAsanaParameters asanaParams)
            {
                _restfulClient = client;
                _asanaParams = asanaParams;
            }

            public async Task<AsanaUser> MeAsync()
            {
                var uri = new Uri(_asanaParams.UsersMeUrl);

                try
                {
                    var response = await _restfulClient.GetAsync<JObject>(uri);
                    var result = response.GetValue("data").ToObject<AsanaUser>();
                    return result;
                }
                catch (Exception exp)
                {
                    Logger.LogError($"terminalAsana error = {exp.Message}");
                    throw;
                }
            }

            public async Task<AsanaUser> GetUserAsync(string userId)
            {
                var uri = new Uri(_asanaParams.UsersUrl.Replace("{user-id}", userId));

                try
                {
                    var response = await _restfulClient.GetAsync<JObject>(uri);
                    var result = response.GetValue("data").ToObject<AsanaUser>();
                    return result;
                }
                catch (Exception exp)
                {
                    Logger.LogError($"terminalAsana error = {exp.Message}");
                    throw;
                }
            }

            public async Task<IEnumerable<AsanaUser>> GetUsersAsync(string  workspaceId)
            {
                var uri = new Uri(_asanaParams.UsersInWorkspaceUrl.Replace("{workspace-id}", workspaceId));

                try
                {
                    var response = await _restfulClient.GetAsync<JObject>(uri);
                    var result = response.GetValue("data").ToObject<IEnumerable<AsanaUser>>();
                    return result;
                }
                catch (Exception exp)
                {
                    Logger.LogError($"terminalAsana error = {exp.Message}");
                    throw;
                }
                
            }
        }
    }


    namespace terminalAsana.Asana
    {
        public class Workspaces : IAsanaWorkspaces
        {
            private IAsanaOAuthCommunicator _restClient;
            private IAsanaParameters _asanaParams;

            public Workspaces(IAsanaOAuthCommunicator client, IAsanaParameters asanaParams)
            {
                _restClient = client;
                _asanaParams = asanaParams;
            }

            public async Task<IEnumerable<AsanaWorkspace>> GetAsync()
            {
                var uri = new Uri(_asanaParams.WorkspacesUrl);
                try
                {
                    var response = await _restClient.GetAsync<JObject>(uri).ConfigureAwait(false);
                    var result = response.GetValue("data").ToObject<IEnumerable<AsanaWorkspace>>();
                    return result;
                }
                catch (Exception exp)
                {
                    Logger.LogError($"terminalAsana error = {exp.Message}");
                    throw;
                }
            }
    }


    namespace terminalAsana.Asana.Services
    {
        public class Tasks : IAsanaTasks
        {
            private IAsanaOAuthCommunicator _restClient;
            private IAsanaParameters _asanaParams;

            public Tasks(IAsanaOAuthCommunicator client, IAsanaParameters parameters)
            {
                _asanaParams = parameters;
                _restClient = client;
            }

            public async Task<IEnumerable<AsanaTask>> GetAsync(AsanaTaskQuery query)
            {
                var baseUrl = _asanaParams.TasksUrl + "?";
                var url = baseUrl;
                url = query.Workspace != null ? url + $"workspace={query.Workspace}&": url;
                url = query.Assignee != null  ? url + $"assignee={query.Assignee}&" : url + $"assignee=me&";
                url = query.Project != null ? baseUrl + $"project={query.Project}&" : url;
                url = query.Tag != null ? baseUrl + $"tag={query.Tag}" : url;
                url = query.CompletedSince != null  ? url + $"completed_since={query.CompletedSince}&" : url;
                url = query.ModifiedSince != null ? url + $"modified_since={query.ModifiedSince}&" : url;
                
                var uri = new Uri(url);

                try
                {
                    var response = await _restClient.GetAsync<JObject>(uri);
                    var result = response.GetValue("data").ToObject<IEnumerable<AsanaTask>>();
                    return result;
                }
                catch (Exception exp)
                {
                    Logger.LogError($"terminalAsana error = {exp.Message}");
                    throw;
                }
            }
        }
    }
---

Finally unite all those class to single *client*

---
    using Fr8.Infrastructure.Interfaces;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Asana.Services
    {
        public class AsanaClient
        {
            //------- Communication logic -------
            public IAsanaOAuth      OAuth       { get; set; }
            public IAsanaParameters Parameters  { get; set; }
            protected IAsanaOAuthCommunicator  RestCommunicator { get; set; }  

            //------- Business logic -------
            public IAsanaTasks      Tasks       { get; set; }
            public IAsanaUsers      Users       { get; set; }
            public IAsanaWorkspaces Workspaces  { get; set; }

            public AsanaClient(IAsanaParameters parameters, IRestfulServiceClient client)
            {
                Parameters = parameters;   

                OAuth = new AsanaOAuthService(client, Parameters);
                RestCommunicator = new AsanaCommunicatorService(OAuth, client);

                Tasks= new Tasks(RestCommunicator,Parameters);
                Users = new Users(RestCommunicator, Parameters);
                Workspaces = new Workspaces(RestCommunicator, Parameters);
            }
        }
    }
---

## Step 5 - Another thing you probably would do is define Manifest for data you operate 
Now we have bunch of standard manifests for crates which other activities will be use. You can read more about this in documentation [Link to docs]()
And submit it via our google form.

---
    using System;
    using System.Collections.Generic;
    using Fr8.Infrastructure.Data.Constants;

    namespace Fr8.Infrastructure.Data.Manifests
    {
        public class AsanaTaskCM:Manifest
        {
            public string Id { get; set; }
            public string Assignee { get; set; }
            public string AssigneeStatus { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool Completed { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime? DueOn { get; set; }
            public DateTime? DueAt { get; set; }
            public string External { get; set; }
            public IEnumerable<string> Followers { get; set; }
            public bool Hearted { get; set; }
            public IEnumerable<string> Hearts { get; set; }
            public DateTime ModifiedAt { get; set; }
            public string Name { get; set; }
            public string Notes { get; set; }
            public int NumHearts { get; set; }
            public IEnumerable<string> Projects { get; set; }
            public string Parent { get; set; }
            public string Workspace { get; set; }
            public IEnumerable<string> Tags { get; set; }

            public AsanaTaskCM():base(MT.AsanaTask)
            {
            }
        }

        public class AsanaTaskListCM : Manifest
        {
            public IEnumerable<AsanaTaskCM> Tasks;

            public AsanaTaskListCM() : base(MT.AsanaTaskList)
            {
            }  
        }
    }
---
*MT.AsanaTask* here is an int value from *MT enum*  you can use number which was given you by Fr8 admins or any other number whch is in allowed developers range (will be defined in future documentation)   


## Step 6 - Add "Get tasks" Activity

To make activities development easy for the terminal we add file AsanaOAuthBaseActivity with same named class to **Activities** folder.
The purpose of this class is handle token interaction. General idea here is separation of concerns all authentification specific actions will do SDK, hub interaction with data transformations is activity responsibility  
You probably want ask 'why don't we implement general auth logic in TerminalActivity base class?' - the answer is simple, every third party service has it's own way of authentication implementation, even if it follows oAuth spec, there are many small incompatible things. 

---
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fr8.Infrastructure.Data.DataTransferObjects;
    using Fr8.Infrastructure.Data.Managers;
    using Fr8.Infrastructure.Data.Manifests;
    using Fr8.Infrastructure.Interfaces;
    using Fr8.Infrastructure.Utilities.Logging;
    using Fr8.TerminalBase.BaseClasses;
    using Newtonsoft.Json.Linq;
    using terminalAsana.Asana;
    using terminalAsana.Asana.Services;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Activities
    {
        public abstract class AsanaOAuthBaseActivity<T> : TerminalActivity<T>
            where T : StandardConfigurationControlsCM
        {
            protected AsanaClient AClient;

            public AsanaOAuthBaseActivity(ICrateManager crateManager, IAsanaParameters parameters, IRestfulServiceClient client) : base(crateManager)
            {
                AClient = new AsanaClient(parameters, client);
            }

            protected override void InitializeInternalState()
            {
                base.InitializeInternalState();
                    
                AClient.OAuth.RefreshTokenEvent += RefreshHubToken;

                var tokenData = JObject.Parse(this.AuthorizationToken.Token);
                var token = new OAuthToken
                {
                    AccessToken = tokenData.Value<string>("access_token"),
                    RefreshToken = tokenData.Value<string>("refresh_token"),
                    ExpirationDate = this.AuthorizationToken.ExpiresAt ?? DateTime.MinValue
                };
                 
                AClient.OAuth = Task.Run(() => AClient.OAuth.InitializeAsync(token)).Result;   
            }

            protected void RefreshHubToken(object sender, AsanaRefreshTokenEventArgs eventArgs)
            {
                var originalTokenData = JObject.Parse(this.AuthorizationToken.Token);
                originalTokenData["access_token"] = eventArgs.RefreshedToken.AccessToken;

                this.AuthorizationToken.AdditionalAttributes = eventArgs.RefreshedToken.ExpirationDate.ToString("O");
                this.AuthorizationToken.ExpiresAt = eventArgs.RefreshedToken.ExpirationDate;
                this.AuthorizationToken.Token = originalTokenData.ToString();
                try
                {
                    var authDTO = Mapper.Map<AuthorizationTokenDTO>(this.AuthorizationToken);
                    Task.Run(() => HubCommunicator.RenewToken(authDTO));
                }
                catch (Exception exp)
                {
                    Logger.LogError($"terminalAsana: Error while token renew:  {exp.Message}", "Asana terminal");
                }
            }
        }
    }
---
We override *InitializeInternalState* because everything we need appears here (not in the class constructor). Also we define *RefreshHubToken* callback which will prepare and renew token data for the hub.   
As like as in previous terminal, we renamed existing or add new file in **Activities** folder to Get_Tasks_v1.cs.

---
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fr8.Infrastructure.Data.Control;
    using Fr8.Infrastructure.Data.Crates;
    using Fr8.Infrastructure.Data.DataTransferObjects;
    using Fr8.Infrastructure.Data.Managers;
    using Fr8.Infrastructure.Data.Manifests;
    using Fr8.Infrastructure.Data.States;
    using Fr8.Infrastructure.Interfaces;
    using Fr8.TerminalBase.Infrastructure;
    using Microsoft.Ajax.Utilities;
    using terminalAsana.Asana.Entities;
    using terminalAsana.Interfaces;

    namespace terminalAsana.Activities
    {
        public class Get_Tasks_v1 : AsanaOAuthBaseActivity<Get_Tasks_v1.ActivityUi>
        {
            public static readonly  ActivityTemplateDTO ActivityTemplateDTO = new ActivityTemplateDTO
            {
                Id = new Guid("4b21e180-8029-4352-a756-52973cd98717"),
                Name = "Get_Tasks",
                Label = "Get Tasks",
                Category = ActivityCategory.Receivers,
                Version = "1",
                MinPaneWidth = 330,
                WebService = TerminalData.WebServiceDTO,
                Terminal = TerminalData.TerminalDTO,
                NeedsAuthentication = true,
                Categories = new[] {
                    ActivityCategories.Receive,
                    new ActivityCategoryDTO(TerminalData.WebServiceDTO.Name, TerminalData.WebServiceDTO.IconPath)
                }
            };
            protected override ActivityTemplateDTO MyTemplate => ActivityTemplateDTO;


---

Now activity template has *NeedsAuthentication* attribute setted to **true**, and don't forget specify **Id** otherwise you activity won't be registered on hub.

---
            private const string RunTimeCrateLabel = "Asana Tasks";
            private const string RunTimeCrateLabelCustomCM = "Asana Tasks List";

            public class ActivityUi : StandardConfigurationControlsCM
            {
                public DropDownList WorkspacesList;
                public DropDownList UsersList;
                public DropDownList ProjectsList;
                public TextBlock Information;

                public ActivityUi()
                {
                    WorkspacesList = new DropDownList()
                    {
                        Label = "Avaliable workspaces",
                        Name = nameof(WorkspacesList),
                        ListItems = new List<ListItem>(),
                        Events = new List<ControlEvent> { ControlEvent.RequestConfig },
                    };

                    ProjectsList = new DropDownList()
                    {
                        Label = "Projects in workspace",
                        Name = nameof(ProjectsList),
                        ListItems = new List<ListItem>(),
                        Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                    };

                    Information = new TextBlock()
                    {
                        Name = nameof(Information),
                        Label = "If you specify a project, username and workspace won`t be taken in account."
                    };

                    UsersList = new DropDownList()
                    {
                        Label = "Users in workspace",
                        Name = nameof(UsersList),
                        ListItems = new List<ListItem>(),
                        Events = new List<ControlEvent> { ControlEvent.RequestConfig }
                    };

                    Controls = new List<ControlDefinitionDTO>(){ WorkspacesList, UsersList, Information, ProjectsList };
                }
            }

            public Get_Tasks_v1(ICrateManager crateManager, IRestfulServiceClient client, IAsanaParameters parameters) : base(crateManager, parameters, client)
            {
                DisableValidationOnFollowup = true;
            }
---
We define activity UI same way as in previous terminal. Pay attention to constructor, now it has *DisableValidationOnFollowup* setted to **true**, it means that when `/configure` call happens, *Validate()* method will not be called before *FollowUp()*.  

To initialize our activity`s UI with Asana workspaces we use client class we created before.

---
            public override async Task Initialize()
            {
                var workspaces = await AClient.Workspaces.GetAsync();
                ActivityUI.WorkspacesList.ListItems = workspaces.Select( w => new ListItem() { Key= w.Name, Value = w.Id} ).ToList();

                CrateSignaller.MarkAvailableAtRuntime<StandardTableDataCM>(RunTimeCrateLabel).AddFields("Task name", "Task id");
                CrateSignaller.MarkAvailableAtRuntime<AsanaTaskListCM>(RunTimeCrateLabelCustomCM);
            }

---
This time we use *CrateSignaller* two times, telling downstream activities that we gonna give them data in form of *AsanaTaskListCM* and *StandardTableDataCM*.

---

            public override async Task FollowUp()
            {           
                if (!ActivityUI.WorkspacesList.Value.IsNullOrWhiteSpace())
                {
                    var users =  await AClient.Users.GetUsersAsync(ActivityUI.WorkspacesList.Value);
                    ActivityUI.UsersList.ListItems = users.Select(w => new ListItem() {Key = w.Name, Value = w.Id}).ToList();                

                    var projects = 
                        await AClient.Projects.Get(new AsanaProjectQuery() {Workspace = ActivityUI.WorkspacesList.Value });     
                    ActivityUI.ProjectsList.ListItems = projects.Select(w => new ListItem() { Key = w.Name, Value = w.Id }).ToList();                                         
                }
            }

            protected override Task Validate()
            {
                ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.WorkspacesList, "Workspace should not be empty");
                if (ActivityUI.WorkspacesList.selectedKey.IsNullOrWhiteSpace())
                {
                    ValidationManager.ValidateDropDownListNotEmpty(ActivityUI.UsersList, "User should not be empty");
                }
    
                return Task.FromResult(0);
            }

            public override async Task Run()
            {
                var query = new AsanaTaskQuery()
                {
                    Workspace = ActivityUI.WorkspacesList.Value,
                    Assignee = ActivityUI.UsersList.Value,
                    Project = ActivityUI.ProjectsList.Value
                };

                var tasks = await AClient.Tasks.GetAsync(query);
                
                var dataRows = tasks.Select(t => new TableRowDTO()
                { Row = {
                    new TableCellDTO() {Cell = new KeyValueDTO("Task name",t.Name)},
                    new TableCellDTO() {Cell = new KeyValueDTO("Task id",t.Id)}
                }}).ToList();      

                var payload = new StandardTableDataCM() {Table = dataRows};
                var customPayload = new AsanaTaskListCM() {Tasks = tasks.Select(t => Mapper.Map<AsanaTaskCM>(t)).ToList()};

                Payload.Add(RunTimeCrateLabel, payload);
                Payload.Add(RunTimeCrateLabelCustomCM, customPayload);
            }
        }
    }

---
Each time before *Run()*, the Activity will call *Validate()* method, and if it has any errors plan execution will be stopped with failure, user`s UI Activity Stream will show description for corresponding validation error.    
Everything inside the Activity is straightforward when you have SDK for external service, one intermediate step we do inside *Run()* is prepare recived data for StandardTableDataCM.   

---

## Step 6 - Configure terminal project

After adding so much stuff the terminal need to be configured to work properly. First, look into web.config appSettings
#### a) web.config file 

---
    <appSettings file="Config\terminalAsana\Settings.config">
        <add key="CoreWebServerUrl" value="http://localhost:30643/" />
        <add key="HubApiVersion" value="v1" />
        <add key="terminalAsana.TerminalEndpoint" value="http://localhost:56785" />
        <add key="TerminalId" value="6a5c763f-4355-49c1-8b25-3e0423d7aaaa" />

        <add key="MinutesBeforeTokenRenewal" value="10" />
        <add key="AsanaOriginalRedirectUrl" value="AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalAsana%26terminalVersion=1" />
        <add key="AsanaOAuthCodeUrl" value="https://app.asana.com/-/oauth_authorize?response_type=code&amp;client_id=%ASANA_CLIENT_ID%&amp;state=%STATE%&amp;redirect_uri=%REDIRECT_URI%" />
        <add key="AsanaOAuthTokenUrl" value="https://app.asana.com/-/oauth_token" />
        <add key="AsanaNumberOfObjectsLimit" value="100" />
    </appSettings>
---

As you can see we choose port 56785 for *terminalAsana.TerminalEndpoint*, don't forget set it in Visual Studio project settings.
Suppose you noticed absence of  *AsanaClientSecret* and *AsanaClientId* values, they are carried out to external settings file, because we don`t want to share our secrets in public repositories.


#### b) TerminalAsansBootstrapper.cs 

---
    using System.Linq;
    using AutoMapper;
    using Fr8.Infrastructure.Data.DataTransferObjects;
    using Fr8.Infrastructure.Data.Manifests;
    using Fr8.TerminalBase.Models;
    using StructureMap;
    using terminalAsana.Interfaces;
    using terminalAsana.Asana;
    using terminalAsana.Asana.Entities;
    using terminalAsana.Asana.Services;

    namespace terminalAsana
    {
        public static class TerminalAsanaBootstrapper
        {
            public static void ConfigureAsanaDependencies(this IContainer container)
            {
                container.Configure(ConfigureLive);
            }

            public static void ConfigureLive(ConfigurationExpression configurationExpression)
            {
                configurationExpression.For<IAsanaParameters>().Use<AsanaParametersService>().Singleton();
                configurationExpression.For<IAsanaOAuth>().Use<AsanaOAuthService>();
                configurationExpression.For<IAsanaWorkspaces>().Use<Workspaces>();
                configurationExpression.For<IAsanaUsers>().Use<Users>();

                TerminalAsanaBootstrapper.ConfigureAutoMappert();
            }

            public static void ConfigureAutoMappert()
            {
                Mapper.CreateMap<AuthorizationToken, AuthorizationTokenDTO>();

                Mapper.CreateMap<AsanaTask, AsanaTaskCM>()
                    .ForMember(cm=>cm.Assignee, opt => opt.ResolveUsing(at => at.Assignee?.Id))
                    .ForMember(cm=>cm.Followers, opt => opt.ResolveUsing(at => at.Followers?.Select(f => f.Id)))
                    .ForMember(cm=>cm.Parent, opt => opt.ResolveUsing(at => at.Parent?.Id))
                    .ForMember(cm=>cm.Hearts, opt => opt.ResolveUsing(at => at.Hearts?.Select(h => h.Id)))
                    //.ForMember(cm=>cm.Projects, opt => opt.ResolveUsing(at => at.Projects?.Select(p => p.Id)))
                    //.ForMember(cm=>cm.Tags, opt => opt.ResolveUsing(at => at.Tags?.Select(t => t.Id)))
                    .ForMember(cm=>cm.Workspace, opt => opt.ResolveUsing(at => at.Workspace?.Id));
            }
        }
    }
---
Here we add our classes to dependency injection container and creat mapping between AsanaTask and AsanaTaskCM, Projects and Tags are commented out because we haven`t brought them to Task definition yet.   

---

c) Startup.cs 
As well as in previous terminal all we need here is register our Activity, to make it avaliable in `/discover` call

---
        protected override void RegisterActivities()
        {
            ActivityStore.RegisterActivity<Activities.Get_Tasks_v1>(Activities.Get_Tasks_v1.ActivityTemplateDTO);
        }
---