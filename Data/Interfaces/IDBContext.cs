using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Data.Interfaces
{
    public interface IDBContext : IDisposable
    {
        IUnitOfWork UnitOfWork { get; set; }

        int SaveChanges();

/*
        List<KwasantDbContext.PropertyChangeInformation> GetEntityModifications<T>(T entity)
            where T : class;

        List<KwasantDbContext.EntityChangeInformation> GetModifiedEntities();
*/

        void DetectChanges();

        object[] AddedEntities { get; }
        object[] ModifiedEntities { get; }
        object[] DeletedEntities { get; }

        IDbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;
    }
}
