using System;
using System.Data.Entity;
using StructureMap.Configuration.DSL;
using Data.Interfaces;
using Data.Repositories;
using Data.Repositories.Plan;

namespace Data.Infrastructure
{
    public class MigrationConsoleSeedRegistry : Registry
    {
        public MigrationConsoleSeedRegistry()
        {
            For<DbContext>().Use<DockyardDbContext>();
            For<IDBContext>().Use<DockyardDbContext>();
            For<IAuthorizationTokenRepository>().Use<AuthorizationTokenRepositoryStub>();
            For<IUnitOfWork>().Use<UnitOfWork>();
            For<IPlanCache>().Use<PlanCache>().Singleton();

            var planCacheExpiration = TimeSpan.FromMinutes(10);
            For<IPlanCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(planCacheExpiration)).Singleton();

            For<IPlanStorageProvider>().Use<PlanStorageProviderEf>();
            For<PlanStorage>().Use<PlanStorage>();
        }
    }
}
