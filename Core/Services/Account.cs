using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Core.Security;
using Microsoft.AspNet.Identity;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace Core.Services
{
    public class Account
    {
        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public RegistrationStatus ProcessRegistrationRequest(String email, String password)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RegistrationStatus curRegStatus;
                UserDO newUserDO = null;
                //check if we know this email address

                EmailAddressDO existingEmailAddressDO = uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == email);
                if (existingEmailAddressDO != null)
                {
                    var existingUserDO = uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddressID == existingEmailAddressDO.Id);
                    if (existingUserDO != null)
                    {
                        if (existingUserDO.PasswordHash == null)
                        {
                            //this is an existing implicit user, who sent in a request in the past, had a UserDO created, and now is registering. Add the password
                            new User().UpdatePassword(uow, existingUserDO, password);
                            curRegStatus = RegistrationStatus.Successful;
                        }
                        else
                        {
                            //tell 'em to login
                            curRegStatus = RegistrationStatus.UserMustLogIn;
                        }
                    }
                    else
                    {
                        newUserDO = Register(uow, email, email, email, password, Roles.Customer);
                        curRegStatus = RegistrationStatus.Successful;
                    }
                }
                else
                {
                    newUserDO = Register(uow, email, email, email, password, Roles.Customer);
                    curRegStatus = RegistrationStatus.Successful;
                }

                uow.SaveChanges();

                if (newUserDO != null)
                {
                    //AlertManager.CustomerCreated(newUserDO);
                    AlertManager.UserRegistration(newUserDO);
                }
               
                return curRegStatus;
            }
        }

        public async Task<LoginStatus> ProcessLoginRequest(string username, string password, bool isPersistent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                LoginStatus curLoginStatus;

                UserDO userDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (userDO != null)
                {
                    if (string.IsNullOrEmpty(userDO.PasswordHash))
                    {
                        curLoginStatus = LoginStatus.ImplicitUser;
                    }
                    else
                    {
                        curLoginStatus = Login(uow, userDO, password, isPersistent);
                    }
                }
                else
                {
                    curLoginStatus = LoginStatus.UnregisteredUser;
                }

                return curLoginStatus;
            }
        }

        public UserDO Register(IUnitOfWork uow, string userName, string firstName, string lastName, string password, string roleID)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);
            return userDO;
        }

        public LoginStatus Login(IUnitOfWork uow, UserDO userDO, string password, bool isPersistent)
        {
            LoginStatus curLogingStatus = LoginStatus.Successful;

            var passwordHasher = new PasswordHasher();
            if (passwordHasher.VerifyHashedPassword(userDO.PasswordHash, password) == PasswordVerificationResult.Success)
            {
                var securityServices = ObjectFactory.GetInstance<ISecurityServices>();
                securityServices.Logout();
                securityServices.Login(uow, userDO);
            }
            else
            {
                curLogingStatus = LoginStatus.InvalidCredential;
            }

            return curLogingStatus;
        }

        public async Task ForgotPasswordAsync(string userEmail)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userManager = new KwasantUserManager(uow);
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null/* || !(await userManager.IsEmailConfirmedAsync(user.Id))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return;
                }

                var code = await userManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = string.Format("{0}Account/ResetPassword?UserId={1}&code={2}", Server.ServerUrl, user.Id, code);

                var emailDO = new EmailDO();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");
                var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
                emailDO.From = emailAddressDO;
                emailDO.FromID = emailAddressDO.Id;
                emailDO.AddEmailRecipient(EmailParticipantType.To, uow.EmailAddressRepository.GetOrCreateEmailAddress(userEmail));
                emailDO.Subject = "Password Recovery Request";

                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, configRepository.Get("ForgotPassword_template"),
                                                               new Dictionary<string, object>()
                                                                   {{"-callback_url-", callbackUrl}});
                uow.SaveChanges();
            }
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string code, string password)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userManager = new KwasantUserManager(uow);
                var result = await userManager.ResetPasswordAsync(userId, code, password);
                uow.SaveChanges();
                return result;
            }
        }

        public string GetUserRole(string userName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var user = uow.UserRepository.GetQuery().FirstOrDefault(u => u.UserName == userName);
                var getRoles = uow.AspNetUserRolesRepository.GetRoles(user.Id).ToList();

                string userRole = "";
                if (getRoles.Select(e => e.Name).Contains("Admin"))
                    userRole = "Admin";
                else if (getRoles.Select(e => e.Name).Contains("Booker"))
                    userRole = "Booker";
                else if (getRoles.Select(e => e.Name).Contains("Customer"))
                    userRole = "Customer";
                return userRole;
            }
        }
    }
}