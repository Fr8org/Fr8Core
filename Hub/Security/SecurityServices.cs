using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Hosting;
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
using Hub.Infrastructure;
using Hub.Interfaces;

namespace Hub.Security
{
    internal class SecurityServices : ISecurityServices
    {
        private ISecurityObjectsStorageProvider _securityObjectStorageProvider;

        public SecurityServices(ISecurityObjectsStorageProvider securityObjectStorageProvider)
        {
            _securityObjectStorageProvider = securityObjectStorageProvider;
        }

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
        /// <param name="curObjectType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool AuthorizeActivity(PermissionType permissionType, string curObjectId, string curObjectType, string propertyName = null)
        {
            //check if user is authenticated. Unauthenticated users cannot pass security and come up to here, which means this is internal fr8 event, that need to be passed 
            if (!IsAuthenticated())
                return true; 

            //check if request came from terminal todo: review this part
            if (Thread.CurrentPrincipal is Fr8Principle)
                return true;

            //get all current roles for current user
            var roles = GetRoleNames().ToList();
            if (!roles.Any())
                return true;

            //first check Record Based Permission.
            var objRolePermissionWrapper = _securityObjectStorageProvider.GetRecordBasedPermissionSetForObject(curObjectId);
            if (objRolePermissionWrapper.RolePermissions.Any() || objRolePermissionWrapper.Properties.Any())
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    var permissionSet = objRolePermissionWrapper.RolePermissions.Where(x => roles.Contains(x.Role.RoleName)).SelectMany(l => l.PermissionSet.Permissions.Select(m => m.Id)).ToList();
                    return EvaluatePermissionSet(permissionType, permissionSet);
                }
                else
                {
                    var permissionsCollection = objRolePermissionWrapper.Properties[propertyName];
                    var permissionSet = permissionsCollection.Where(x => roles.Contains(x.Role.RoleName)).SelectMany(l => l.PermissionSet.Permissions.Select(m => m.Id)).ToList();
                    return EvaluatePermissionSet(permissionType, permissionSet);
                }
            }

            //Object Based permission set checks
            var permissionSets = _securityObjectStorageProvider.GetObjectBasedPermissionSetForObject(curObjectId, curObjectType, roles);
            return EvaluatePermissionSet(permissionType, permissionSets);
        }

        private bool EvaluatePermissionSet(PermissionType permissionType, List<int> permissionSet)
        {
            var modifyAllData = permissionSet.FirstOrDefault(x => x == (int) PermissionType.ModifyAllObjects);
            var viewAllData = permissionSet.FirstOrDefault(x => x == (int) PermissionType.ViewAllObjects);

            if (viewAllData != 0 && permissionType == PermissionType.ReadObject) return true;
            if (modifyAllData != 0) return true;

            var currentPermission = permissionSet.FirstOrDefault(x => x == (int) permissionType);
            if (currentPermission != 0) return true;

            return false;
        }
    }
}
