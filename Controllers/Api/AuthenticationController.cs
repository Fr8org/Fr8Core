using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using HubWeb.ViewModels;
using System.Web.Http.Description;
using Fr8.Infrastructure;
using Fr8.Infrastructure.Utilities.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;

namespace HubWeb.Controllers
{
    public class AuthenticationController : ApiController
    {
        private readonly ISecurityServices _security;
        private readonly IAuthorization _authorization;
        private readonly IPlanDirectoryService _planDirectoryService;
        private readonly ITerminal _terminal;

        public AuthenticationController(ISecurityServices securityServices, ITerminal terminal, IAuthorization authorization, IPlanDirectoryService planDirectoryService)
        {
            _security = securityServices;
            _terminal = terminal;
            _authorization = authorization;
            _planDirectoryService = planDirectoryService;
        }
        /// <summary>
        /// Authenticates user with specified credentials within specified terminal. Returns authorazition token, terminal id and error message if there is any
        /// </summary>
        /// <param name="credentials">Authentication parameters</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        [HttpPost]
        [Fr8ApiAuthorize]
        [ActionName("token")]
        [SwaggerResponse(HttpStatusCode.OK, "Received authorization token", typeof(TokenResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Specified terminal doesn't support authentication mechanism", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> Authenticate(CredentialsDTO credentials)
        {
            Fr8AccountDO account;
            TerminalDO terminalDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalDO = _terminal.GetByNameAndVersion(credentials.Terminal.Name, credentials.Terminal.Version);
                account = _security.GetCurrentAccount(uow);
            }

            var response = await _authorization.AuthenticateInternal(
                account,
                terminalDO,
                credentials.Domain,
                credentials.Username,
                credentials.Password,
                credentials.IsDemoAccount
            );

            return Ok(new TokenResponseDTO
            {
                TerminalId = response.AuthorizationToken?.TerminalID,
                TerminalName = terminalDO.Name,
                AuthTokenId = response.AuthorizationToken?.Id.ToString(),
                Error = response.Error
            });
        }

        /// <summary>
        /// Retrieves URL used as auhtorization url in OAuth authorization scenario
        /// </summary>
        /// <param name="terminal">Terminal name</param>
        /// <param name="version">Terminal version</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        [HttpGet]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(UrlResponseDTO))]
        [ActionName("initial_url")]
        [SwaggerResponse(HttpStatusCode.OK, "OAuth authorization URL", typeof(UrlResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Specified terminal doesn't support authentication mechanism", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> GetOAuthInitiationURL(
            [FromUri(Name = "terminal")]string name,
            [FromUri(Name = "version")]string version)
        {
            Fr8AccountDO account;
            TerminalDO terminal;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminal = _terminal.GetByNameAndVersion(name, version);
                account = _security.GetCurrentAccount(uow);
            }

            var externalAuthUrlDTO = await _authorization.GetOAuthInitiationURL(account, terminal);
            return Ok(new UrlResponseDTO { Url = externalAuthUrlDTO.Url });
        }

        /// <summary>
        /// Returns demo account information for given terminal if system is in Debug or Dev. Otherwise returns null
        /// </summary>
        /// <param name="terminal">Terminal name</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Receieved demo account information request</response>
        [HttpGet]
        [Fr8ApiAuthorize]
        [ActionName("demoAccountInfo")]
        [ResponseType(typeof(InternalDemoAccountVM))]
        public async Task<IHttpActionResult> GetDemoCredentials([FromUri(Name = "terminal")] string terminalName)
        {
#if DEBUG
            var demoUsername = CloudConfigurationManager.GetSetting(terminalName + ".DemoAccountUsername");
            var demoPassword = CloudConfigurationManager.GetSetting(terminalName + ".DemoAccountPassword");
            var docuSignAuthTokenDTO = new InternalDemoAccountVM()
            {
                Username = demoUsername,
                Password = demoPassword,
                Domain = CloudConfigurationManager.GetSetting(terminalName + ".DemoAccountDomain"),
                HasDemoAccount = (!String.IsNullOrEmpty(demoUsername) && !String.IsNullOrEmpty(demoPassword))
            };
#else
            var docuSignAuthTokenDTO = new InternalDemoAccountVM()
            {
                HasDemoAccount = false
            };
#endif
            return Ok(docuSignAuthTokenDTO);
        }

        /// <summary>
        /// Perform cookie-based authentication on Fr8 Hub. HTTP response will contain authentication cookies
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        [SwaggerResponse(HttpStatusCode.OK, "Login attempt was successful")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "Username of password is invalid")]
        [HttpPost]
        public IHttpActionResult Login([FromUri]string username, [FromUri]string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Logger.GetLogger().Warn($"Username or password is not specified");
                return BadRequest("Username or password is not specified");
            }

            Request.GetOwinContext().Authentication.SignOut();

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO dockyardAccountDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (dockyardAccountDO != null)
                {
                    var passwordHasher = new PasswordHasher();
                    if (passwordHasher.VerifyHashedPassword(dockyardAccountDO.PasswordHash, password) == PasswordVerificationResult.Success)
                    {
                        ISecurityServices security = ObjectFactory.GetInstance<ISecurityServices>();
                        ClaimsIdentity identity = security.GetIdentity(uow, dockyardAccountDO);
                        Request.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
                        {
                            IsPersistent = true
                        }, identity);

                        return Ok();
                    }
                }
            }
            Logger.GetLogger().Warn($"Loging failed for {username}");
            return StatusCode(HttpStatusCode.Forbidden);
        }


        /// <summary>
        /// Updates existing authorization token with new values provided
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="token">Authorization token containing Id of existing token and new values to apply</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Token was successfully renewed")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult RenewToken([FromBody]AuthorizationTokenDTO token)
        {
            _authorization.RenewToken(token);
            return Ok();
        }

        /// <summary>
        /// Lists all current user's authorization tokens grouped by terminal
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Collection of authorization tokens grouped by terminal</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [ActionName("tokens")]
        [ResponseType(typeof(List<AuthenticationTokenTerminalDTO>))]
        public IHttpActionResult UserTokens()
        {
            var terminals = _terminal.GetAll();
            var authTokens = _authorization.GetAllTokens(User.Identity.GetUserId());

            var groupedTerminals = terminals
                .Where(x => authTokens.Any(y => y.TerminalID == x.Id))
                .OrderBy(x => x.Name)
                .AsEnumerable()
                .Select(x => new AuthenticationTokenTerminalDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Label = x.Label,
                    AuthTokens = authTokens
                        .Where(y => y.TerminalID == x.Id && !string.IsNullOrEmpty(y.ExternalAccountId))
                        .Select(y => new AuthenticationTokenDTO
                        {
                            Id = y.Id,
                            ExternalAccountName = y.DisplayName,
                            IsMain = y.IsMain
                        })
                        .OrderBy(y => y.ExternalAccountName)
                        .ToList(),
                    AuthenticationType = x.AuthenticationType,
                    Version = x.Version
                })
                .ToList();

            return Ok(groupedTerminals);
        }

        /// <summary>
        /// Removes authorization token with specified Id from any activity that uses it and then deletes it 
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="id">Id of authorization token</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Token was successfully revoked")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult RevokeToken(Guid id)
        {
            var accountId = User.Identity.GetUserId();
            _authorization.RevokeToken(accountId, id);

            return Ok();
        }
        /// <summary>
        /// Marks authorization token as default allowing automatically assign it to all newly created activities
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="id">Id of authorization token</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "Token was successfully marked as default")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Authorization token doesn't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult SetDefaultToken(Guid id)
        {
            var userId = User.Identity.GetUserId();
            _authorization.SetMainToken(userId, id);

            return Ok();
        }
        /// <summary>
        /// Assigns multiple authorization tokens to respictive activities and marks them as default if specified
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="authTokenList">List of authorization token Id/activity Id pairs</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8TerminalAuthentication]
        [SwaggerResponse(HttpStatusCode.OK, "All tokens were successfully granted")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Activity or authorization token don't exist", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unauthorized request", typeof(ErrorDTO))]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult GrantTokens(IEnumerable<AuthenticationTokenGrantDTO> authTokenList)
        {
            var userId = User.Identity.GetUserId();

            foreach (var applyItem in authTokenList)
            {
                _authorization.GrantToken(applyItem.ActivityId, applyItem.AuthTokenId);

                if (applyItem.IsMain)
                {
                    _authorization.SetMainToken(userId, applyItem.AuthTokenId);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Performs authentication based on sending SMS with verification code to the specified phone number
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="phoneNumberCredentials">Object containing details about auhtorization request</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Result of successful verification request sent", typeof(PhoneNumberVerificationDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Terminal doesn't support authentication mechanism", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unathorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> AuthenticatePhoneNumber(PhoneNumberCredentialsDTO phoneNumberCredentials)
        {
            Fr8AccountDO account;
            TerminalDO terminalDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalDO = _terminal.GetByNameAndVersion(phoneNumberCredentials.Terminal.Name, phoneNumberCredentials.Terminal.Version);
                account = _security.GetCurrentAccount(uow);
            }

            var response = await _authorization.SendAuthenticationCodeToMobilePhone(
                account,
                terminalDO,
                phoneNumberCredentials.PhoneNumber
            );

            return Ok(new PhoneNumberVerificationDTO
            {
                TerminalId = terminalDO.Id,
                TerminalName = terminalDO.Name,
                ClientId = response.ClientId,
                ClientName = response.PhoneNumber,//client name is used as external account id, which is nice to be the phone number
                PhoneNumber = response.PhoneNumber,
                Error = response.Error, 
                Title = response.Title,
                Message = response.Message
            });
        }
        /// <summary>
        /// Verifies SMS-based authorization request by providing recieved verification code
        /// </summary>
        /// <param name="credentials">Object containing details about verification request and verification code</param>
        [HttpPost]
        [Fr8ApiAuthorize]
        [SwaggerResponse(HttpStatusCode.OK, "Result of successful phone number verification", typeof(TokenResponseDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Terminal doesn't support authentication mechanism", typeof(ErrorDTO))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "Unathorized request", typeof(ErrorDTO))]
        public async Task<IHttpActionResult> VerifyPhoneNumberCode(PhoneNumberCredentialsDTO credentials)
        {
            Fr8AccountDO account;
            TerminalDO terminalDO;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminalDO = _terminal.GetByNameAndVersion(credentials.Terminal.Name, credentials.Terminal.Version);
                account = _security.GetCurrentAccount(uow);
            }

            var response = await _authorization.VerifyCodeAndGetAccessToken(
                account,
                terminalDO,
                credentials.PhoneNumber,
                credentials.VerificationCode,
                credentials.ClientId,
                credentials.ClientName);

            return Ok(new TokenResponseDTO
            {
                TerminalId = response.AuthorizationToken?.TerminalID,
                TerminalName = terminalDO.Name,
                AuthTokenId = response.AuthorizationToken?.Id.ToString(),
                Error = response.Error
            });
        }

        [HttpGet]
        [ActionName("is_authenticated")]
        public IHttpActionResult IsAuthenicated()
        {
            var authenticated = User.Identity.IsAuthenticated;
            return Ok(new { authenticated });
        }

        [HttpGet]
        [ActionName("is_privileged")]
        public IHttpActionResult IsPrivileged()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Ok(new { privileged = false });
            }

            var privileged = identity.HasClaim(ClaimsIdentity.DefaultRoleClaimType, "Admin");

            return Ok(new { privileged });
        }
    }
    //This class is purely for Swagger documentation purposes
    public class TokenWrapper
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}