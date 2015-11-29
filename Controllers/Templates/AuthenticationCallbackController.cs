using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;
using HubWeb.ViewModels;

namespace HubWeb.Controllers
{
    public class AuthenticationCallbackController : Controller
    {
        private readonly IAction _action;

        private readonly Authorization _authorization;

        public AuthenticationCallbackController()
        {
            _action = ObjectFactory.GetInstance<IAction>();
            _authorization = new Authorization();
        }

        [HttpGet]
        public async Task<ActionResult> ProcessSuccessfulOAuthResponse(
            string terminalName,
            string terminalVersion)
        {

            if (string.IsNullOrEmpty(terminalName) || string.IsNullOrEmpty(terminalVersion))
            {
                throw new ApplicationException("TerminalName or TerminalVersion is not specified.");
            }

            var requestQueryString = Request.Url.Query;
            if (!string.IsNullOrEmpty(requestQueryString) && requestQueryString[0] == '?')
            {
                requestQueryString = requestQueryString.Substring(1);
            }

            TerminalDO terminal;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                terminal = uow.TerminalRepository
                    .FindOne(x => x.Name == terminalName && x.Version == terminalVersion);
                if (terminal == null)
                {
                    throw new ApplicationException("Could not find terminal.");
                }
            }
            
            var externalAuthenticationDTO = new ExternalAuthenticationDTO()
            {
                RequestQueryString = requestQueryString
            };

            var error = await _authorization.GetOAuthToken(terminal, externalAuthenticationDTO);

            if (string.IsNullOrEmpty(error))
            {
                return View();
            }
            else
            {
                return View("Error", new AuthenticationErrorVM()
                {
                    Error = error
                });
            }
        }
    }
}