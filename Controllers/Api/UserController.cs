using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8Data.DataTransferObjects;
using Hub.Managers;
using Hub.Services;
using HubWeb.ViewModels;
using Utilities;

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

        //[DockyardAuthorize(Roles = "Admin")]
        //public ActionResult Index()
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        List<DockyardAccountDO> userList = uow.UserRepository.GetAll().ToList();

        //        var userVMList = userList.Select(u => CreateUserVM(u, uow)).ToList();

        //        return View(userVMList);
        //    }
        //}

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

        //public async Task<ActionResult> GrantAccess(string providerName)
        //{
        //    var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
        //    var result = await authorizer.AuthorizeAsync(
        //        this.GetUserId(),
        //        this.GetUserName(),
        //        GetCallbackUrl(providerName),
        //        Request.RawUrl,
        //        CancellationToken.None);

        //    if (result.IsAuthorized)
        //    {
        //        // don't wait for this, run it async and return response to the user.
        //        return RedirectToAction("RemoteServices", new { remoteServiceAccessGranted = providerName });
        //    }
        //    return new RedirectResult(result.RedirectUri);
        //}

        //public async Task<ActionResult> RevokeAccess(string providerName)
        //{
        //    var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(providerName);
        //    await authorizer.RevokeAccessTokenAsync(this.GetUserId(), CancellationToken.None);
        //    return RedirectToAction("RemoteServices", new { remoteServiceAccessForbidden = providerName });
        //}

        //[HttpPost]
        //public ActionResult UpdateUserTimezone(String userID, String timezoneID)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var userDO = uow.UserRepository.GetByKey(userID);
        //        userDO.TimeZoneID = timezoneID;
        //        uow.SaveChanges();
        //        return Json(true);
        //    }
        //}

        //public ActionResult MyAccount()
        //{
        //    return View();
        //}

        //public ActionResult ShowAddUser()
        //{
        //    return View(new UserVM());
        //}

        //[DockyardAuthorize(Roles = "Admin")]
        //public ActionResult Details(String userId)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var userDO = uow.UserRepository.GetByKey(userId);
        //        var userVM = CreateUserVM(userDO, uow);

        //        return View(userVM);
        //    }
        //}


        //[HttpPost]
        //public ActionResult RunQuery(UserVM queryParams)
        //{
        //    if (string.IsNullOrEmpty(queryParams.EmailAddress) && string.IsNullOrEmpty(queryParams.FirstName) &&
        //        string.IsNullOrEmpty(queryParams.LastName))
        //    {
        //        var jsonErrorResult = Json(_jsonPackager.Pack(new { Error = "Atleast one field is required" }));
        //        return jsonErrorResult;
        //    }
        //    if (queryParams.EmailAddress != null)
        //    {
        //        var ru = new RegexUtilities();

        //        if (!(ru.IsValidEmailAddress(queryParams.EmailAddress)))
        //        {
        //            var jsonErrorResult = Json(_jsonPackager.Pack(new { Error = "Please provide valid email address" }));
        //            return jsonErrorResult;
        //        }
        //    }
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var query = uow.UserRepository.GetQuery();
        //        if (!String.IsNullOrWhiteSpace(queryParams.FirstName))
        //            query = query.Where(u => u.FirstName.Contains(queryParams.FirstName));
        //        if (!String.IsNullOrWhiteSpace(queryParams.LastName))
        //            query = query.Where(u => u.LastName.Contains(queryParams.LastName));
        //        if (!String.IsNullOrWhiteSpace(queryParams.EmailAddress))
        //            query = query.Where(u => u.EmailAddress.Address.Contains(queryParams.EmailAddress));

        //        var matchedUsers = query.ToList();

        //        var jsonResult = Json(_jsonPackager.Pack(matchedUsers));

        //        jsonResult.MaxJsonLength = int.MaxValue;
        //        return jsonResult;
        //    }
        //}

        //[HttpPost]
        //[DockyardAuthorize(Roles = Roles.Admin)]
        //public ActionResult ProcessAddUser(UserVM curCreateUserVM)
        //{
        //    DockyardAccountDO submittedDockyardAccountData = new DockyardAccountDO();
        //    Mapper.Map(curCreateUserVM, submittedDockyardAccountData);
        //    string userPassword = curCreateUserVM.NewPassword;
        //    bool sendConfirmation = curCreateUserVM.SendMail;
        //    string displayMessage;

        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        DockyardAccountDO existingDockyardAccount = _dockyardAccount.GetExisting(uow, submittedDockyardAccountData.EmailAddress.Address);

        //        if (existingDockyardAccount != null && String.IsNullOrEmpty(submittedDockyardAccountData.Id))
        //        {
        //            var jsonSuccessResult = Json(_jsonPackager.Pack(new { Data = "DockYardAccount already exists.", UserId = existingDockyardAccount.Id }));
        //            return jsonSuccessResult;
        //        }
        //        ConvertRoleStringToRoles(curCreateUserVM.Role).Each(e => submittedDockyardAccountData.Roles.Add(e));
        //        if (existingDockyardAccount != null)
        //        {
        //            _dockyardAccount.Update(uow, submittedDockyardAccountData, existingDockyardAccount);
        //            displayMessage = "DockYardAccount updated successfully.";
        //        }
        //        else
        //        {
        //            _dockyardAccount.Create(uow, submittedDockyardAccountData);
        //            displayMessage = "DockYardAccount created successfully.";
        //        }
        //        if (!String.IsNullOrEmpty(userPassword))
        //        {
        //            _dockyardAccount.UpdatePassword(uow, submittedDockyardAccountData, userPassword);
        //        }
        //        if (sendConfirmation && !String.IsNullOrEmpty(userPassword))
        //        {
        //            //_email.SendLoginCredentials(uow, submittedUserData.EmailAddress.Address, userPassword);
        //        }
        //    }
        //    return Json(_jsonPackager.Pack(new { Data = displayMessage }));
        //}

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

        //public ActionResult ExistingUserAlert(string UserId)
        //{
        //    ViewBag.UserId = UserId;
        //    return View();
        //}

        //public ActionResult MakeNewBookingRequest()
        //{
        //    return View();
        //}

        //public ActionResult RemoteServices(string remoteServiceAccessGranted = null,
        //    string remoteServiceAccessForbidden = null)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var curUserId = this.GetUserId();
        //        var curUserDO = uow.UserRepository.GetByKey(curUserId);
        //        if (curUserDO == null)
        //        {
        //            // if we found no user then assume that this user doesn't exists any more and force log off action.
        //            return RedirectToAction("LogOff", "DockyardAccount");
        //        }
        //        var curManageUserVM = Mapper.Map<DockyardAccountDO, ManageUserVM>(curUserDO);
        //        var tokens = uow.AuthorizationTokenRepository.FindList(at => at.UserID == curUserId);
        //        curManageUserVM.HasDocusignToken = tokens.Any();
        //        var googleAuthDatas = uow.RemoteServiceAuthDataRepository.FindList(ad => ad.Provider.Name == "Google" && ad.UserID == curUserId).ToArray();
        //        var googleAuthData = googleAuthDatas.FirstOrDefault(ad => ad.HasAccessToken());
        //        curManageUserVM.HasGoogleToken = googleAuthData != null;
        //        if (googleAuthData != null)
        //        {
        //            var spreadsheet = ObjectFactory.GetInstance<GoogleSheet>();
        //            curManageUserVM.GoogleSpreadsheets = spreadsheet.EnumerateSpreadsheetsUris(curUserId);
        //        }
        //        return View(curManageUserVM);
        //    }
        //}

        //[HttpPost]
        //public ActionResult ExportGoogleSpreadsheet(string spreadsheetUri)
        //{
        //    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        var curUserId = this.GetUserId();
        //        var curUserDO = uow.UserRepository.GetByKey(curUserId);
        //        if (curUserDO == null)
        //        {
        //            // if we found no user then assume that this user doesn't exists any more and force log off action.
        //            return RedirectToAction("LogOff", "DockyardAccount");
        //        }
        //        var googleAuthDatas = uow.RemoteServiceAuthDataRepository.FindList(ad => ad.Provider.Name == "Google" && ad.UserID == curUserId).ToArray();
        //        var googleAuthData = googleAuthDatas.FirstOrDefault(ad => ad.HasAccessToken());
        //        if (googleAuthData == null)
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Google authorization info");
        //        var spreadsheet = ObjectFactory.GetInstance<GoogleSheet>();
        //        var fileUrl = spreadsheet.ExtractData(spreadsheetUri, curUserId);
        //        Logger.GetLogger().InfoFormat("Google Spreadsheet '{0}' exported to '{1}'", spreadsheetUri, fileUrl);
        //        return RedirectToAction("RemoteServices");
        //    }
        //}

        //public ActionResult LearnHowToUseKwasant()
        //{
        //    return View();
        //}

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