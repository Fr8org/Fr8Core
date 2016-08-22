using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web.Http;
using AutoMapper;
using AutoMapper.Internal;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Utilities;
using Hub.Managers;
using Hub.Services;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using System.Web.Http.Description;
using Fr8.Infrastructure;
using Swashbuckle.Swagger.Annotations;
using WebApi.OutputCache.V2;

namespace HubWeb.Controllers
{
    [DockyardAuthorize]
    public class UsersController : ApiController
    {
        private readonly IMappingEngine _mappingEngine;
        private readonly ISecurityServices _securityServices;
        private readonly Fr8Account _fr8Account;

        public UsersController()
        {
            _fr8Account = ObjectFactory.GetInstance<Fr8Account>();
            _securityServices = ObjectFactory.GetInstance<ISecurityServices>();
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>();
            _fr8Account = ObjectFactory.GetInstance<Fr8Account>();
        }

        #region API Endpoints

        /// <summary>
        /// Retrieves collection of users
        /// </summary> 
        /// <remarks>
        /// User must be logged in. <br/>
        /// Result collection depends on the security privilegies of current user. <br/>
        /// If current user is Fr8 admin then all users are returned. <br/>
        /// If current user is organization admin then all users from his organization are returned. <br/>
        /// Otherwise empty collection is returned
        /// </remarks>
        /// <response code="200">Collection of users. Can be empty</response>
        [ResponseType(typeof(List<UserDTO>))]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                if (_securityServices.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO)))
                {
                    Expression<Func<Fr8AccountDO, bool>> predicate = x => true;
                    return Ok(GetUsers(uow, predicate));
                }

                int? organizationId;
                if (_securityServices.UserHasPermission(PermissionType.ManageInternalUsers, nameof(Fr8AccountDO)))
                {
                    var currentUser = _securityServices.GetCurrentAccount(uow);
                    organizationId = currentUser.OrganizationId;

                    Expression<Func<Fr8AccountDO, bool>> predicate = x => x.OrganizationId == organizationId;
                    return Ok(GetUsers(uow, predicate));
                }

                //todo: show not authorized messsage in activityStream.
                return Ok(new List<UserDTO>(0));
            }
        }
        /// <summary>
        /// Retrieves the collection of user profiles
        /// </summary>
        /// <remarks>
        /// User must be logged in. <br/>
        /// Depending on the current user permissions some profiles may be unavailable
        /// </remarks>
        /// <response code="200">Collection of profiles. Can be empty</response>
        [HttpGet]
        [ResponseType(typeof(List<ProfileDTO>))]
        public IHttpActionResult GetProfiles()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //for now only return profiles that are protected. Those are all default Fr8 core profiles
                var profiles = uow.ProfileRepository.GetQuery().Where(x => x.Protected).Select(x => new ProfileDTO() { Id = x.Id.ToString(), Name = x.Name });

                //only users with permission 'Manage Fr8 Users' need to be able to use 'Fr8 Administrator' profile
                if (!_securityServices.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO)))
                {
                    //remove from list that profile  
                    profiles = profiles.Where(x => x.Name != DefaultProfiles.Fr8Administrator);
                }

                return Ok(profiles.ToList());
            }
        }
        /// <summary>
        /// Retrieves user info for user with specified Id
        /// </summary>
        /// <remarks>
        /// User must be logged in
        /// </remarks>
        /// <param name="id">User Id</param>
        [HttpGet]
        [DockyardAuthorize(Roles = Roles.Admin)]
        [SwaggerResponse(HttpStatusCode.OK, "User info", typeof(UserDTO))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "User doesn't exist", typeof(ErrorDTO))]
        public IHttpActionResult UserData(string id = "")
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO user;

                if (string.IsNullOrEmpty(id))
                {
                    user = uow.UserRepository.FindOne(u => u.EmailAddress.Address == User.Identity.Name);
                }
                else
                {
                    user = uow.UserRepository.FindOne(u => u.Id == id);
                }
                if (user == null)
                {
                    throw new MissingObjectException($"User with Id '{id}' doesn't exist");
                }

                UserDTO userDTO = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                userDTO.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(userDTO.Id).Select(r => r.Name).ToArray());
                return Ok(userDTO);
            }
        }
        /// <summary>
        /// Changes password of the current user
        /// </summary>
        /// <remarks>
        /// User must be logged in
        /// </remarks>
        /// <param name="oldPassword">Old password</param>
        /// <param name="newPassword">New password</param>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "Password was succesfully changed")]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult Update(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(oldPassword))
                throw new Exception("Old password is required.");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.EmailAddress.Address == User.Identity.Name);

                if (_fr8Account.IsValidHashedPassword(user, oldPassword))
                {
                    _fr8Account.UpdatePassword(uow, user, newPassword);
                    uow.SaveChanges();
                }
                else
                    throw new Exception("Invalid current password.");
            }
            return Ok();
        }

        /// <summary>
        /// Updates user info
        /// </summary>
        /// <remarks>
        /// User must be logged in
        /// </remarks>
        /// <param name="userDTO">New user info values</param>
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, "User info was successfully updated")]
        [SwaggerResponseRemoveDefaults]
        public IHttpActionResult UpdateUserProfile(UserDTO userDTO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                bool hasChanged = false;
                var user = uow.UserRepository.FindOne(u => u.Id == userDTO.Id);

                if (_securityServices.UserHasPermission(PermissionType.ManageFr8Users, nameof(Fr8AccountDO)))
                {
                    user.Class = userDTO.Class;
                    user.ProfileId = userDTO.ProfileId;
                    uow.SaveChanges();
                    return Ok();
                }

                if (_securityServices.UserHasPermission(PermissionType.ManageInternalUsers, nameof(Fr8AccountDO)))
                {
                    //security check if user is from same organization
                    user.ProfileId = userDTO.ProfileId;
                    hasChanged = true;
                }

                if (hasChanged)
                    uow.SaveChanges();
            }

            return Ok();
        }

        /// <summary>
        /// Checks if User has the specified permission for the specified object. 
        /// </summary>
        /// <remarks>
        /// User must be logged in
        /// </remarks>
        /// <param name="objectType">Class name to check permissions against (e.g. TerminalDO, PlanNodeDO, etc).</param>
        /// <param name="permissionType">The permission to check.</param>
        /// <param name="userId">Current user Id.</param>
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, "true if the current user has the specified permission, and false if not.")]
        [SwaggerResponseRemoveDefaults]
        [CacheOutput(ServerTimeSpan = 300, ClientTimeSpan = 300, ExcludeQueryStringFromCacheKey = false)]
        public IHttpActionResult CheckPermission(string userId, PermissionType permissionType, string objectType)
        {
            // Check that the correct userid is supplied. 
            // We need User to provide User Id in order to return the correct cached value. 
            // Otherwise all users would receive the same cached value. 
            if (userId != _securityServices.GetCurrentUser())
            {
                return BadRequest("User Id does not correspond to the current user identity.");
            }

            return Ok(_securityServices.UserHasPermission(permissionType, objectType));
        }

        #endregion

        #region Helper Methods

        private List<UserDTO> GetUsers(IUnitOfWork uow, Expression<Func<Fr8AccountDO, bool>> predicate)
        {
            var users = uow.UserRepository.GetQuery().Where(predicate).ToList();
            return users.Select(user =>
            {
                var dto = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                dto.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(user.Id).Select(r => r.Name).ToArray());
                return dto;
            }).ToList();
        }

        public static string GetCallbackUrl(string providerName)
        {
            return GetCallbackUrl(providerName, Server.ServerUrl);
        }

        public static string GetCallbackUrl(string providerName, string serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl))
                throw new ArgumentException("Server Url is empty", "serverUrl");

            return string.Format("{0}{1}AuthCallback/IndexAsync", serverUrl.Replace("www.", ""), providerName);
        }
        
        private ICollection<IdentityUserRole> ConvertRoleStringToRoles(string selectedRole)
        {
            List<IdentityUserRole> userNewRoles = new List<IdentityUserRole>();
            string[] userRoles = { };
            switch (selectedRole)
            {
                case Roles.Admin:
                    userRoles = new[] { Roles.Admin, Roles.StandardUser };
                    break;
                case Roles.StandardUser:
                    userRoles = new[] { Roles.StandardUser };
                    break;
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.AspNetRolesRepository.GetQuery().Where(e => userRoles.Contains(e.Name))
                    .Each(e => userNewRoles.Add(new IdentityUserRole()
                    {
                        RoleId = e.Id
                    }));
            }
            return userNewRoles;
        }

        private string ConvertRolesToRoleString(String[] userRoles)
        {
            if (userRoles.Contains(Roles.Admin))
                return Roles.Admin;
            else if (userRoles.Contains(Roles.StandardUser))
                return Roles.StandardUser;
            else
                return "";
        }
        /// <summary>
        /// Updates status of current user
        /// </summary>
        /// <remarks>
        /// User must be logged in
        /// </remarks>
        /// <param name="userId">User Id</param>
        /// <param name="status">Status to set. 0 - deleted, 1 - active</param>
        public void UpdateStatus(string userId, int status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO curDockyardAccount = uow.UserRepository.GetQuery().Where(user => user.Id == userId).FirstOrDefault();

                if (curDockyardAccount != null)
                {
                    curDockyardAccount.State = status;
                    uow.SaveChanges();
                }
            }
        }

        #endregion
    }
}
