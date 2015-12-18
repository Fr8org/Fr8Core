using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;

namespace HubWeb.Controllers.Api
{
    [Fr8ApiAuthorize]
    public class ManageAuthTokenController : ApiController
    {
        public IAction Action { get; set; }
        public IAuthorization Authorization { get; set; }
        public ITerminal Terminal { get; set; }


        public ManageAuthTokenController()
        {
            Action = ObjectFactory.GetInstance<IAction>();
            Authorization = ObjectFactory.GetInstance<IAuthorization>();
            Terminal = ObjectFactory.GetInstance<ITerminal>();
        }

        /// <summary>
        /// Extract user's auth-tokens and parent terminals.
        /// </summary>
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

        /// <summary>
        /// Revoke token.
        /// </summary>
        [HttpPost]
        public IHttpActionResult Revoke(Guid id)
        {
            var accountId = User.Identity.GetUserId();
            Authorization.RevokeToken(accountId, id);

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult TerminalsByActions(IEnumerable<Guid> actionIds)
        {
            var result = new List<ManageAuthToken_Terminal_Action>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var accountId = User.Identity.GetUserId();
                var authTokens = Authorization.GetAllTokens(accountId);

                foreach (Guid actionId in actionIds)
                {
                    var action = Action.GetById(uow, actionId);

                    result.Add(
                        new ManageAuthToken_Terminal_Action()
                        {
                            ActionId = actionId,
                            Terminal = new ManageAuthToken_Terminal()
                            {
                                Id = action.ActivityTemplate.Terminal.Id,
                                Name = action.ActivityTemplate.Terminal.Name,
                                AuthenticationType = action.ActivityTemplate.Terminal.AuthenticationType,
                                AuthTokens = authTokens
                                    .Where(x => x.TerminalID == action.ActivityTemplate.Terminal.Id)
                                    .Select(x => new ManageAuthToken_AuthToken()
                                    {
                                         Id = x.Id,
                                         ExternalAccountName = x.ExternalAccountId,
                                         IsMain = x.IsMain
                                    })
                                    .ToList()
                            }
                        }
                    );
                }
            }

            result = result.OrderBy(x => x.Terminal.Name).ToList();

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult Apply(IEnumerable<ManageAuthToken_Apply> apply)
        {
            foreach (var applyItem in apply)
            {
                Authorization.GrantToken(applyItem.ActionId, applyItem.AuthTokenId);
            }

            return Ok();
        }
    }
}
