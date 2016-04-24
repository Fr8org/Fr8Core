using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Repositories.Security;
using Data.Repositories.Security.Entities;
using Data.States;
using Hub.Exceptions;
using Hub.Interfaces;

namespace Hub.Security
{
    internal class SecurityServices : ISecurityServices
    {
        public void Login(IUnitOfWork uow, Fr8AccountDO fr8AccountDO)
        {
            ClaimsIdentity identity = GetIdentity(uow, fr8AccountDO);
            HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
            {
                IsPersistent = true
            }, identity);
            ObjectFactory.GetInstance<ITracker>().Identify(fr8AccountDO);
        }

        public Fr8AccountDO GetCurrentAccount(IUnitOfWork uow)
        {
            var currentUser = GetCurrentUser();

            if (string.IsNullOrWhiteSpace(currentUser))
            {
                throw new AuthenticationExeception("Failed to resolve current user id.");
            }

            var account = uow.UserRepository.FindOne(x => x.Id == currentUser);

            if (account == null)
            {
                throw new AuthenticationExeception("Current user id can't be mapped to fr8 user.");
            }

            return account;
        }

        public bool IsCurrentUserHasRole(string role)
        {
            return GetRoleNames().Any(x => x == role);
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

        public ClaimsIdentity GetIdentity(IUnitOfWork uow, Fr8AccountDO fr8AccountDO)
        {
            var um = new DockyardIdentityManager(uow);
            var identity = um.CreateIdentity(fr8AccountDO, DefaultAuthenticationTypes.ApplicationCookie);
            foreach (var roleId in fr8AccountDO.Roles.Select(r => r.RoleId))
            {
                var role = uow.AspNetRolesRepository.GetByKey(roleId);
                identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

                //add organization as claim for runtime usage
                if (fr8AccountDO.Organization != null)
                {
                    identity.AddClaim(new Claim("Organization", fr8AccountDO.Organization.Name));
                }
            }

            return identity;
        }

        /// <summary>
        /// For every new created object setup default security with permissions for Read Object, Edit Object, Delete Object 
        /// and Role OwnerOfCurrentObject
        /// </summary>
        /// <param name="dataObjectId"></param>
        /// <param name="dataObjectType"></param>
        public void SetDefaultObjectSecurity(Guid dataObjectId, string dataObjectType)
        {
            var securityStorageProvider = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
            securityStorageProvider.SetDefaultObjectSecurity(dataObjectId.ToString(), dataObjectType);
        }

        /// <summary>
        /// Authorize current activity by a permission name for some data object. Get role permission for a compare them with all roles that current uses has.
        /// When at least one role is found for this user, he is authorized to perform some activity.
        /// </summary>
        /// <param name="permissionType"></param>
        /// <param name="curObjectId"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool AuthorizeActivity(int permissionType, string curObjectId, string propertyName = null)
        {
            //get all current roles for current user
            var roles = GetRoleNames().ToList();

            //get all role permissions for object
            var securityStorageProvider = ObjectFactory.GetInstance<ISecurityObjectsStorageProvider>();
            var objRolePermissionWrapper = securityStorageProvider.GetRolePermissionsForSecuredObject(curObjectId);

            if (objRolePermissionWrapper == null)
                return false;

            if (string.IsNullOrEmpty(propertyName))
            {
                var authorizedRoles = objRolePermissionWrapper.RolePermissions.Where(x => roles.Contains(x.Role.RoleName));
                return authorizedRoles.Any();
            }
            else
            {
                //find property inside object properties collection with permissions
                if (!objRolePermissionWrapper.Properties.ContainsKey(propertyName)) return false;

                var propertyRolePermissions = objRolePermissionWrapper.Properties[propertyName];
                var authorizedRoles = propertyRolePermissions.Where(x => roles.Contains(x.Role.RoleName));
                return authorizedRoles.Any();
            }
        }
    }
}
