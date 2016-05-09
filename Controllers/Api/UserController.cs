using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Managers;
using Hub.Managers.APIManagers.Authorizers;
using Hub.Services;
using HubWeb.ViewModels;
using Microsoft.AspNet.Identity;
using Utilities;
using Utilities.Logging;

namespace HubWeb.Controllers
{
    [DockyardAuthorize]
    public class UserController : ApiController
    {
        private readonly JsonPackager _jsonPackager;
        private readonly Fr8Account _dockyardAccount;
        private readonly IMappingEngine _mappingEngine;
        private readonly Email _email;

        public UserController()
        {
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>();
            _jsonPackager = new JsonPackager();
            _dockyardAccount = new Fr8Account();
            _email = new Email();
        }

        public static string GetCallbackUrl(string providerName)
        {
            return GetCallbackUrl(providerName, Utilities.Server.ServerUrl);
        }

        public static string GetCallbackUrl(string providerName, string serverUrl)
        {
            if (String.IsNullOrEmpty(serverUrl))
                throw new ArgumentException("Server Url is empty", "serverUrl");

            return String.Format("{0}{1}AuthCallback/IndexAsync", serverUrl.Replace("www.", ""), providerName);
        }

        public ICollection<IdentityUserRole> ConvertRoleStringToRoles(string selectedRole)
        {
            List<IdentityUserRole> userNewRoles = new List<IdentityUserRole>();
            string[] userRoles = { };
            switch (selectedRole)
            {
                case Roles.Admin:
                    userRoles = new[] { Roles.Admin, Roles.Booker, Roles.Customer };
                    break;
                case Roles.Booker:
                    userRoles = new[] { Roles.Booker, Roles.Customer };
                    break;
                case Roles.Customer:
                    userRoles = new[] { Roles.Customer };
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
            else if (userRoles.Contains(Roles.Booker))
                return Roles.Booker;
            else if (userRoles.Contains(Roles.Customer))
                return Roles.Customer;
            else
                return "";
        }

        private UserVM CreateUserVM(Fr8AccountDO u, IUnitOfWork uow)
        {
            return new UserVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                EmailAddress = u.EmailAddress.Address,
                Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(u.Id).Select(r => r.Name).ToArray()),
                //Calendars = u.Calendars.Select(c => new UserCalendarVM { Id = c.Id, Name = c.Name }).ToList(),
                EmailAddressID = u.EmailAddressID.Value,
                Status = u.State.Value
            };
        }

        //Update DockYardAccount Status from user details view valid states are "Active" and "Deleted"
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

        [DockyardAuthorize(Roles = Roles.Admin)]
        public IHttpActionResult Get()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var users = uow.UserRepository.GetAll();
                var userDTOList = users.Select(user =>
                {
                    var dto = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                    dto.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(user.Id).Select(r => r.Name).ToArray());
                    return dto;
                }).ToList();

                return Ok(userDTOList);
            }
        }

        [DockyardAuthorize(Roles = Roles.Admin)]
        public IHttpActionResult Get(string id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.Id == id);
                var userDTO = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                userDTO.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository
                    .GetRoles(userDTO.Id).Select(r => r.Name).ToArray());
                return Ok(userDTO);
            }
        }

        //[Route("api/user/getCurrent")]
        [HttpGet]
        public IHttpActionResult GetCurrent()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.EmailAddress.Address == User.Identity.Name);
                var userDTO = _mappingEngine.Map<Fr8AccountDO, UserDTO>(user);
                userDTO.Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(userDTO.Id).Select(r => r.Name).ToArray());
                return Ok(userDTO);
            }
        }
        //[Route("api/user/getUserData?id=")]
        [HttpGet]
        public IHttpActionResult GetUserData(string id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.Id == id);
                return Ok(new UserDTO { FirstName = user.FirstName, LastName = user.LastName });
            }
        }


        [HttpPost]
        public IHttpActionResult UpdatePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(oldPassword))
                throw new Exception("Old password is required.");
            if (!string.Equals(newPassword, confirmPassword, StringComparison.OrdinalIgnoreCase))
                throw new Exception("New password and confirm password did not match.");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.FindOne(u => u.EmailAddress.Address == User.Identity.Name);

                Fr8Account fr8Account = new Fr8Account();
                if (fr8Account.IsValidHashedPassword(user, oldPassword))
                {
                    fr8Account.UpdatePassword(uow, user, newPassword);
                    uow.SaveChanges();
                }
                else
                    throw new Exception("Invalid current password.");
            }

            return Ok();
        }
    }
}