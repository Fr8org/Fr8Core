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
using Data.Interfaces.DataTransferObjects;
using Data.Repositories.Security;
using Data.Repositories.Security.Entities;
using Data.States;
using Fr8Data.States;
using Hub.Exceptions;
using Hub.Infrastructure;
using Hub.Interfaces;

namespace Hub.Security
{
    internal class SecurityServices : ISecurityServices
    {
        private const string ProfileClaim = "Profile";

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
            }
            if (fr8AccountDO.OrganizationId.HasValue)
            {
                identity.AddClaim(new Claim("Organization", fr8AccountDO.OrganizationId.Value.ToString()));
            }

            //save profileId from current logged user for future usage inside authorization activities logic
            identity.AddClaim(new Claim(ProfileClaim, fr8AccountDO.ProfileId.ToString()));

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
            if (!IsAuthenticated()) return;

            var currentUserId = GetCurrentUser();
            if (string.IsNullOrEmpty(currentUserId))
            {
                return;
            }

            //get organization id
            int? organizationId = null;
            var claimIdentity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            var claim = claimIdentity?.FindFirst("Organization");
            if (claim != null)
            {
                int orgId;
                int.TryParse(claim.Value, out orgId);
                if (orgId != 0) organizationId = orgId;
            }

            _securityObjectStorageProvider.SetDefaultObjectSecurity(currentUserId, dataObjectId.ToString(), dataObjectType, Guid.Empty, organizationId);
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

            //check if request came from terminal 
            if (Thread.CurrentPrincipal is Fr8Principle)
                return true;

            //get all current roles for current user
            var roles = GetRoleNames().ToList();
            if (!roles.Any())
                return true;

            Guid profileId = GetCurrentUserProfile();
            string fr8AccountId = null;
            if (curObjectType == nameof(PlanNodeDO))
            {
                if (CheckForAppBuilderPlanAndBypassSecurity(curObjectId,out fr8AccountId))
                {
                    return true;
                }
            }

            //first check Record Based Permission.
            bool? evaluator = null;
            var objRolePermissionWrapper = _securityObjectStorageProvider.GetRecordBasedPermissionSetForObject(curObjectId);
            if (objRolePermissionWrapper.RolePermissions.Any() || objRolePermissionWrapper.Properties.Any())
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    //security check for the whole object
                    evaluator = EvaluateObjectPermissionSet(permissionType, objRolePermissionWrapper.Fr8AccountId, objRolePermissionWrapper.RolePermissions, roles);
                }
                else
                {
                    //security check for property inside object
                    var permissionsCollection = objRolePermissionWrapper.Properties[propertyName];
                    evaluator = EvaluateObjectPermissionSet(permissionType, objRolePermissionWrapper.Fr8AccountId, permissionsCollection, roles);
                }
            }

            if (evaluator.HasValue)
            {
                return evaluator.Value;
            }
            
            if(fr8AccountId == GetCurrentUser())
            {
                return true;
            }

            //Object Based permission set checks
            var permissionSets = _securityObjectStorageProvider.GetObjectBasedPermissionSetForObject(curObjectId, curObjectType, profileId);
            return EvaluateProfilesPermissionSet(permissionType, permissionSets);
        }

        private bool CheckForAppBuilderPlanAndBypassSecurity(string curObjectId, out string fr8AccountId)
        {
            //TODO: @makigjuro temp fix until FR-3008 is implemented 
            //bypass security on AppBuilder plan, because that one is visible for every user that has this url
            fr8AccountId = null;
            var activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Guid id;
                if (!Guid.TryParse(curObjectId, out id)) return false;

                var planNode = uow.PlanRepository.GetById<PlanNodeDO>(id);
                fr8AccountId = planNode.Fr8AccountId;
                var mainPlan = uow.PlanRepository.GetById<PlanDO>(planNode.RootPlanNodeId);
                if (mainPlan.Visibility == PlanVisibility.Internal) return true;
                return mainPlan.ChildNodes.OfType<SubPlanDO>().Any(subPlan => subPlan.ChildNodes.OfType<ActivityDO>().Select(activity => activityTemplate.GetByKey(activity.ActivityTemplateId)).Any(template => template.Name == "AppBuilder"));
            }
        }

        /// <summary>
        /// Returns assigned profile to current user. Check inside Identity claims, as a backup query database
        /// </summary>
        /// <returns></returns>
        private Guid GetCurrentUserProfile()
        {
            Guid profileId = Guid.Empty;
            var claimsIdentity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            var profile = claimsIdentity?.Claims.Where(c => c.Type == ProfileClaim).Select(c => c.Value).FirstOrDefault();
            if (profile != null)
            {
                Guid.TryParse(profile, out profileId);
            }

            if (profileId != Guid.Empty)
            {
                return profileId;
            }
            
            //in case nothing found check database for a profile
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var currentUserId = GetCurrentUser();
                var currentUser = uow.UserRepository.GetQuery().FirstOrDefault(x => x.Id == currentUserId);
                if (currentUser != null)
                {
                    profileId = currentUser.ProfileId ?? Guid.Empty;
                }
            }

            return profileId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionType"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public bool UserHasPermission(PermissionType permissionType, string objectType)
        {
            if (!IsAuthenticated())
                return false;

            //this permissions will be returned from cache based on profile
            var permissions = _securityObjectStorageProvider.GetAllPermissionsForUser(GetCurrentUserProfile());
            return permissions.Any(x => x.Permission == (int)permissionType && x.ObjectType == objectType);
        }

        private bool? EvaluateObjectPermissionSet(PermissionType permissionType, string fr8AccountId, List<RolePermission> rolePermissions, List<string> roles, int? organizationId = null)
        {
            //first check if current user is the owner here
            if (fr8AccountId == GetCurrentUser())
            {
                var ownerRolePermission = rolePermissions.FirstOrDefault(x => x.Role.RoleName == Roles.OwnerOfCurrentObject);
                if (ownerRolePermission != null && roles.Contains(ownerRolePermission.Role.RoleName))
                {
                    var currentPermission = ownerRolePermission.PermissionSet.Permissions.Select(x => x.Id).FirstOrDefault(l => l == (int)permissionType);
                    return currentPermission != 0;
                }
            }
            else
            {
                //check also organization
                var claimIdentity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                var claim = claimIdentity?.FindFirst("Organization");
                if (claim != null)
                {
                    int orgId;
                    if (int.TryParse(claim.Value, out orgId) && organizationId.HasValue)
                    {
                        var permissionSetOrg = (from x in rolePermissions.Where(x => x.Role.RoleName != Roles.OwnerOfCurrentObject) where roles.Contains(x.Role.RoleName) from i in x.PermissionSet.Permissions.Select(m => m.Id) select i).ToList();

                        var modifyAllData = permissionSetOrg.FirstOrDefault(x => x == (int)PermissionType.ModifyAllObjects);
                        var viewAllData = permissionSetOrg.FirstOrDefault(x => x == (int)PermissionType.ViewAllObjects);
                        if (viewAllData != 0 && permissionType == PermissionType.ReadObject) return true;
                        if (modifyAllData != 0) return true;
                    }

                    return false;
                }

                var permissionSet = (from x in rolePermissions.Where(x => x.Role.RoleName != Roles.OwnerOfCurrentObject) where roles.Contains(x.Role.RoleName) from i in x.PermissionSet.Permissions.Select(m => m.Id) select i).ToList();
                if (permissionSet.Any())
                {
                    if (permissionType == PermissionType.CreateObject)
                    {
                        var currentPermission = permissionSet.FirstOrDefault(x => x == (int)permissionType);
                        return currentPermission != 0;
                    }

                    var modifyAllData = permissionSet.FirstOrDefault(x => x == (int)PermissionType.ModifyAllObjects);
                    var viewAllData = permissionSet.FirstOrDefault(x => x == (int)PermissionType.ViewAllObjects);
                    if (viewAllData != 0 && permissionType == PermissionType.ReadObject) return true;
                    if (modifyAllData != 0) return true;

                    return false;
                }
            }

            return null;
        }

        private bool EvaluateProfilesPermissionSet(PermissionType permissionType, List<int> permissionSet)
        {
            if (permissionType == PermissionType.CreateObject)
            {
                var currentPermission = permissionSet.FirstOrDefault(x => x == (int)permissionType);
                return currentPermission != 0;
            }

            var modifyAllData = permissionSet.FirstOrDefault(x => x == (int)PermissionType.ModifyAllObjects);
            var viewAllData = permissionSet.FirstOrDefault(x => x == (int)PermissionType.ViewAllObjects);
            if (viewAllData != 0 && permissionType == PermissionType.ReadObject) return true;
            if (modifyAllData != 0) return true;

            return false;
        }
    }
}
