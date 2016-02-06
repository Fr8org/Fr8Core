namespace Data.Migrations
{
    using Entities;
    using Interfaces;
    using StructureMap;
    using System;
    using System.Data.Entity.Migrations;
    using Utilities;
    using System.Linq;
    using States;
    public partial class Add_Test_User_2 : DbMigration
    {
        private string email = "integration_test_runner@fr8.company";
        private const string password = "fr8#s@lt!";

        public override void Up()
        {
            Register();
        }

        public override void Down()
        {
        }

        private void Register()
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
                    newDockyardAccountDO = Register(uow, email, email, email, password, Roles.Customer);
                }
                else
                {
                    newDockyardAccountDO = Register(uow, email, email, email, password, Roles.Customer);
                }

                uow.SaveChanges();
            }

        }

        public Fr8AccountDO Register(IUnitOfWork uow, string userName, string firstName, string lastName,
    string password, string roleID)
        {
            var userDO = uow.UserRepository.GetOrCreateUser(userName, roleID);
            uow.UserRepository.UpdateUserCredentials(userDO, userName, password);
            uow.AspNetUserRolesRepository.AssignRoleToUser(roleID, userDO.Id);
            return userDO;
        }
    }
}
