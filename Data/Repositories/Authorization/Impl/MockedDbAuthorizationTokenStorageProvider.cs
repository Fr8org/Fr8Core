using Data.Interfaces;

namespace Data.Repositories.Authorization
{
    public class MockedDbAuthorizationTokenStorageProvider : EfAuthorizationTokenStorageProvider
    {
        public MockedDbAuthorizationTokenStorageProvider(IUnitOfWork uow) 
            : base(uow)
        {
        }

        public override void Update(AuthorizationTokenChanges changes)
        {
            foreach (var planNodeDo in changes.Delete)
            {
                Repository.Remove(planNodeDo);
            }

            foreach (var planNodeDo in changes.Insert)
            {
                var entity = planNodeDo.Clone();

                ClearNavigationProperties(entity);

                Repository.Add(entity);
            }

            foreach (var changedObject in changes.Update)
            {
                var token = changedObject.Token.Clone();
                object entity = Repository.GetByKey(token.Id);
                
                foreach (var changedProperty in changedObject.ChangedProperties)
                {
                    changedProperty.SetValue(entity, changedProperty.GetValue(token));
                }
            }
        }
    }
}
