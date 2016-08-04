using Data.Interfaces;
using Data.States;
using StructureMap;
using Fr8.Infrastructure.Utilities;
using Newtonsoft.Json;
using System.Linq;
using AutoMapper;
using Data.Repositories.Encryption;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;

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
        
        //TODO: this method is a one-time update of transitions inside ContainerTransition control and should be removed after it is deployed to prod
        public static void UpdateTransitionNames()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var encryptionService = ObjectFactory.GetInstance<IEncryptionService>();
                foreach (var activity in uow.PlanRepository
                    .GetActivityQueryUncached()
                    .Where(x => x.ActivityTemplate.Name == "Make_a_Decision" && x.ActivityTemplate.Version == "1"))
                {

                    var dto = Mapper.Map<ActivityDTO>(activity);
                    if (dto.CrateStorage == null)
                    {
                        dto.CrateStorage = JsonConvert.DeserializeObject<CrateStorageDTO>(encryptionService.DecryptString(activity.Fr8AccountId, activity.EncryptedCrateStorage));
                    }
                    using (var storage = new CrateManager().GetUpdatableStorage(dto))
                    {
                        var controls = storage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content;
                        if (controls == null)
                        {
                            continue;
                        }
                        var transitionList = controls.Controls.OfType<ContainerTransition>().First();
                        for (var i = 0; i < transitionList.Transitions.Count; i++)
                        {
                            var transition = transitionList.Transitions[i];
                            transition.Name = $"transition_{i}";
                        }
                    }
                    activity.CrateStorage = JsonConvert.SerializeObject(dto.CrateStorage, Formatting.Indented);
                    activity.EncryptedCrateStorage = encryptionService.EncryptData(activity.Fr8AccountId, activity.CrateStorage);
                    uow.SaveChanges();
                }
            }
        }
    }
}