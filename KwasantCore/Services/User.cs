using System;
using System.Linq;
using Data.Interfaces;
using Data.Validations;
using Data.Entities;
using Data.States;
using StructureMap;
using Data.Infrastructure;
using Utilities;

namespace KwasantCore.Services
{
    public class User
    {
               
        public void UpdatePassword(IUnitOfWork uow, UserDO userDO, string password)
        {
            if (userDO != null)
            {
                uow.UserRepository.UpdateUserCredentials(userDO, password: password);
            }
        }

        /// <summary>
        /// Determines <see cref="CommunicationMode">communication mode</see> for user
        /// </summary>
        /// <param name="userDO">User</param>
        /// <returns>Direct if the user has a booking request or a password. Otherwise, Delegate.</returns>
        public CommunicationMode GetMode(UserDO userDO)
        {
            if (userDO.UserBookingRequests != null && userDO.UserBookingRequests.Any())
                return CommunicationMode.Direct;
            if(!String.IsNullOrEmpty(userDO.PasswordHash))
                return CommunicationMode.Direct;
            return CommunicationMode.Delegate;
        }

        //
        //get roles for this User
        //if at least one role meets or exceeds the provided level, return true, else false
        public bool VerifyMinimumRole(string minAuthLevel, string curUserId, IUnitOfWork uow)
        {
            var roleIds = uow.AspNetUserRolesRepository.GetQuery().Where(ur => ur.UserId == curUserId).Select(ur => ur.RoleId).ToList();
            var roles = uow.AspNetRolesRepository.GetQuery().Where(r => roleIds.Contains(r.Id)).Select(r => r.Name).ToList();

            String[] acceptableRoles = { };
            switch (minAuthLevel)
            {
                case "Customer":
                    acceptableRoles = new[] { "Customer", "Booker", "Admin" };
                    break;
                case "Booker":
                    acceptableRoles = new[] { "Booker", "Admin" };
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

        public void Create(IUnitOfWork uow, UserDO submittedUserData, string role, bool sendEmail)
        {
            if (sendEmail)
            {
                new Email().SendUserSettingsNotification(uow, submittedUserData);
            }
            new Account().Register(uow, submittedUserData.EmailAddress.Address, submittedUserData.FirstName, submittedUserData.LastName, "test@1234", role);
        }

        //if we have a first name and last name, use them together
        //else if we have a first name only, use that
        //else if we have just an email address, use the portion preceding the @ unless there's a name
        //else throw
        public static string GetDisplayName(UserDO curUser)
        {
            string firstName = curUser.FirstName;
            string lastName = curUser.LastName;
            if (firstName != null)
            {
                if (lastName == null)
                    return firstName;

                return firstName + " " + lastName;
            }

            EmailAddressDO curEmailAddress = curUser.EmailAddress;
            if (curEmailAddress.Name != null)
                return curEmailAddress.Name;

            RegexUtilities.ValidateEmailAddress(curEmailAddress.Address);
            return curEmailAddress.Address.Split(new[] {'@'})[0];
        }

        public void Create(IUnitOfWork uow, UserDO submittedUserData)
        {
            submittedUserData.State = UserState.Active;
            submittedUserData.Id = Guid.NewGuid().ToString();
            submittedUserData.UserName = submittedUserData.FirstName;
            submittedUserData.EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(submittedUserData.EmailAddress.Address);
            submittedUserData.Roles.ToList().ForEach(e =>
                uow.AspNetUserRolesRepository.Add(new AspNetUserRolesDO
                {
                    RoleId = e.RoleId,
                    UserId = submittedUserData.Id
                }));
            submittedUserData.Roles.Clear();
            uow.UserRepository.Add(submittedUserData);
            uow.SaveChanges();
            AlertManager.ExplicitCustomerCreated(submittedUserData.Id);
        }

        public UserDO GetExisting(IUnitOfWork uow, string emailAddress)
        {
            UserDO existingUser = uow.UserRepository.GetQuery().Where(e => e.EmailAddress.Address == emailAddress).FirstOrDefault();
            return existingUser;
        }

        public void Update(IUnitOfWork uow, UserDO submittedUserData, UserDO existingUser)
        {
            existingUser.FirstName = submittedUserData.FirstName;
            existingUser.LastName = submittedUserData.LastName;

            //Remove old roles
            foreach (var existingRole in existingUser.Roles.ToList())
            {
                if (!submittedUserData.Roles.Select(role => role.RoleId).Contains(existingRole.RoleId))
                    uow.AspNetUserRolesRepository.Remove(uow.AspNetUserRolesRepository.FindOne(e => e.RoleId == existingRole.RoleId && e.UserId == existingUser.Id));
            }

            //Add new roles
            foreach (var newRole in submittedUserData.Roles)
            {
                if (!existingUser.Roles.Select(role => role.RoleId).Contains(newRole.RoleId))
                    uow.AspNetUserRolesRepository.Add(new AspNetUserRolesDO
                    {
                        RoleId = newRole.RoleId,
                        UserId = submittedUserData.Id
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
    }
}
