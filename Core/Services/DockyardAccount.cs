using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Security;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Data.Wrappers;
using Microsoft.AspNet.Identity;
using StructureMap;
using System.Data.Entity;
using Utilities;

namespace Core.Services
{
    public class DockyardAccount
    {

        public void UpdatePassword(IUnitOfWork uow, DockyardAccountDO dockyardAccountDO, string password)
        {
            if (dockyardAccountDO != null)
            {
                uow.UserRepository.UpdateUserCredentials(dockyardAccountDO, password: password);
            }
        }

        /// <summary>
        /// Determines <see cref="CommunicationMode">communication mode</see> for user
        /// </summary>
        /// <param name="dockyardAccountDO">DockYardAccount</param>
        /// <returns>Direct if the user has a booking request or a password. Otherwise, Delegate.</returns>
        public CommunicationMode GetMode(DockyardAccountDO dockyardAccountDO)
        {
            //if (userDO.UserBookingRequests != null && userDO.UserBookingRequests.Any())
            //    return CommunicationMode.Direct;
            if (!String.IsNullOrEmpty(dockyardAccountDO.PasswordHash))
                return CommunicationMode.Direct;
            return CommunicationMode.Delegate;
        }

        //
        //get roles for this DockYardAccount
        //if at least one role meets or exceeds the provided level, return true, else false
        public bool VerifyMinimumRole(string minAuthLevel, string curUserId, IUnitOfWork uow)
        {
            var roleIds =
                uow.AspNetUserRolesRepository.GetQuery()
                    .Where(ur => ur.UserId == curUserId)
                    .Select(ur => ur.RoleId)
                    .ToList();
            var roles =
                uow.AspNetRolesRepository.GetQuery().Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();

            String[] acceptableRoles = {};
            switch (minAuthLevel)
            {
                case "Customer":
                    acceptableRoles = new[] {"Customer", "Booker", "Admin"};
                    break;
                case "Booker":
                    acceptableRoles = new[] {"Booker", "Admin"};
                    break;
                case "Admin":
                    acceptableRoles = new[] {"Admin"};
                    break;
            }
            //if any of the roles that this user belongs to are contained in the current set of acceptable roles, return true
            if (roles.Any(role => acceptableRoles.Contains(role)))
                return true;
            return false;
        }

        //public void Create(IUnitOfWork uow, UserDO submittedUserData, string role, bool sendEmail)
        //{
        //    if (sendEmail)
        //    {
        //	  new Email().SendUserSettingsNotification(uow, submittedUserData);
        //    }
        //    new Account().Register(uow, submittedUserData.EmailAddress.Address, submittedUserData.FirstName, submittedUserData.LastName, "test@1234", role);
        //}

        //if we have a first name and last name, use them together
        //else if we have a first name only, use that
        //else if we have just an email address, use the portion preceding the @ unless there's a name
        //else throw
        public static string GetDisplayName(DockyardAccountDO curDockyardAccount)
        {
            string firstName = curDockyardAccount.FirstName;
            string lastName = curDockyardAccount.LastName;
            if (firstName != null)
            {
                if (lastName == null)
                    return firstName;

                return firstName + " " + lastName;
            }

            EmailAddressDO curEmailAddress = curDockyardAccount.EmailAddress;
            if (curEmailAddress.Name != null)
                return curEmailAddress.Name;

            RegexUtilities.ValidateEmailAddress(curEmailAddress.Address);
            return curEmailAddress.Address.Split(new[] {'@'})[0];
        }

        public void Create(IUnitOfWork uow, DockyardAccountDO submittedDockyardAccountData)
        {
            submittedDockyardAccountData.State = UserState.Active;
            submittedDockyardAccountData.Id = Guid.NewGuid().ToString();
            submittedDockyardAccountData.UserName = submittedDockyardAccountData.FirstName;
            submittedDockyardAccountData.EmailAddress =
                uow.EmailAddressRepository.GetOrCreateEmailAddress(submittedDockyardAccountData.EmailAddress.Address);
            submittedDockyardAccountData.Roles.ToList().ForEach(e =>
                uow.AspNetUserRolesRepository.Add(new AspNetUserRolesDO
                {
                    RoleId = e.RoleId,
                    UserId = submittedDockyardAccountData.Id
                }));
            submittedDockyardAccountData.Roles.Clear();
            uow.UserRepository.Add(submittedDockyardAccountData);
            uow.SaveChanges();
            EventManager.ExplicitCustomerCreated(submittedDockyardAccountData.Id);
        }

        public DockyardAccountDO GetExisting(IUnitOfWork uow, string emailAddress)
        {
            DockyardAccountDO existingDockyardAccount =
                uow.UserRepository.GetQuery().Where(e => e.EmailAddress.Address == emailAddress).FirstOrDefault();
            return existingDockyardAccount;
        }

        public void Update(IUnitOfWork uow, DockyardAccountDO submittedDockyardAccountData,
            DockyardAccountDO existingDockyardAccount)
        {
            existingDockyardAccount.FirstName = submittedDockyardAccountData.FirstName;
            existingDockyardAccount.LastName = submittedDockyardAccountData.LastName;

            //Remove old roles
            foreach (var existingRole in existingDockyardAccount.Roles.ToList())
            {
                if (!submittedDockyardAccountData.Roles.Select(role => role.RoleId).Contains(existingRole.RoleId))
                    uow.AspNetUserRolesRepository.Remove(
                        uow.AspNetUserRolesRepository.FindOne(
                            e => e.RoleId == existingRole.RoleId && e.UserId == existingDockyardAccount.Id));
            }

            //Add new roles
            foreach (var newRole in submittedDockyardAccountData.Roles)
            {
                if (!existingDockyardAccount.Roles.Select(role => role.RoleId).Contains(newRole.RoleId))
                    uow.AspNetUserRolesRepository.Add(new AspNetUserRolesDO
                    {
                        RoleId = newRole.RoleId,
                        UserId = submittedDockyardAccountData.Id
                    });
            }
            uow.SaveChanges();
        }

        public string GetUserId(string emailAddress)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.UserRepository.GetOrCreateUser(emailAddress).Id;
            }
        }

        /// <summary>
        /// Returns the list of all processes to run for the specified user.
        /// </summary>
        public IEnumerable<ProcessDO> GetProcessList(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ProcessRepository.GetQuery().Where
                    (r => r.ProcessState == ProcessState.Executing
                          & r.DockyardAccountId == userId).ToList();
            }
        }

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
                DockyardAccountDO newDockyardAccountDO = null;
                //check if we know this email address

                EmailAddressDO existingEmailAddressDO =
                    uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == email);
                if (existingEmailAddressDO != null)
                {
                    var existingUserDO =
                        uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddressID == existingEmailAddressDO.Id);
                    if (existingUserDO != null)
                    {
                        if (existingUserDO.PasswordHash == null)
                        {
                            //this is an existing implicit user, who sent in a request in the past, had a DockyardAccountDO created, and now is registering. Add the password
                            new DockyardAccount().UpdatePassword(uow, existingUserDO, password);
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
                        newDockyardAccountDO = Register(uow, email, email, email, password, Roles.Customer);
                        curRegStatus = RegistrationStatus.Successful;
                    }
                }
                else
                {
                    newDockyardAccountDO = Register(uow, email, email, email, password, Roles.Customer);
                    curRegStatus = RegistrationStatus.Successful;
                }

                uow.SaveChanges();

                if (newDockyardAccountDO != null)
                {
                    //AlertManager.CustomerCreated(newDockyardAccountDO);
                    EventManager.UserRegistration(newDockyardAccountDO);
                }

                return curRegStatus;
            }
        }

        public async Task<LoginStatus> ProcessLoginRequest(string username, string password, bool isPersistent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                LoginStatus curLoginStatus;

                DockyardAccountDO dockyardAccountDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (dockyardAccountDO != null)
                {
                    if (string.IsNullOrEmpty(dockyardAccountDO.PasswordHash))
                    {
                        curLoginStatus = LoginStatus.ImplicitUser;
                    }
                    else
                    {
                        curLoginStatus = Login(uow, dockyardAccountDO, password, isPersistent);
                    }
                }
                else
                {
                    curLoginStatus = LoginStatus.UnregisteredUser;
                }

                return curLoginStatus;
            }
        }

        public DockyardAccountDO Register(IUnitOfWork uow, string userName, string firstName, string lastName,
            string password, string roleID)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);
            return userDO;
        }

        public LoginStatus Login(IUnitOfWork uow, DockyardAccountDO dockyardAccountDO, string password,
            bool isPersistent)
        {
            var curLogingStatus = LoginStatus.Successful;

            var passwordHasher = new PasswordHasher();
            if (passwordHasher.VerifyHashedPassword(dockyardAccountDO.PasswordHash, password) ==
                PasswordVerificationResult.Success)
            {
                var securityServices = ObjectFactory.GetInstance<ISecurityServices>();
                securityServices.Logout();
                securityServices.Login(uow, dockyardAccountDO);
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
                var userManager = new DockyardIdentityManager(uow);
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null /* || !(await userManager.IsEmailConfirmedAsync(user.Id))*/)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return;
                }

                var code = await userManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = string.Format("{0}Account/ResetPassword?UserId={1}&code={2}", Server.ServerUrl,
                    user.Id, code);

                var emailDO = new EmailDO();
                IConfigRepository configRepository = ObjectFactory.GetInstance<IConfigRepository>();
                string fromAddress = configRepository.Get("EmailAddress_GeneralInfo");
                var emailAddressDO = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress);
                emailDO.From = emailAddressDO;
                emailDO.FromID = emailAddressDO.Id;
                emailDO.AddEmailRecipient(EmailParticipantType.To,
                    uow.EmailAddressRepository.GetOrCreateEmailAddress(userEmail));
                emailDO.Subject = "Password Recovery Request";

                //uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, configRepository.Get("ForgotPassword_template"),
                //  new Dictionary<string, object>()
                // {{"-callback_url-", callbackUrl}});
                uow.SaveChanges();
            }
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string code, string password)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var userManager = new DockyardIdentityManager(uow);
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

        public DocuSignAccount LoginToDocuSign()
        {
            var packager = new DocuSignPackager();
            return packager.Login();
        }

        public IEnumerable<ProcessTemplateDO> GetActiveProcessTemplates(string userId)
        {
            IEnumerable<ProcessTemplateDO> subscribingProcessTemplates;
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplateQuery = unitOfWork.ProcessTemplateRepository.GetQuery().Include(i => i.ProcessNodeTemplates).Include(i => i.DockyardAccount);

                processTemplateQuery
                    .Where(pt => pt.ProcessTemplateState == ProcessTemplateState.Active)//1.
                    .Where(id => id.DockyardAccount.Id == userId);//2

                subscribingProcessTemplates = processTemplateQuery.ToList();
            }
            return subscribingProcessTemplates;
        }
    }
}