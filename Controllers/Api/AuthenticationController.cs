using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Hub.Infrastructure;
using Hub.Interfaces;
using HubWeb.Infrastructure_HubWeb;
using System.Web.Http.Description;
using Fr8.Infrastructure;

namespace HubWeb.Controllers
{
    public class AuthenticationController : ApiController
    {
        private readonly ISecurityServices _security;
        private readonly IAuthorization _authorization;
        private readonly ITerminal _terminal;


        public AuthenticationController()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _authorization = ObjectFactory.GetInstance<IAuthorization>();
        }
        /// <summary>
        /// Authenticates user with specified credentials within specified terminal
        /// </summary>
        /// <param name="credentials">Authentication parameters</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">Receieved authorization token</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [ActionName("token")]
        [ResponseType(typeof(TokenResponseDTO))]
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
        /// <param name="name">Terminal name</param>
        /// <param name="version">Terminal version</param>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <response code="200">OAuth authorization URL</response>
        /// <response code="403">Unauthorized request</response>
        [HttpGet]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(UrlResponseDTO))]
        [ActionName("initial_url")]
        public async Task<IHttpActionResult> GetOAuthInitiationURL(
            [FromUri(Name = "terminal")]string name,
            [FromUri(Name = "version")]string version)
        {
            Fr8AccountDO account;
            TerminalDO terminal;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminal = _terminal.GetByNameAndVersion(name, version);

                if (terminal == null)
                {
                    throw new ApplicationException("Terminal was not found.");
                }

                account = _security.GetCurrentAccount(uow);
            }

            var externalAuthUrlDTO = await _authorization.GetOAuthInitiationURL(account, terminal);
            return Ok(new UrlResponseDTO { Url = externalAuthUrlDTO.Url });
        }
        /// <summary>
        /// Perform cookie-based authentication on Fr8 Hub. HTTP response will contain authentication cookies
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <response code="200">Successful login attempt</response>
        /// <response code="403">Username or password is invalid</response>
        [HttpPost]
        public IHttpActionResult Login([FromUri]string username, [FromUri]string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
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
            return StatusCode(System.Net.HttpStatusCode.Forbidden);
        }
        //TODO: this method is commented at 07 Jul as it seems not be used anywhere. Also it violates the idea of transferring only DTO objects instead of DO
        //If this method is not needed after a week, please delete it
        /// <summary>
        /// Retrieves auth token structure for the specified terminal and external account
        /// </summary>
        /// <param name="externalAccountId">Id of external account</param>
        /// <param name="terminalId">Terminal Id</param>
        /// <remarks>Id of external account must match that received during initial authentication</remarks>
        /// <response code="403">Unauthorized request</response>
        //[HttpGet]
        //[Fr8ApiAuthorize]
        //[Fr8HubWebHMACAuthenticate]
        //public async Task<IHttpActionResult> GetAuthToken(
        //    [FromUri]string externalAccountId,
        //    [FromUri]string terminalId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var userId = User.Identity.GetUserId();
        //        var terminalDO = await ObjectFactory.GetInstance<ITerminal>().GetTerminalByPublicIdentifier(terminalId);
        //        var token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(externalAccountId, terminalDO.Id, userId);
        //        if (token != null)
        //            return Ok(token);
        //    }
        //    return Ok();
        //}

        //Used internally to pass existing authentication to PlanDirectory. Doesn't show up in API listing
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IHttpActionResult> AuthenticatePlanDirectory()
        {
            var hmacService = ObjectFactory.GetInstance<IHMACService>();
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/authentication/token");
            var headers =
                await
                    hmacService.GenerateHMACHeader(uri, "PlanDirectory",
                        CloudConfigurationManager.GetSetting("PlanDirectorySecret"), User.Identity.GetUserId());

            var json = await client.PostAsync<JObject>(uri, headers: headers);
            var token = json.Value<string>("token");

            return Ok(new { token });
        }
        /// <summary>
        /// Updates existing authorization token with new values provided
        /// </summary>
        /// <remarks>Fr8 authentication headers must be provided</remarks>
        /// <param name="token">Authorization token containing Id of existing token and new values to apply</param>
        /// <response code="200">Token was successfully updated</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
        public IHttpActionResult RenewToken([FromBody]AuthorizationTokenDTO token)
        {
            _authorization.RenewToken(Guid.Parse(token.Id), token.ExternalAccountId, token.Token, token.ExpiresAt);
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
        [Fr8HubWebHMACAuthenticate]
        [ActionName("tokens")]
        [ResponseType(typeof(List<AuthenticationTokenTerminalDTO>))]
        public IHttpActionResult UserTokens()
        {
            var terminals = _terminal.GetAll();
            var authTokens = _authorization.GetAllTokens(User.Identity.GetUserId());

            var groupedTerminals = terminals
                .Where(x => authTokens.Any(y => y.TerminalID == x.Id))
                .OrderBy(x => x.Name)
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
        /// <response code="200">Token was successfully detached and removed</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
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
        /// <response code="200">Token was set as default</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
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
        /// <response code="200">All tokens were successfully granted</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
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
        /// <response code="200">Result of successful verification request sent</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(PhoneNumberVerificationDTO))]
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
                Message = response.Message
            });
        }
        /// <summary>
        /// Verifies SMS-based authorization request by providing recieved verification code
        /// </summary>
        /// <param name="credentials">Object containing details about verification request and verification code</param>
        /// <response code="200">Result of successful phone number verification</response>
        /// <response code="403">Unauthorized request</response>
        [HttpPost]
        [Fr8ApiAuthorize]
        [ResponseType(typeof(TokenResponseDTO))]
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
    }
}