using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories.Plan
{
    public class PlanStorageProviderMockedDb : PlanStorageProviderEf
    {
        public PlanStorageProviderMockedDb(IUnitOfWork uow) 
            : base(uow)
        {
        }

        public override void Update(RouteSnapshot.Changes changes)
        {
            foreach (var routeNodeDo in changes.Delete)
            {
                RouteNodes.Remove(routeNodeDo);

                if (routeNodeDo is ActivityDO)
                {
                    ActivityRepository.Remove((ActivityDO) routeNodeDo);
                }
                else if (routeNodeDo is PlanDO)
                {
                    Routes.Remove((PlanDO) routeNodeDo);
                }
                else if (routeNodeDo is SubrouteDO)
                {
                    Subroutes.Remove((SubrouteDO) routeNodeDo);
                }
            }

            foreach (var routeNodeDo in changes.Insert)
            {
                //RouteNodes.Add(routeNodeDo);

                var entity = routeNodeDo.Clone();

                ClearNavigationProperties(entity);
               
                if (entity is ActivityDO)
                {
                    ActivityRepository.Add((ActivityDO)entity);
                }
                else if (entity is PlanDO)
                {
                    Routes.Add((PlanDO)entity);
                }
                else if (entity is SubrouteDO)
                {
                    Subroutes.Add((SubrouteDO)entity);
                }
                else
                {
                    RouteNodes.Add(entity);
                }
            }

            foreach (var changedObject in changes.Update)
            {
                var routeNodeDo = changedObject.Node;
                object entity = null;

                if (routeNodeDo is ActivityDO)
                {
                    entity = ActivityRepository.GetByKey(routeNodeDo.Id);
                }
                else if (routeNodeDo is PlanDO)
                {
                    entity = Routes.GetByKey(routeNodeDo.Id);
                }
                else if (routeNodeDo is SubrouteDO)
                {
                    entity = Subroutes.GetByKey(routeNodeDo.Id);
                }

                foreach (var changedProperty in changedObject.ChangedProperties)
                {
                    changedProperty.SetValue(entity, changedProperty.GetValue(changedObject.Node));
                }
            }
        }
    }
}
