using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hub.Managers.APIManagers.Packagers;
using Microsoft.AspNet.Identity;
using StructureMap;
using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Hub.Security;
using Utilities;
using System.Web;

namespace Hub.Services
{
    public class Fr8Account
    {

        public void UpdatePassword(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO, string password)
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
        public CommunicationMode GetMode(Fr8AccountDO dockyardAccountDO)
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
        public static string GetDisplayName(Fr8AccountDO curDockyardAccount)
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

        public void Create(IUnitOfWork uow, Fr8AccountDO submittedDockyardAccountData)
        {
            submittedDockyardAccountData.State = UserState.Active;
            submittedDockyardAccountData.Id = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(submittedDockyardAccountData.UserName))
            {
                submittedDockyardAccountData.UserName = submittedDockyardAccountData.EmailAddress != null
                    ? submittedDockyardAccountData.EmailAddress.Address
                    : null;
            }
            if (string.IsNullOrEmpty(submittedDockyardAccountData.UserName))
            {
                throw new ApplicationException("User must have username or email address");
            }
            submittedDockyardAccountData.EmailAddress =
                uow.EmailAddressRepository.GetOrCreateEmailAddress(submittedDockyardAccountData.EmailAddress.Address);
            submittedDockyardAccountData.Roles.ToList().ForEach(e =>
                uow.AspNetUserRolesRepository.Add(new AspNetUserRolesDO
                {
                    RoleId = e.RoleId,
                    UserId = submittedDockyardAccountData.Id
                }));
            submittedDockyardAccountData.Roles.Clear();
            var userManager = new DockyardIdentityManager(uow);
            var result = userManager.Create(submittedDockyardAccountData);
            if (!result.Succeeded)
                throw new AggregateException(result.Errors.Select(s => new ApplicationException(s)));
            uow.SaveChanges();
            EventManager.ExplicitCustomerCreated(submittedDockyardAccountData.Id);
        }

        public Fr8AccountDO GetExisting(IUnitOfWork uow, string emailAddress)
        {
            Fr8AccountDO existingDockyardAccount =
                uow.UserRepository.GetQuery().Where(e => e.EmailAddress.Address == emailAddress).FirstOrDefault();
            return existingDockyardAccount;
        }

        public void Update(IUnitOfWork uow, Fr8AccountDO submittedDockyardAccountData,
            Fr8AccountDO existingDockyardAccount)
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
        public IEnumerable<ContainerDO> GetContainerList(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ContainerRepository.GetQuery().Where
                    (r => r.ContainerState == ContainerState.Executing
                          & r.Plan.Fr8Account.Id == userId).ToList();
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
                Fr8AccountDO newDockyardAccountDO = null;
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
                            new Fr8Account().UpdatePassword(uow, existingUserDO, password);
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

        public Task<LoginStatus> ProcessLoginRequest(string username, string password, bool isPersistent)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                LoginStatus curLoginStatus;

                Fr8AccountDO dockyardAccountDO = uow.UserRepository.FindOne(x => x.UserName == username);
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

                return Task.FromResult(curLoginStatus);
            }
        }

        public Fr8AccountDO Register(IUnitOfWork uow, string userName, string firstName, string lastName,
            string password, string roleID)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);
            return userDO;
        }

        public LoginStatus Login(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO, string password,
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
                code = HttpUtility.HtmlEncode(code);

                var callbackUrl = string.Format("{0}DockyardAccount/ResetPassword?UserId={1}&code={2}", Server.ServerUrl,
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
                string htmlText = string.Format("Please reset your password by clicking this <a href='{0}'>link:</a> <br> <b>Note: </b> Reset password link will be expired after 24 hours.", callbackUrl);
                emailDO.HTMLText = htmlText;

                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, configRepository.Get("ForgotPassword_template"),
                    new Dictionary<string, object>() {{"-callback_url-", callbackUrl}});
                uow.SaveChanges();
                
               await ObjectFactory.GetInstance<IEmailPackager>().Send(new EnvelopeDO {Email = emailDO});
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

		  //public DocuSignAccount LoginToDocuSign()
		  //{
		  //	 var packager = new DocuSignPackager();
		  //	 return packager.Login();
		  //}

        public IEnumerable<PlanDO> GetActivePlans(string userId)
        {
            IEnumerable<PlanDO> activeRoutes;
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var routeQuery = unitOfWork.PlanRepository.GetQuery().Include(i => i.Fr8Account);

                routeQuery
                    .Where(pt => pt.RouteState == RouteState.Active)//1.
                    .Where(id => id.Fr8Account.Id == userId);//2

                activeRoutes = routeQuery.ToList();
            }
            return activeRoutes;
        }

        public Task<LoginStatus> CreateAuthenticateGuestUser()
        {
            Guid guid = Guid.NewGuid();
            string guestUserEmail = guid + "@fr8.co";
            string guestUserPassword = "fr8123";
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // Register a guest user 
               Fr8AccountDO fr8AccountDO = Register(uow, guestUserEmail, guestUserEmail, guestUserEmail, guestUserPassword, Roles.Guest);
               uow.SaveChanges();

               // By default #Register adds Customer role for the user so remove it 
               uow.AspNetUserRolesRepository.RevokeRoleFromUser(Roles.Customer, fr8AccountDO.Id);
               uow.SaveChanges();

               return Task.FromResult(Login(uow, fr8AccountDO, guestUserPassword, false));
            }
        }

        /// <summary>
        /// Register Guest account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="tempEmail"></param>
        /// <returns></returns>
        public Task<RegistrationStatus> UpdateGuestUserRegistration(String email, String password, String tempEmail)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                RegistrationStatus curRegStatus = RegistrationStatus.Successful;
                Fr8AccountDO newDockyardAccountDO = null;


                EmailAddressDO existingEmailAddressDO =
                   uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == email);
                if (existingEmailAddressDO != null)
                {
                    curRegStatus = RegistrationStatus.UserMustLogIn;
                    return Task.FromResult(curRegStatus);
                }
                

                //check if we know this email address

                EmailAddressDO guestUserexistingEmailAddressDO =
                    uow.EmailAddressRepository.GetQuery().FirstOrDefault(ea => ea.Address == tempEmail);
                if (guestUserexistingEmailAddressDO != null)
                {
                    var existingUserDO =
                        uow.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddressID == guestUserexistingEmailAddressDO.Id);
                    
                    // Update Email
                    uow.UserRepository.UpdateUserCredentials(existingUserDO, email, password);
                    guestUserexistingEmailAddressDO.Address = email;

                    uow.AspNetUserRolesRepository.RevokeRoleFromUser(Roles.Guest, existingUserDO.Id);
                    // Add new role
                    uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, existingUserDO.Id);
                }

                uow.SaveChanges();
                return Task.FromResult(curRegStatus);
            }
        }
    }
}