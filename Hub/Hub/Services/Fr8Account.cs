using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
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
using System.Web;
using System.Net.Http;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Utilities;
using Hub.Interfaces;
using Fr8.Infrastructure.Data.States;
using Hub.Enums;

namespace Hub.Services
{
    public class Fr8Account : IFr8Account
    {
        private readonly IUnitOfWorkFactory _uowFactory;

        private readonly IConfigRepository _configRepository;

        public Fr8Account(IUnitOfWorkFactory uowFactory, IConfigRepository configRepository)
        {
            if (uowFactory == null)
            {
                throw new ArgumentNullException(nameof(uowFactory));
            }
            if (configRepository == null)
            {
                throw new ArgumentNullException(nameof(configRepository));
            }
            _uowFactory = uowFactory;
            _configRepository = configRepository;
        }

        public void UpdatePassword(IUnitOfWork uow, Fr8AccountDO dockyardAccountDO, string password)
        {
            if (dockyardAccountDO != null)
            {
                uow.UserRepository.UpdateUserCredentials(dockyardAccountDO, password: password);
            }
        }

        public bool IsValidHashedPassword(Fr8AccountDO dockyardAccountDO, string password)
        {
            if (dockyardAccountDO != null)
            {
                var passwordHasher = new PasswordHasher();
                return (passwordHasher.VerifyHashedPassword(dockyardAccountDO.PasswordHash, password) ==
                    PasswordVerificationResult.Success);
            }
            else
                return false;
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

            String[] acceptableRoles = { };
            switch (minAuthLevel)
            {
                case "StandardUser":
                    acceptableRoles = new[] { "StandardUser", "Admin" };
                    break;
                case "Admin":
                    acceptableRoles = new[] { "Admin" };
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
        public string GetDisplayName(Fr8AccountDO curDockyardAccount)
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

            RegexUtilities.ValidateEmailAddress(_configRepository, curEmailAddress.Address);
            return curEmailAddress.Address.Split(new[] { '@' })[0];
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
            Fr8AccountDO existingDockyardAccount = uow.UserRepository.GetQuery().FirstOrDefault(e => e.EmailAddress.Address == emailAddress);
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

        public Fr8AccountDO GetSystemUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.UserRepository.GetQuery().Include(x => x.EmailAddress).FirstOrDefault(x => x.SystemAccount);
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
                    (r => r.State == State.Executing
                          && r.Plan.Fr8Account.Id == userId).ToList();
            }
        }

        /// <summary>
        /// Register account
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="organizationDO">organization where the user belongs</param>
        /// <param name="isNewOrganization">In case of new created organization, make user admin of that organization</param>
        /// <param name="anonimousId"></param>
        /// <returns></returns>
        public RegistrationStatus ProcessRegistrationRequest(IUnitOfWork uow, string email, string password, OrganizationDO organizationDO, bool isNewOrganization, string anonimousId)
        {
            RegistrationStatus curRegStatus;
            Fr8AccountDO newFr8Account = null;
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
                        UpdatePassword(uow, existingUserDO, password);
                        existingUserDO.Organization = organizationDO;

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
                    newFr8Account = Register(uow, email, email, email, password, Roles.StandardUser, organizationDO);
                    curRegStatus = RegistrationStatus.Successful;
                }
            }
            else
            {
                newFr8Account = Register(uow, email, email, email, password, Roles.StandardUser, organizationDO);
                curRegStatus = RegistrationStatus.Successful;
            }

            if (newFr8Account != null)
            {
                if (organizationDO != null)
                {
                    AssingRolesAndProfilesBasedOnOrganization(uow, newFr8Account, organizationDO.Name, isNewOrganization);
                }
                else
                {
                    AssignProfileToUser(uow, newFr8Account, DefaultProfiles.StandardUser);
                }
            }

            uow.SaveChanges();

            if (newFr8Account != null)
            {
                EventManager.UserRegistration(newFr8Account);
                ObjectFactory.GetInstance<ITracker>().Registered(anonimousId, newFr8Account);
            }

            return curRegStatus;
        }

        public Task<Tuple<LoginStatus, string>> ProcessLoginRequest(string username, string password, bool isPersistent, HttpRequestMessage request = null)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                LoginStatus curLoginStatus;
                string userId;
                Fr8AccountDO fr8AccountDO = uow.UserRepository.FindOne(x => x.UserName == username);
                if (fr8AccountDO != null)
                {
                    userId = fr8AccountDO.Id;
                    if (string.IsNullOrEmpty(fr8AccountDO.PasswordHash))
                    {
                        curLoginStatus = LoginStatus.ImplicitUser;
                    }
                    else
                    {
                        curLoginStatus = Login(uow, fr8AccountDO, password, isPersistent);
                    }
                }
                else
                {
                    curLoginStatus = LoginStatus.UnregisteredUser;
                    userId = "";
                }
                return Task.FromResult(Tuple.Create(curLoginStatus, userId));
            }
        }

        public Fr8AccountDO Register(IUnitOfWork uow, string userName, string firstName, string lastName,
            string password, string roleID, OrganizationDO organizationDO = null)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName, roleID, organizationDO);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);

            //assign OwnerOfCurrentObject role to user
            uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.OwnerOfCurrentObject, userDO.Id);

            return userDO;
        }

        private void AssingRolesAndProfilesBasedOnOrganization(IUnitOfWork uow, Fr8AccountDO newFr8Account, string organizationName, bool isNewOrganization)
        {
            //in case when the new user is the one that created this new organization, add to user role as admin of new organization
            if (isNewOrganization)
            {
                var orgAdminRoleName = Organization.AdminOfOrganizationRoleName(organizationName);
                uow.AspNetUserRolesRepository.AssignRoleToUser(orgAdminRoleName, newFr8Account.Id);
                //this user need to have system administrator profile assigned to him
                AssignProfileToUser(uow, newFr8Account, DefaultProfiles.SystemAdministrator);
            }
            else
            {
                //every new user that registers inside some organization must have role that is member of that organization
                var orgMemberRoleName = Organization.MemberOfOrganizationRoleName(organizationName);
                uow.AspNetUserRolesRepository.AssignRoleToUser(orgMemberRoleName, newFr8Account.Id);
                //this user need to have standard user profile assigned to him
                AssignProfileToUser(uow, newFr8Account, DefaultProfiles.StandardUser);
            }
        }

        private void AssignProfileToUser(IUnitOfWork uow, Fr8AccountDO fr8Account, string profileName)
        {
            var profile = uow.ProfileRepository.GetQuery().FirstOrDefault(x => x.Name == profileName);
            if (profile != null)
            {
                fr8Account.ProfileId = profile.Id;
            }
            else
            {
                throw new ApplicationException($"Profile '{profileName}' not found for User");
            }
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
                string htmlText = string.Format("Please reset your password by clicking this <a href='{0}'>link:</a> <br> <b>Note: </b> Reset password link will be expired after 24 hours.", callbackUrl);
                emailDO.HTMLText = htmlText;

                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, configRepository.Get("ForgotPassword_template"),
                    new Dictionary<string, object>() { { "-callback_url-", callbackUrl } });
                uow.SaveChanges();

                await ObjectFactory.GetInstance<IEmailPackager>().Send(new EnvelopeDO { Email = emailDO });
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
                else if (getRoles.Select(e => e.Name).Contains("StandardUser"))
                    userRole = "StandardUser";
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
            IEnumerable<PlanDO> activePlans;
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planQuery = unitOfWork.PlanRepository.GetPlanQueryUncached().Include(i => i.Fr8Account);

                planQuery
                    .Where(pt => pt.PlanState == PlanState.Executing ||
                                 pt.PlanState == PlanState.Active)//1.
                    .Where(id => id.Fr8Account.Id == userId);//2

                activePlans = planQuery.ToList();
            }
            return activePlans;
        }

        public Task<Tuple<LoginStatus, string>> CreateAuthenticateGuestUser()
        {
            Guid guid = Guid.NewGuid();
            string guestUserEmail = guid + "@fr8.co";
            string guestUserPassword = "fr8123";
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // Register a guest user 
                Fr8AccountDO fr8AccountDO = Register(uow, guestUserEmail, guestUserEmail, guestUserEmail, guestUserPassword, Roles.Guest);
                uow.SaveChanges();
                var loginStatus = Login(uow, fr8AccountDO, guestUserPassword, false);
                var userId = fr8AccountDO.Id;
                return Task.FromResult(Tuple.Create(loginStatus, userId));
            }
        }

        /// <summary>
        /// Register Guest account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="tempEmail"></param>
        /// <param name="organizationDO">Name of organization where the user belongs to</param>
        /// <returns></returns>
        public Task<RegistrationStatus> UpdateGuestUserRegistration(IUnitOfWork uow, string email, string password, string tempEmail, OrganizationDO organizationDO = null)
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

                // update organization
                if (organizationDO != null)
                {
                    existingUserDO.Organization = organizationDO;
                }

                guestUserexistingEmailAddressDO.Address = email;

                uow.AspNetUserRolesRepository.RevokeRoleFromUser(Roles.Guest, existingUserDO.Id);
                // Add new role
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.StandardUser, existingUserDO.Id);
            }

            uow.SaveChanges();
            return Task.FromResult(curRegStatus);
        }

        public bool IsCurrentUserInAdminRole()
        {
            ISecurityServices _security = ObjectFactory.GetInstance<ISecurityServices>();
            bool isAdmin = false;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //get the current account
                var curAccount = _security.GetCurrentAccount(uow);
                //get the roles to check if the account has admin role
                var curAccountRoles = curAccount.Roles;
                //get the role id
                var adminRoleId = uow.AspNetRolesRepository.GetQuery().Single(r => r.Name == Roles.Admin).Id;
                //provide all facts if the user has admin role
                if (curAccountRoles.Any(x => x.RoleId == adminRoleId))
                {
                    isAdmin = true;
                }
            }

            return isAdmin;
        }

        /// <summary>
        /// Check if there is in existence any Admin user account
        /// </summary>
        /// <returns></returns>
        public bool CheckForExistingAdminUsers()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var adminRoleId = uow.AspNetUserRolesRepository.GetRoleID(Roles.Admin);
                return uow.AspNetUserRolesRepository.GetQuery().Any(x => x.RoleId == adminRoleId);
            }
        }

        /// <summary>
        /// Create new Master Admin Account from the Wizard page
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="curPassword"></param>
        /// <returns></returns>
        public async Task CreateAdminAccount(string userEmail, string curPassword)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var newFr8Account = uow.UserRepository.GetOrCreateUser(userEmail);
                uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, newFr8Account.Id);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.StandardUser, newFr8Account.Id);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.OwnerOfCurrentObject, newFr8Account.Id);
                //this master admin account will also be system account
                newFr8Account.SystemAccount = true;
                if (newFr8Account != null)
                {
                    AssignProfileToUser(uow, newFr8Account, DefaultProfiles.Fr8Administrator);
                }

                uow.SaveChanges();

                if (newFr8Account != null)
                {
                    EventManager.UserRegistration(newFr8Account);
                }

                // need to generate ManifestDescription under new System User Account
                var generator = ObjectFactory.GetInstance<IManifestPageGenerator>();
                var existingManifestDescriptions = uow.MultiTenantObjectRepository.Query<ManifestDescriptionCM>("system1@fr8.co", x => true);
                foreach (var item in existingManifestDescriptions)
                {
                    uow.MultiTenantObjectRepository.AddOrUpdate(newFr8Account.UserName, item);
                }

                uow.SaveChanges();
                
                var generateTasks = uow.MultiTenantObjectRepository.Query<ManifestDescriptionCM>(newFr8Account.UserName, x => true).Select(x => x.Name).Distinct().Select(manifestName => generator.Generate(manifestName, GenerateMode.GenerateAlways)).Cast<Task>().ToList();
                await Task.WhenAll(generateTasks);
                //EnsureMThasaDocuSignRecipientCMTypeStored
                var type = uow.MultiTenantObjectRepository.FindTypeReference(typeof(DocuSignRecipientCM));
                if (type == null)
                {
                    uow.MultiTenantObjectRepository.Add(new DocuSignRecipientCM() { RecipientId = Guid.NewGuid().ToString() }, newFr8Account.Id);
                    uow.SaveChanges();
                }
            }
        }
    }
}