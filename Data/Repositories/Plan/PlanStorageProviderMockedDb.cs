using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Encryption;

namespace Data.Repositories.Plan
{
    public class PlanStorageProviderMockedDb : PlanStorageProviderEf
    {
        public PlanStorageProviderMockedDb(IUnitOfWork uow, IEncryptionService encryptionService) 
            : base(uow, encryptionService)
        {
        }

        public override void Update(PlanSnapshot.Changes changes)
        {
            foreach (var planNodeDo in changes.Delete)
            {
                PlanNodes.Remove(planNodeDo);

                if (planNodeDo is ActivityDO)
                {
                    ActivityRepository.Remove((ActivityDO) planNodeDo);
                }
                else if (planNodeDo is PlanDO)
                {
                    Plans.Remove((PlanDO) planNodeDo);
                }
                else if (planNodeDo is SubplanDO)
                {
                    SubPlans.Remove((SubplanDO) planNodeDo);
                }
            }

            foreach (var planNodeDo in changes.Insert)
            {
                var entity = planNodeDo.Clone();

                ClearNavigationProperties(entity);
               
                if (entity is ActivityDO)
                {
                    EncryptActivityCrateStorage((ActivityDO) entity);
                    ActivityRepository.Add((ActivityDO)entity);
                }
                else if (entity is PlanDO)
                {
                    Plans.Add((PlanDO)entity);
                }
                else if (entity is SubplanDO)
                {
                    SubPlans.Add((SubplanDO)entity);
                }
                else
                {
                    PlanNodes.Add(entity);
                }
            }

            foreach (var changedObject in changes.Update)
            {
                var planNodeDo = changedObject.Node.Clone();
                object entity = null;

                if (planNodeDo is ActivityDO)
                {
                    entity = ActivityRepository.GetByKey(planNodeDo.Id);
                    UpdateEncryptedActivityCrateStorage((ActivityDO)planNodeDo, changedObject);
                }
                else if (planNodeDo is PlanDO)
                {
                    entity = Plans.GetByKey(planNodeDo.Id);
                }
                else if (planNodeDo is SubplanDO)
                {
                    entity = SubPlans.GetByKey(planNodeDo.Id);
                }

                foreach (var changedProperty in changedObject.ChangedProperties)
                {
                    changedProperty.SetValue(entity, changedProperty.GetValue(planNodeDo));
                }
            }
        }
    }
}
