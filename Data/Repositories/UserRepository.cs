using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Microsoft.AspNet.Identity;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<Fr8AccountDO>, IUserRepository
    {
        internal UserRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
        public override void Add(Fr8AccountDO entity)
        {
            base.Add(entity);
            //AddDefaultCalendar(entity);
            //AddDefaultProfile(entity);
        }

        public Fr8AccountDO UpdateUserCredentials(String emailAddress, String userName = null, String password = null)
        {
            return UpdateUserCredentials(UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress), userName, password);
        }

        public Fr8AccountDO UpdateUserCredentials(EmailAddressDO emailAddressDO, String userName = null, String password = null)
        {
            return UpdateUserCredentials(UnitOfWork.UserRepository.GetOrCreateUser(emailAddressDO), userName, password);
        }

        public Fr8AccountDO UpdateUserCredentials(Fr8AccountDO dockyardAccountDO, String userName = null, String password = null)
        {
            if (userName != null)
                dockyardAccountDO.UserName = userName;
            if (password != null)
            {
                var passwordHasher = new PasswordHasher();
                dockyardAccountDO.PasswordHash = passwordHasher.HashPassword(password);       
            }
            
            return dockyardAccountDO;
        }


        public Fr8AccountDO GetOrCreateUser(String emailAddress, string userRole = Roles.Customer)
        {
            return GetOrCreateUser(UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress), userRole);
        }

        public Fr8AccountDO GetOrCreateUser(EmailAddressDO emailAddressDO, string userRole = Roles.Customer)
        {
            var matchingUser = UnitOfWork.UserRepository.DBSet.Local.FirstOrDefault(u => u.EmailAddress.Address == emailAddressDO.Address);
            if (matchingUser == null)
                matchingUser = UnitOfWork.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddress.Address == emailAddressDO.Address);

            if (matchingUser == null)
            {
                matchingUser =
                    new Fr8AccountDO
                    {
                        EmailAddress = emailAddressDO,
                        UserName = emailAddressDO.Address,
                        FirstName = emailAddressDO.Name,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        State = UserState.Active
                    };
                UnitOfWork.UserRepository.Add(matchingUser);

                if (!userRole.Equals(Roles.Guest))
                {
                    // Assign Customer role only if creation of Guest user is not intended
                    UnitOfWork.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, matchingUser.Id);
                }
            }
            
            return matchingUser;
        }


        //public void AddDefaultCalendar(UserDO curUser)
        //{
        //    if (curUser == null)
        //        throw new ArgumentNullException("curUser");

        //    if (!curUser.Calendars.Any())
        //    {
        //        var curCalendar = new CalendarDO
        //        {
        //            Name = "Default Calendar",
        //            Owner = curUser,
        //            OwnerID = curUser.Id
        //        };
        //        curUser.Calendars.Add(curCalendar);
        //    }
        //}


        public void AddDefaultProfile(Fr8AccountDO curDockyardAccount)
        {
            if (curDockyardAccount == null)
                throw new ArgumentNullException("curDockyardAccount");

            if (!curDockyardAccount.Profiles.Any())
            {
                var defaultProfile = new ProfileDO() {Name = "Default Profile", DockyardAccount = curDockyardAccount};
                defaultProfile.ProfileNodes.Add(new ProfileNodeDO { Name = "Communications", Profile = defaultProfile, ProfileID = defaultProfile.Id});
                defaultProfile.ProfileNodes.Add(new ProfileNodeDO { Name = "Locations", Profile = defaultProfile, ProfileID = defaultProfile.Id });
                defaultProfile.ProfileNodes.Add(new ProfileNodeDO { Name = "Travel", Profile = defaultProfile, ProfileID = defaultProfile.Id });
                curDockyardAccount.Profiles.Add(defaultProfile);
            }
        }
    }
}