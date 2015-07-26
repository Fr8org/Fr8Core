using System;
using System.Linq;
using Data.Interfaces;
using Data.Validations;
using Data.Entities;
using Data.States;
using StructureMap;
using Data.Infrastructure;
using Utilities;
using System.Collections.Generic;

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
            if(!String.IsNullOrEmpty(dockyardAccountDO.PasswordHash))
                return CommunicationMode.Direct;
            return CommunicationMode.Delegate;
        }

        //
        //get roles for this DockYardAccount
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
            submittedDockyardAccountData.EmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(submittedDockyardAccountData.EmailAddress.Address);
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
            DockyardAccountDO existingDockyardAccount = uow.UserRepository.GetQuery().Where(e => e.EmailAddress.Address == emailAddress).FirstOrDefault();
            return existingDockyardAccount;
        }

        public void Update(IUnitOfWork uow, DockyardAccountDO submittedDockyardAccountData, DockyardAccountDO existingDockyardAccount)
        {
            existingDockyardAccount.FirstName = submittedDockyardAccountData.FirstName;
            existingDockyardAccount.LastName = submittedDockyardAccountData.LastName;

            //Remove old roles
            foreach (var existingRole in existingDockyardAccount.Roles.ToList())
            {
                if (!submittedDockyardAccountData.Roles.Select(role => role.RoleId).Contains(existingRole.RoleId))
                    uow.AspNetUserRolesRepository.Remove(uow.AspNetUserRolesRepository.FindOne(e => e.RoleId == existingRole.RoleId && e.UserId == existingDockyardAccount.Id));
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
                    (r => r.ProcessState == ProcessState.Processing
                        & r.UserId == userId).ToList();
            }
        }
    }
}
