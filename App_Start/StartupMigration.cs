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
                    
                    var storage = activity.EncryptedCrateStorage == null || activity.EncryptedCrateStorage.Length == 0
                        ? activity.CrateStorage
                        : encryptionService.DecryptString(activity.Fr8AccountId, activity.EncryptedCrateStorage);
                    if (string.IsNullOrEmpty(storage))
                    {
                        continue;
                    }
                    var crateStorageDto = JsonConvert.DeserializeObject<CrateStorageDTO>(storage);
                    var crateStorage = CrateStorageSerializer.Default.ConvertFromDto(crateStorageDto);
                    var controls = crateStorage.FirstCrateOrDefault<StandardConfigurationControlsCM>()?.Content;
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
                    crateStorageDto = CrateStorageSerializer.Default.ConvertToDto(crateStorage);
                    storage = JsonConvert.SerializeObject(crateStorageDto, Formatting.Indented);
                    if (!string.IsNullOrEmpty(activity.CrateStorage))
                    {
                        activity.CrateStorage = storage;
                    }
                    activity.EncryptedCrateStorage = encryptionService.EncryptData(activity.Fr8AccountId, storage);
                    uow.SaveChanges();
                }
            }
        }
    }
}