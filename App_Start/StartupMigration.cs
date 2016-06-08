using Data.Interfaces;
using Data.States;
using StructureMap;
using Fr8.Infrastructure.Utilities;

namespace HubWeb.App_Start
{
    public class StartupMigration
    {
        public static void CreateSystemUser()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var configRepository  = ObjectFactory.GetInstance<IConfigRepository>();
                string userEmail = configRepository.Get("SystemUserEmail");
                string curPassword = configRepository.Get("SystemUserPassword");

                var user = uow.UserRepository.GetOrCreateUser(userEmail);
                uow.UserRepository.UpdateUserCredentials(userEmail, userEmail, curPassword);
                uow.AspNetUserRolesRepository.AssignRoleToUser(Roles.Admin, user.Id);
                user.TestAccount = false;
                
                uow.SaveChanges();
            }
        }
    }
}