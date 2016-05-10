using System;
using System.Configuration;
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
using Data.Interfaces.DataTransferObjects;
using Data.Infrastructure.StructureMap;
using Hub.Infrastructure;
using Hub.Interfaces;
using Hub.Managers.APIManagers.Transmitters.Restful;
using HubWeb.Infrastructure;
using Utilities.Configuration.Azure;

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

        [HttpPost]
        [Fr8ApiAuthorize]
        [ActionName("token")]
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

            return Ok(new
            {
                TerminalId =
                    response.AuthorizationToken != null
                        ? response.AuthorizationToken.TerminalID
                        : (int?)null,

                AuthTokenId =
                    response.AuthorizationToken != null
                        ? response.AuthorizationToken.Id.ToString()
                        : null,

                Error = response.Error
            });
        }

        [HttpGet]
        [Fr8ApiAuthorize]
        [ActionName("initial_url")]
        public async Task<IHttpActionResult> GetOAuthInitiationURL(
            [FromUri(Name = "id")] int terminalId)
        {
            Fr8AccountDO account;
            TerminalDO terminal;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminal = _terminal.GetByKey(terminalId);

                if (terminal == null)
                {
                    throw new ApplicationException("Terminal was not found.");
                }

                account = _security.GetCurrentAccount(uow);
            }

            var externalAuthUrlDTO = await _authorization.GetOAuthInitiationURL(account, terminal);
            return Ok(new { Url = externalAuthUrlDTO.Url });
        }

        [HttpPost]
        public IHttpActionResult Login([FromUri]string username, [FromUri]string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return BadRequest();
            }

            Request.GetOwinContext().Authentication.SignOut();

            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO dockyardAccountDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (dockyardAccountDO != null)
                {
                    var passwordHasher = new PasswordHasher();
                    if (passwordHasher.VerifyHashedPassword(dockyardAccountDO.PasswordHash, password) ==
                        PasswordVerificationResult.Success)
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

        [HttpGet]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> GetAuthToken([FromUri]string curFr8UserId, [FromUri]string externalAccountId, [FromUri] string terminalId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var terminalDO = await ObjectFactory.GetInstance<ITerminal>().GetTerminalByPublicIdentifier(terminalId);
                var token = uow.AuthorizationTokenRepository.FindTokenByExternalAccount(externalAccountId, terminalDO.Id, curFr8UserId);
                if (token != null)
                    return Ok(token);
            }
            return Ok();
        }

        [HttpPost]
        [Fr8ApiAuthorize]
        [Fr8HubWebHMACAuthenticate]
        public async Task<IHttpActionResult> AuthenticatePlanDirectory()
        {
            var hmacService = ObjectFactory.GetInstance<IHMACService>();
            var client = ObjectFactory.GetInstance<IRestfulServiceClient>();

            var uri = new Uri(ConfigurationManager.AppSettings["PlanDirectoryUrl"] + "/api/authentication/token");
            var headers = await hmacService.GenerateHMACHeader(uri, "PlanDirectory", CloudConfigurationManager.GetSetting("PlanDirectorySecret"), User.Identity.GetUserId());

            var json = await client.PostAsync<JObject>(uri, headers: headers);
            var token = json.Value<string>("token");

            return Ok(new { token });
        }
    }
}