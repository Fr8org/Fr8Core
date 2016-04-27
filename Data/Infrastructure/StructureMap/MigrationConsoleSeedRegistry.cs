using System;
using System.Data.Entity;
using StructureMap.Configuration.DSL;
using Data.Interfaces;
using Data.Repositories;
using Data.Repositories.Cache;
using Data.Repositories.Encryption;
using Data.Repositories.Encryption.Impl;
using Data.Repositories.MultiTenant;
using Data.Repositories.MultiTenant.InMemory;
using Data.Repositories.Plan;
using Data.Repositories.Security.StorageImpl.Cache;
using Data.Repositories.SqlBased;

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
            For<IMultiTenantObjectRepository>().Use<MultitenantRepository>();
            For<IMtObjectConverter>().Use<MtObjectConverter>().Singleton();
            For<ISqlConnectionProvider>().Use<SqlConnectionProvider>().Singleton();
            For<IMtTypeStorage>().Use<MtTypeStorage>().Singleton();

            For<IMtObjectsStorage>().Use<InMemoryMtObjectsStorage>().Singleton();
            For<IMtTypeStorageProvider>().Use<InMemoryMtTypeStorageProvider>();

            var planCacheExpiration = TimeSpan.FromMinutes(10);
            For<IPlanCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(planCacheExpiration)).Singleton();
            For<ISecurityCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(planCacheExpiration)).Singleton();
            For<IEncryptionService>().Use<EncryptionService>().Singleton();
            For<IPlanStorageProvider>().Use<PlanStorageProviderEf>();

            //*************** !!!!!!!!CHANGE TO REAL PROVIDER!!!!!!!!! ********************//
            For<IEncryptionProvider>().Use<DefaultEncryptionProvider>().Singleton();

            For<PlanStorage>().Use<PlanStorage>();
        }
    }
}
