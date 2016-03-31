using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hangfire;
using Hub.Interfaces;
using Hub.Managers;
using HubWeb.Infrastructure;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Microsoft.AspNet.Identity;
using Utilities.Interfaces;

namespace HubWeb.Controllers
{
    public class Fr8BaseApiController : ApiController
    {
        protected readonly ISecurityServices _security;

        public Fr8BaseApiController()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        protected bool IsThisTerminalCall()
        {
            return User.Identity.Name.StartsWith("terminal-");
        }

        protected Fr8AccountDO GetUserTerminalOperatesOn()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return _security.GetCurrentAccount(uow);
            }
        }
    }
}