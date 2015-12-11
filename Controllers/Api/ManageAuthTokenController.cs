using System;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;

namespace HubWeb.Controllers.Api
{
    [Fr8ApiAuthorize]
    public class ManageAuthTokenController : ApiController
    {
        public IAuthorization Authorization { get; set; }
        public ITerminal Terminal { get; set; }

        public ManageAuthTokenController()
        {
            Authorization = ObjectFactory.GetInstance<IAuthorization>();
            Terminal = ObjectFactory.GetInstance<ITerminal>();
        }

        public IHttpActionResult Get()
        {
            var terminals = Terminal.GetAll();
            var authTokens = Authorization.GetAllTokens(User.Identity.GetUserId());

            var groupedTerminals = terminals
                .Where(x => authTokens.Any(y => y.TerminalID == x.Id))
                .OrderBy(x => x.Name)
                .Select(x => new ManageAuthToken_Terminal()
                {
                    Id = x.Id,
                    Name = x.Name,
                    AuthTokens = authTokens
                        .Where(y => y.TerminalID == x.Id)
                        .OrderBy(y => y.ExternalAccountId)
                        .Select(y => new ManageAuthToken_AuthToken()
                        {
                            Id = y.Id,
                            ExternalAccountName = y.ExternalAccountId
                        })
                        .ToList()
                })
                .ToList();

            return Ok(groupedTerminals);
        }

        public IHttpActionResult Revoke(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}