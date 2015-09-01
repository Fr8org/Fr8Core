using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Core.Interfaces;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using StructureMap;

namespace Core.Security
{
    class SecurityServices : ISecurityServices
    {
        public void Login(IUnitOfWork uow, DockyardAccountDO dockyardAccountDO)
        {
            ClaimsIdentity identity = GetIdentity(uow, dockyardAccountDO);
            HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
            {
                IsPersistent = true
            }, identity);
            ObjectFactory.GetInstance<ITracker>().Identify(dockyardAccountDO);
        }

        public String GetCurrentUser()
        {
            return Thread.CurrentPrincipal.Identity.GetUserId();
        }

        public String GetUserName()
        {
            return Thread.CurrentPrincipal.Identity.GetUserName();
        }

        public String[] GetRoleNames()
        {
            var claimsIdentity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
                return new string[0];
            return claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
        }

        public bool IsAuthenticated()
        {
            return Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        public void Logout()
        {
            HttpContext.Current.GetOwinContext().Authentication.SignOut();
        }

        public ClaimsIdentity GetIdentity(IUnitOfWork uow, DockyardAccountDO dockyardAccountDO)
        {
            var um = new DockyardIdentityManager(uow);
            var identity = um.CreateIdentity(dockyardAccountDO, DefaultAuthenticationTypes.ApplicationCookie);
            foreach (var roleId in dockyardAccountDO.Roles.Select(r => r.RoleId))
            {
                var role = uow.AspNetRolesRepository.GetByKey(roleId);
                identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }
            return identity;
        }
    }
}
