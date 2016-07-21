using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories.Authorization
{
    public class EfAuthorizationTokenStorageProvider : IAuthorizationTokenStorageProvider
    {
        public GenericRepository<AuthorizationTokenDO> Repository { get; }
        public IUnitOfWork Uow { get; }

        public EfAuthorizationTokenStorageProvider(IUnitOfWork uow)
        {
            Uow = uow;
            Repository = new GenericRepository<AuthorizationTokenDO>(uow);
        }

        public virtual void Update(AuthorizationTokenChanges changes)
        {
            var adapter = (IObjectContextAdapter)Uow.Db;
            var objectContext = adapter.ObjectContext;

            ObjectStateEntry entry;

            foreach (var token in changes.Delete)
            {
                var entryStub = token.Clone();

                ClearNavigationProperties(entryStub);

                var key = objectContext.CreateEntityKey("AuthorizationTokenDOes", entryStub);

                if (!objectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
                {
                    Repository.Attach(entryStub);
                    entry = objectContext.ObjectStateManager.GetObjectStateEntry(entryStub);
                    entry.Delete();
                }
                else
                {
                    var planNodeFromObjectContext = objectContext.GetObjectByKey(key);
                    Repository.Remove((AuthorizationTokenDO)planNodeFromObjectContext);
                }
            }

            foreach (var token in changes.Insert)
            {
                var entity = token.Clone();

                ClearNavigationProperties(entity);
                
                Repository.Add(entity);
            }

            foreach (var changedObject in changes.Update)
            {
                var entryStub = changedObject.Token.Clone();

                ClearNavigationProperties(entryStub);
                
                var key = objectContext.CreateEntityKey("AuthorizationTokenDOes", entryStub);
                if (!objectContext.ObjectStateManager.TryGetObjectStateEntry(key, out entry))
                {
                    Repository.Attach(entryStub);
                    entry = objectContext.ObjectStateManager.GetObjectStateEntry(entryStub);
                    foreach (var changedProperty in changedObject.ChangedProperties)
                    {
                        if (changedProperty.Name == nameof(AuthorizationTokenDO.Token))
                        {
                            continue;
                        }

                        entry.SetModifiedProperty(changedProperty.Name);
                    }
                }
                else
                {
                    foreach (var changedProperty in changedObject.ChangedProperties)
                    {
                        if (changedProperty.Name == nameof(AuthorizationTokenDO.Token))
                        {
                            continue;
                        }

                        changedProperty.SetValue(entry.Entity, changedProperty.GetValue(entryStub));
                    }
                }
            }
        }

        protected void ClearNavigationProperties(AuthorizationTokenDO entryStub)
        {
            entryStub.UserDO = null;
            entryStub.AuthorizationTokenStateTemplate = null;
            entryStub.Terminal = null;
        }

        public IQueryable<AuthorizationTokenDO> GetQuery()
        {
            return Repository.GetQuery();
        }

        public AuthorizationTokenDO GetByKey(Guid id)
        {
            return Repository.GetByKey(id);
        }
    }
}
