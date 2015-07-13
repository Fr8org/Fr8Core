using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Core.Managers;
using Core.Services;
using Web.ViewModels;
using Microsoft.AspNet.Identity;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace Web.Controllers
{
    /// <summary>
    /// Email service
    /// </summary>
    public class KwasantEmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                String senderMailAddress = ObjectFactory.GetInstance<IConfigRepository>().Get("EmailFromAddress_DirectMode");

                EmailDO emailDO = new EmailDO();
                emailDO.AddEmailRecipient(EmailParticipantType.To, Email.GenerateEmailAddress(uow, new MailAddress(message.Destination)));
                emailDO.From = Email.GenerateEmailAddress(uow, new MailAddress(senderMailAddress));

                emailDO.Subject = message.Subject;
                emailDO.HTMLText = message.Body;

                uow.EnvelopeRepository.ConfigurePlainEmail(emailDO);
                uow.SaveChanges();
            }
        }
    }

    [KwasantAuthorize]
    public class AccountController : Controller
    {
        private readonly Account _account;

        public AccountController()
        {
            _account = ObjectFactory.GetInstance<Account>();
        }

        [AllowAnonymous]
        public ActionResult InterceptLogin(string returnUrl, string urlReferrer)
        {
            ViewBag.ReturnUrl = returnUrl;
            TempData["urlReferrer"] = urlReferrer;
            if (this.UserIsAuthenticated())
                throw new HttpException(403, "We're sorry, but you don't have permission to access this page.");
            return View("Index");
        }

        [AllowAnonymous]
        public ActionResult AccessDenied(string errorMessage)
        {
            ViewBag.ErrorMessage = errorMessage;
            ViewBag.UrlReferrer = TempData["urlReferrer"];
            return View();
        }

        [AllowAnonymous]
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (this.UserIsAuthenticated())
                return RedirectToAction("MyAccount", "User");
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RegistrationSuccessful()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult LogOff()
        {
            this.Logout();
            return RedirectToAction("Index", "Account");
        }

        [AllowAnonymous]
        public ActionResult Confirm(RegisterVM model)
        {
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    RegistrationStatus curRegStatus = _account.ProcessRegistrationRequest(model.Email.Trim(), model.Password.Trim());
                    if (curRegStatus == RegistrationStatus.UserMustLogIn)
                    {
                        ModelState.AddModelError("", @"You are already registered with us. Please login.");
                    }
                    else
                    {
                        // return RedirectToAction("Index", "Home");
                        return View("RegistrationConfirmation");
                    }
                }
            }
            catch (ApplicationException appEx)
            {
                ModelState.AddModelError("", appEx.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginVM model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    string username = model.Email.Trim();
                    LoginStatus curLoginStatus = await new Account().ProcessLoginRequest(username, model.Password, model.RememberMe);
                    switch (curLoginStatus)
                    {
                        case LoginStatus.InvalidCredential:
                            ModelState.AddModelError("", @"Invalid Email id or Password.");
                            break;

                        case LoginStatus.ImplicitUser:
                            ModelState.AddModelError("", @"We already have a record of that email address, but No password exists for this Email id. 
Please register first.");
                            break;

                        case LoginStatus.UnregisteredUser:
                            ModelState.AddModelError("", @"We do not have a registered account associated with this email address. 
Please register first.");
                            break;

                        default:
                            if (curLoginStatus == LoginStatus.Successful)
                            {
                                if (!String.IsNullOrEmpty(returnUrl))
                                    return Redirect(returnUrl);

                                string getRole = _account.GetUserRole(username);

                                if (getRole == "Admin")
                                    return RedirectToAction("Index", "Admin");
                                else if (getRole == "Booker")
                                    return RedirectToAction("Index", "Booker");

                                return RedirectToAction("MyAccount", "User");
                            }
                            break;
                    }
                }
            }
            catch (ApplicationException appEx)
            {
                ModelState.AddModelError("", appEx.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            // If we got this far, something failed, redisplay form
            return View("Index", model);
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult ConfirmEmail(string userId, string code)
        {
            string returnViewName = "RegistrationSuccessful";
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    UserDO curUserDO = uow.UserRepository.FindOne(u => u.Id == userId);
                    if (curUserDO != null)
                    {
                        curUserDO.EmailConfirmed = true;
                        uow.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                returnViewName = "Register";
            }

            return RedirectToAction(returnViewName);
        }

        [System.Web.Http.HttpPost]
        public ActionResult Edit(UserVM usersAdminVM)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(usersAdminVM.Id);
                userDO.Id = usersAdminVM.Id;
                userDO.FirstName = usersAdminVM.FirstName;
                userDO.LastName = usersAdminVM.LastName;
                userDO.EmailAddress = new EmailAddressDO
                {
                    Id = usersAdminVM.EmailAddressID,
                    Address = usersAdminVM.EmailAddress
                };

                userDO.EmailAddressID = usersAdminVM.EmailAddressID;
                userDO.UserName = usersAdminVM.EmailAddress;

                // Set RoleId & UserId if role is changed on the font-end other wise IdentityUserRole is set to null and user's role will not be updated.
                //uow.AspNetUserRolesRepository.AssignRoleIDToUser(usersAdminVM.RoleId, usersAdminVM.Id);

                uow.SaveChanges();
                return RedirectToAction("Index", "User");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _account.ForgotPasswordAsync(model.Email);
                    return View("ForgotPasswordConfirmation", model);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error("ForgotPassword failed.", ex);
                    ModelState.AddModelError("", ex);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(string userId, string code)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userDO = uow.UserRepository.GetByKey(userId);
                if (userDO == null)
                    return HttpNotFound();
                return View(
                    new ResetPasswordVM()
                    {
                        UserId = userId,
                        Code = code,
                        Email = userDO.EmailAddress.Address
                    });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordVM viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _account.ResetPasswordAsync(viewModel.UserId, viewModel.Code, viewModel.Password);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordConfirmation", viewModel);
                    }
                    else
                    {
                        Array.ForEach(result.Errors.ToArray(), e => ModelState.AddModelError("", e));
                    }
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error("ResetPassword failed.", ex);
                    ModelState.AddModelError("", ex);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(viewModel);
        }
    }
}