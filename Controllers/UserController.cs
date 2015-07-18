using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Core.Managers;
using Core.Managers.APIManagers.Authorizers;
using Web.ViewModels;
using Microsoft.Ajax.Utilities;
using StructureMap;
using Data.Validations;
using System.Linq;
using Utilities;
using Core.Services;
using AutoMapper;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Web.Controllers
{
    [KwasantAuthorize]
    public class UserController : Controller
    {
        private readonly JsonPackager _jsonPackager;
        private readonly User _user;
        private readonly IMappingEngine _mappingEngine;
        private readonly Email _email;

        public UserController()
        {
            _mappingEngine = ObjectFactory.GetInstance<IMappingEngine>();
            _jsonPackager = new JsonPackager();
            _user = new User();
            _email = new Email();
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Index()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<UserDO> userList = uow.UserRepository.GetAll().ToList();

                var userVMList = userList.Select(u => CreateUserVM(u, uow)).ToList();

                return View(userVMList);
            }
        }

        public static string GetCallbackUrl(string providerName, string serverUrl = null)
        {
            if (String.IsNullOrEmpty(serverUrl))
                serverUrl = Utilities.Server.ServerUrl;
            
            return String.Format("{0}{1}AuthCallback/IndexAsync", serverUrl.Replace("www.", ""), providerName);
        }

        public async Task<ActionResult> GrantRemoteCalendarAccess(string providerName)
        {
            var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
            var result = await authorizer.AuthorizeAsync(
                this.GetUserId(),
                this.GetUserName(),
                GetCallbackUrl(providerName),
                Request.RawUrl,
                CancellationToken.None);

            if (result.IsAuthorized)
            {
                // don't wait for this, run it async and return response to the user.
                return RedirectToAction("ShareCalendar", new { remoteCalendarAccessGranted = providerName });
            }
            return new RedirectResult(result.RedirectUri);
        }

        public async Task<ActionResult> RevokeRemoteCalendarAccess(string providerName)
        {
            var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
            await authorizer.RevokeAccessTokenAsync(this.GetUserId(), CancellationToken.None);
            return RedirectToAction("ShareCalendar", new { remoteCalendarAccessForbidden = providerName });
        }

        [HttpPost]
        public async Task<ActionResult> SyncCalendarsNow()
        {
            try
            {
               // await ObjectFactory.GetInstance<CalendarSyncManager>().SyncNowAsync(this.GetUserId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateUserTimezone(String userID, String timezoneID)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userID);
                userDO.TimeZoneID = timezoneID;
                uow.SaveChanges();
                return Json(true);
            }
        }

        public ActionResult MyAccount()
        {
             return View();
        }

        public ActionResult ShowAddUser()
        {
            return View(new UserVM());
        }

        [KwasantAuthorize(Roles = "Admin")]
        public ActionResult Details(String userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userId);
                var userVM = CreateUserVM(userDO, uow);

                return View(userVM);
            }
        }


        [HttpPost]
        public ActionResult RunQuery(UserVM queryParams)
        {
            if (string.IsNullOrEmpty(queryParams.EmailAddress) && string.IsNullOrEmpty(queryParams.FirstName) &&
                string.IsNullOrEmpty(queryParams.LastName))
            {
                var jsonErrorResult = Json(_jsonPackager.Pack(new { Error = "Atleast one field is required" }));
                return jsonErrorResult;
            }
            if (queryParams.EmailAddress != null)
            {
                var ru = new RegexUtilities();

                if (!(ru.IsValidEmailAddress(queryParams.EmailAddress)))
                {
                    var jsonErrorResult = Json(_jsonPackager.Pack(new { Error = "Please provide valid email address" }));
                    return jsonErrorResult;
                }
            }
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var query = uow.UserRepository.GetQuery();
                if (!String.IsNullOrWhiteSpace(queryParams.FirstName))
                    query = query.Where(u => u.FirstName.Contains(queryParams.FirstName));
                if (!String.IsNullOrWhiteSpace(queryParams.LastName))
                    query = query.Where(u => u.LastName.Contains(queryParams.LastName));
                if (!String.IsNullOrWhiteSpace(queryParams.EmailAddress))
                    query = query.Where(u => u.EmailAddress.Address.Contains(queryParams.EmailAddress));

                var matchedUsers = query.ToList();

                var jsonResult = Json(_jsonPackager.Pack(matchedUsers));

                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }

        [HttpPost]
        [KwasantAuthorize(Roles = Roles.Admin)]
        public ActionResult ProcessAddUser(UserVM curCreateUserVM)
        {
            UserDO submittedUserData = _mappingEngine.Map<UserDO>(curCreateUserVM);
            string userPassword = curCreateUserVM.NewPassword;
            bool sendConfirmation = curCreateUserVM.SendMail;
            string displayMessage;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                UserDO existingUser = _user.GetExisting(uow, submittedUserData.EmailAddress.Address);

                if (existingUser != null && String.IsNullOrEmpty(submittedUserData.Id))
                {
                    var jsonSuccessResult = Json(_jsonPackager.Pack(new { Data = "User already exists.", UserId = existingUser.Id }));
                    return jsonSuccessResult;
                }
                ConvertRoleStringToRoles(curCreateUserVM.Role).Each(e => submittedUserData.Roles.Add(e));
                if (existingUser != null)
                {
                    _user.Update(uow, submittedUserData, existingUser);
                    displayMessage = "User updated successfully.";
                }
                else
                {
                    _user.Create(uow, submittedUserData);
                    displayMessage = "User created successfully.";
                }
                if (!String.IsNullOrEmpty(userPassword))
                {
                    _user.UpdatePassword(uow, submittedUserData, userPassword);
                }
                if (sendConfirmation && !String.IsNullOrEmpty(userPassword))
                {
                    //_email.SendLoginCredentials(uow, submittedUserData.EmailAddress.Address, userPassword);
                }
            }
            return Json(_jsonPackager.Pack(new { Data = displayMessage }));
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

        public ActionResult FindUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(String firstName, String lastName, String emailAddress, int[] states)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var users = uow.UserRepository.GetQuery();
                if (!String.IsNullOrWhiteSpace(firstName))
                    users = users.Where(u => u.FirstName.Contains(firstName));
                if (!String.IsNullOrWhiteSpace(lastName))
                    users = users.Where(u => u.LastName.Contains(lastName));
                if (!String.IsNullOrWhiteSpace(emailAddress))
                    users = users.Where(u => u.EmailAddress.Address.Contains(emailAddress));

                users = users.Where(u => states.Contains(u.State.Value));

                return Json(users.ToList().Select(u => new
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        EmailAddress = u.EmailAddress.Address
                    }).ToList()
                );
            }
        }

        private UserVM CreateUserVM(UserDO u, IUnitOfWork uow)
        {
            return new UserVM
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserName = u.UserName,
                EmailAddress = u.EmailAddress.Address,
                Role = ConvertRolesToRoleString(uow.AspNetUserRolesRepository.GetRoles(u.Id).Select(r => r.Name).ToArray()),
                Calendars = u.Calendars.Select(c => new UserCalendarVM { Id = c.Id, Name = c.Name }).ToList(),
                EmailAddressID = u.EmailAddressID.Value,
                Status = u.State.Value
            };
        }

        //Update User Status from user details view valid states are "Active" and "Deleted"
        public void UpdateStatus(string userId, int status)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                UserDO curUser = uow.UserRepository.GetQuery().Where(user => user.Id == userId).FirstOrDefault();

                if (curUser != null)
                {
                    curUser.State = status;
                    uow.SaveChanges();
                }
            }
        }

        public ActionResult ExistingUserAlert(string UserId)
        {
            ViewBag.UserId = UserId;
            return View();
        }

        public ActionResult MakeNewBookingRequest()
        {
            return View();
        }

        public ActionResult ShareCalendar(string remoteCalendarAccessGranted = null,
            string remoteCalendarAccessForbidden = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUserId = this.GetUserId();
                var curUserDO = uow.UserRepository.GetByKey(curUserId);
                if (curUserDO == null)
                {
                    // if we found no user then assume that this user doesn't exists any more and force log off action.
                    return RedirectToAction("LogOff", "Account");
                }
                var tokens = uow.AuthorizationTokenRepository.FindList(at => at.UserID == curUserId);
                var tuple = new Tuple<UserDO, IEnumerable<AuthorizationTokenDO>>(curUserDO, tokens);

                var curManageUserVM =
                    AutoMapper.Mapper.Map<Tuple<UserDO, IEnumerable<AuthorizationTokenDO>>, ManageUserVM>(tuple);
                return View(curManageUserVM);
            }
        }

        public ActionResult LearnHowToUseKwasant()
        {
            return View();
        }
    }
}