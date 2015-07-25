using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Microsoft.AspNet.Identity;

namespace Data.Repositories
{
    public class UserRepository : GenericRepository<UserDO>, IUserRepository
    {
        internal UserRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
        public override void Add(UserDO entity)
        {
            base.Add(entity);
            //AddDefaultCalendar(entity);
            AddDefaultProfile(entity);
        }

        public UserDO UpdateUserCredentials(String emailAddress, String userName = null, String password = null)
        {
            return UpdateUserCredentials(UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress), userName, password);
        }

        public UserDO UpdateUserCredentials(EmailAddressDO emailAddressDO, String userName = null, String password = null)
        {
            return UpdateUserCredentials(UnitOfWork.UserRepository.GetOrCreateUser(emailAddressDO), userName, password);
        }

        public UserDO UpdateUserCredentials(UserDO userDO, String userName = null, String password = null)
        {
            if (userName != null)
                userDO.UserName = userName;
            if (password != null)
            {
                var passwordHasher = new PasswordHasher();
                userDO.PasswordHash = passwordHasher.HashPassword(password);       
            }
            
            return userDO;
        }


        public UserDO GetOrCreateUser(String emailAddress)
        {
            return GetOrCreateUser(UnitOfWork.EmailAddressRepository.GetOrCreateEmailAddress(emailAddress));
        }

        public UserDO GetOrCreateUser(EmailAddressDO emailAddressDO)
        {
            var matchingUser = UnitOfWork.UserRepository.DBSet.Local.FirstOrDefault(u => u.EmailAddress.Address == emailAddressDO.Address);
            if (matchingUser == null)
                matchingUser = UnitOfWork.UserRepository.GetQuery().FirstOrDefault(u => u.EmailAddress.Address == emailAddressDO.Address);

            if (matchingUser == null)
            {
                matchingUser =
                    new UserDO
                    {
                        EmailAddress = emailAddressDO,
                        UserName = emailAddressDO.Address,
                        FirstName = emailAddressDO.Name,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        State = UserState.Active
                    };
                UnitOfWork.UserRepository.Add(matchingUser);

                UnitOfWork.AspNetUserRolesRepository.AssignRoleToUser(Roles.Customer, matchingUser.Id);
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


        public void AddDefaultProfile(UserDO curUser)
        {
            if (curUser == null)
                throw new ArgumentNullException("curUser");

            if (!curUser.Profiles.Any())
            {
                var defaultProfile = new ProfileDO() {Name = "Default Profile", User = curUser};
                defaultProfile.ProfileNodes.Add(new ProfileNodeDO { Name = "Communications", Profile = defaultProfile, ProfileID = defaultProfile.Id});
                defaultProfile.ProfileNodes.Add(new ProfileNodeDO { Name = "Locations", Profile = defaultProfile, ProfileID = defaultProfile.Id });
                defaultProfile.ProfileNodes.Add(new ProfileNodeDO { Name = "Travel", Profile = defaultProfile, ProfileID = defaultProfile.Id });
                curUser.Profiles.Add(defaultProfile);
            }
        }
    }
}