using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Infrastructure.StructureMap;
using Hub.Interfaces;
using Hub.Services;

namespace HubWeb.Controllers
{
    public class AuthenticationController : ApiController
    {
        private readonly ISecurityServices _security;
        private readonly IAuthorization _authorization;


        public AuthenticationController()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
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
                terminalDO = uow.TerminalRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.Id == credentials.TerminalId);

                if (terminalDO == null)
                {
                    throw new ApplicationException("Terminal was not found.");
                }

                account = _security.GetCurrentAccount(uow);
            }

            var response = await _authorization.AuthenticateInternal(
                account,
                terminalDO,
                credentials.Domain,
                credentials.Username,
                credentials.Password
            );

            return Ok(new {
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
                terminal = uow.TerminalRepository
                    .GetQuery()
                    .SingleOrDefault(x => x.Id == terminalId);

                if (terminal == null)
                {
                    throw new ApplicationException("Terminal was not found.");
                }

                account = _security.GetCurrentAccount(uow);
            }

            var externalAuthUrlDTO = await _authorization.GetOAuthInitiationURL(account, terminal);
            return Ok(new { Url = externalAuthUrlDTO.Url });
        }
    }
}