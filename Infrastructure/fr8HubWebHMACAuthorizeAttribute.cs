using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Web.Http;
using System.Web.Http.Controllers;
using Data.Interfaces.DataTransferObjects;
using System.Web.Http.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Caching;
using System.Web;
using Hub.Infrastructure;
using StructureMap;
using Hub.Interfaces;
using HubWeb.Infrastructure;

namespace HubWeb.Infrastructure
{
    public class fr8HubWebHMACAuthorizeAttribute : fr8HMACAuthorizeAttribute
    {
        public fr8HubWebHMACAuthorizeAttribute()
        {
            _terminalService = ObjectFactory.GetInstance<ITerminal>();
        }

        private readonly ITerminal _terminalService;

        protected override async Task<string> GetTerminalSecret(string terminalId)
        {
            var terminal = await _terminalService.GetTerminalById(int.Parse(terminalId));
            if (terminal == null)
            {
                return null;
            }

            return terminal.Secret;
        }

        protected override async Task<bool> CheckAuthentication(string terminalId, string userId)
        {
            var intTerminalId = int.Parse(terminalId);
            var terminal = await _terminalService.GetTerminalById(intTerminalId);
            if (terminal == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(userId))
            {
                //hmm think about this
                //TODO with a empty userId a terminal can only call single Controller
                //which is OpsController
                //until we figure out exceptions, we won't allow this
                return false;
            }

            //let's check if user allowed this terminal to modify it's data
            if (!await _terminalService.IsUserSubscribedToTerminal(intTerminalId, userId))
            {
                return false;
            }
            
            return true;
        }

        protected override void Success(string terminalId, string userId)
        {
            HttpContext.Current.User = new TerminalPrinciple(terminalId, userId, new GenericIdentity(terminalId));
        }
    }
}