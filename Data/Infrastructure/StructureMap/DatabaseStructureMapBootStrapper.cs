using System;
using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces;
using Data.Repositories;
using Data.Repositories.Authorization;
using Data.Repositories.Cache;
using Data.Repositories.Encryption;
using Data.Repositories.Encryption.Impl;
using Data.Repositories.Encryption.Impl.KeyVault;
using Data.Repositories.Encryption.Impl.Rijndael;
using Data.Repositories.MultiTenant;
using Data.Repositories.MultiTenant.InMemory;
using Data.Repositories.MultiTenant.Sql;
using Data.Repositories.Plan;
using Data.Repositories.Security;
using Data.Repositories.Security.StorageImpl;
using Data.Repositories.Security.StorageImpl.Cache;
using Data.Repositories.Security.StorageImpl.SqlBased;
using Data.Repositories.SqlBased;
using Data.Repositories.Utilization;
using Fr8.Infrastructure.Utilities.Configuration;
using StructureMap.Configuration.DSL;

//using MT_FieldService = Data.Infrastructure.MultiTenant.MT_Field;

namespace Data.Infrastructure.StructureMap
{
    public class DatabaseStructureMapBootStrapper
    {
        public class CoreRegistry : Registry
        {
            public CoreRegistry()
            {
                int exp;
                TimeSpan planCacheExpiration = TimeSpan.FromMinutes(10);

                var expStr = CloudConfigurationManager.GetSetting("Cache.PlanRepository.Expiration");

                if (!string.IsNullOrWhiteSpace(expStr) && int.TryParse(expStr, out exp))
                {
                    planCacheExpiration = TimeSpan.FromMinutes(exp);
                }
                //todo: add setting key with expiration for security objects

                For<IAttachmentDO>().Use<AttachmentDO>();
                For<IEmailDO>().Use<EmailDO>();
                For<IEmailAddressDO>().Use<EmailAddressDO>();
                For<IFr8AccountDO>().Use<Fr8AccountDO>();
                For<IAspNetRolesDO>().Use<AspNetRolesDO>();
                For<IAspNetUserRolesDO>().Use<AspNetUserRolesDO>();
                For<IUnitOfWork>().Use<UnitOfWork>();
                For<IUnitOfWorkFactory>().Use<StructureMapUnitOfWorkFactory>().Singleton();
                For<IMultiTenantObjectRepository>().Use<MultitenantRepository>();
                For<IMtObjectConverter>().Use<MtObjectConverter>().Singleton();
                For<IMtTypeStorage>().Use<MtTypeStorage>().Singleton();
                For<IPlanCache>().Use<PlanCache>().Singleton();
                For<PlanStorage>().Use<PlanStorage>();
                For<ISecurityObjectsCache>().Use<SecurityObjectsCache>().Singleton();
                For<IPlanCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(planCacheExpiration)).Singleton();
                For<ISecurityCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(planCacheExpiration)).Singleton();
                For<IEncryptionService>().Use<EncryptionService>().Singleton();
                For<IEncryptionProvider>().Add<BypassEncryptionProvider>().Singleton();
                For<IEncryptionProvider>().Add<CompressingEncryptionProvider>().Singleton();
                For<IEncryptionKeyProvider>().Use<KeyVaultEncryptionKeyProvider>().Singleton();
            }
        }

        public class LiveMode : CoreRegistry 
        {
            public LiveMode()
            {
                For<DbContext>().Use<DockyardDbContext>();
                For<IDBContext>().Use<DockyardDbContext>();
                For<CloudFileManager>().Use<CloudFileManager>();
               
                var mode = CloudConfigurationManager.GetSetting("AuthorizationTokenStorageMode");
                if (mode != null)
                {
                    switch (mode.ToLower())
                    {
                        case "local":
                            For<IAuthorizationTokenRepository>().Use<SqlAuthorizationTokenRepository>();
                            break;

                        case "keyvault":
                            For<IAuthorizationTokenRepository>().Use<KeyVaultAuthorizationTokenRepository>();
                            break;

                        default:
                            throw new NotSupportedException($"Unsupported AuthorizationTokenStorageMode = {mode}");
                    }
                }
                else
                {
                    For<IAuthorizationTokenRepository>().Use<AuthorizationTokenRepositoryStub>();
                }

                For<IAuthorizationTokenStorageProvider>().Use<EfAuthorizationTokenStorageProvider>();

                For<IPlanStorageProvider>().Use<PlanStorageProviderEf>();
                For<ISqlConnectionProvider>().Use<SqlConnectionProvider>();
                For<IMtObjectsStorage>().Use<SqlMtObjectsStorage>().Singleton();
                For<IMtTypeStorageProvider>().Use<SqlMtTypeStorageProvider>();
                For<ISqlConnectionProvider>().Use<SqlConnectionProvider>();
                For<ISecurityObjectsStorageProvider>().Use<SqlSecurityObjectsStorageProvider>();
                For<ISecurityObjectsStorageProvider>().DecorateAllWith<SecurityObjectsStorage>();
                For<IUtilizationDataProvider>().Use<SqlUtilizationDataProvider>();
                
                var defaultEncryptionProvider = CloudConfigurationManager.GetSetting("DefaultEncryptionProvider");

                if (!string.IsNullOrWhiteSpace(defaultEncryptionProvider))
                {
                    var providerType = Type.GetType(defaultEncryptionProvider, false);

                    if (providerType == null)
                    {
                        throw new NotSupportedException($"Encryption provider {defaultEncryptionProvider} was not found.");
                    }

                    For(typeof(IEncryptionProvider)).Use(providerType);
                }
                else
                {
                    For<IEncryptionProvider>().Use<RijndaelEncryptionProviderWithCompressionV1>().Singleton();
                }

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }

        public class TestMode : CoreRegistry
        {
            public TestMode()
            {
                For<IAuthorizationTokenStorageProvider>().Use<MockedDbAuthorizationTokenStorageProvider>();
                For<IAuthorizationTokenRepository>().Use<AuthorizationTokenRepositoryForTests>();
                For<IDBContext>().Use<MockedDBContext>();
                For<CloudFileManager>().Use<CloudFileManager>();
                For<IPlanCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(TimeSpan.FromDays(365))).Singleton(); // in test mode cache will never expire in practice
                For<ISecurityCacheExpirationStrategy>().Use(_ => new SlidingExpirationStrategy(TimeSpan.FromDays(365))).Singleton(); // in test mode cache will never expire in practice
                For<IPlanCache>().Use<PlanCache>().Singleton();
                For<ISecurityObjectsCache>().Use<SecurityObjectsCache>().Singleton();
                For<IPlanStorageProvider>().Use<PlanStorageProviderMockedDb>();
                For<ISqlConnectionProvider>().Use<DummyConnectionProvider>();
                For<IMtObjectsStorage>().Use<InMemoryMtObjectsStorage>().Singleton();
                For<IMtTypeStorageProvider>().Use<InMemoryMtTypeStorageProvider>();
                For<ISecurityObjectsStorageProvider>().Use<InMemorySecurityObjectsStorageProvider>();
                For<IEncryptionProvider>().Use<BypassEncryptionProvider>().Singleton();
                For<IUtilizationDataProvider>().Use<MockedUtilizationDataProvider>().Singleton();

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }
    }
}