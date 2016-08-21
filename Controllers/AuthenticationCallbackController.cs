using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using StructureMap;
using Data.Entities;
using Hub.Interfaces;
using HubWeb.ViewModels;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Controllers
{
    public class AuthenticationCallbackController : Controller
    {
        private readonly IActivity _activity;
        private readonly IAuthorization _authorization;
        private readonly ITerminal _terminal;
        private readonly ISecurityServices _security;

        public AuthenticationCallbackController()
        {
            _terminal = ObjectFactory.GetInstance<ITerminal>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _authorization = ObjectFactory.GetInstance<IAuthorization>();
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        [HttpGet]
        public async Task<ActionResult> ProcessSuccessfulOAuthResponse(
            string terminalName,
            string terminalVersion,
            string state = null)
        {
            //Here state is optional and is designed to pass auth token external state (to identify token in database) in case 3rd party service doesn't return unknown parameters contained in URL back
            if (string.IsNullOrEmpty(terminalName) || string.IsNullOrEmpty(terminalVersion))
            {
                throw new ApplicationException("TerminalName or TerminalVersion is not specified.");
            }

            var requestQueryString = Request.Url.Query;
            if (!string.IsNullOrEmpty(requestQueryString) && requestQueryString[0] == '?')
            {
                requestQueryString = requestQueryString.Substring(1);
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(requestQueryString);
                if (queryDictionary["error"] != null && queryDictionary["error"] == "access_denied")
                {
                    //user has denied access for our app, so close the window
                    dynamic model = new ExpandoObject();
                    model.AuthorizationTokenId = string.Empty;
                    model.TerminalId = 0;
                    model.TerminalName = string.Empty;

                    return View(model);
                }
                if (!string.IsNullOrEmpty(state) && queryDictionary["state"] == null)
                {
                    requestQueryString = $"{requestQueryString}&state={state}";
                }
            }

            TerminalDO terminal = _terminal.GetAll().FirstOrDefault(x => x.Name == terminalName && x.Version == terminalVersion);

            if (terminal == null)
            {
                throw new ApplicationException("Could not find terminal.");
            }
            string userId=null;

            // It is unlikely that ASP cookies/headers/other stuff that is used to track current session will be send within auth callback from external service
            // We need a smarter approach
            /*using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                userId = _security.GetCurrentAccount(uow).Id;
            }*/

            var externalAuthenticationDTO = new ExternalAuthenticationDTO()
            {
                RequestQueryString = requestQueryString,
                Fr8UserId = userId
            };

            var response = await _authorization.GetOAuthToken(terminal, externalAuthenticationDTO);

            if (string.IsNullOrEmpty(response.Error))
            {
                dynamic model = new ExpandoObject();
                model.AuthorizationTokenId = response.AuthorizationToken.Id;
                model.TerminalId = response.AuthorizationToken.TerminalID;
                model.TerminalName = terminal.Name;

                if (response.AuthorizationToken.ExternalAccountId == "ga_admin@fr8.co")
                {
                    EventManager.TerminalAuthenticationCompleted(response.AuthorizationToken.UserId, terminal, response.AuthorizationToken);
                }
                return View(model);
            }
            else
            {
                EventManager.OAuthAuthenticationFailed(requestQueryString, response.Error);
                return View("Error", new AuthenticationErrorVM()
                {
                    Error = response.Error
                });
            }
        }
    }
}