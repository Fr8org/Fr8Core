using System;
using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.MultiTenant;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.Data.Edm.Library.Values;
using StructureMap.Configuration.DSL;
using Utilities.Configuration.Azure;

//using MT_FieldService = Data.Infrastructure.MultiTenant.MT_Field;

namespace Data.Infrastructure.StructureMap
{
    public class DatabaseStructureMapBootStrapper
    {
        public class CoreRegistry : Registry
        {
            public CoreRegistry()
            {
                For<IAttachmentDO>().Use<AttachmentDO>();
                For<IEmailDO>().Use<EmailDO>();
                For<IEmailAddressDO>().Use<EmailAddressDO>();
                For<IFr8AccountDO>().Use<Fr8AccountDO>();
                For<IAspNetRolesDO>().Use<AspNetRolesDO>();
                For<IAspNetUserRolesDO>().Use<AspNetUserRolesDO>();
                For<IUnitOfWork>().Use(_ => new UnitOfWork(_.GetInstance<IDBContext>()));

               // For<IMT_Field>().Use<MT_FieldService>();
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
                            throw new NotSupportedException(string.Format("Unsupported AuthorizationTokenStorageMode = {0}", mode));
                    }
                }
                else
                {
                    For<IAuthorizationTokenRepository>().Use<AuthorizationTokenRepositoryStub>();
                }

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }

        public class TestMode : CoreRegistry
        {
            public TestMode()
            {
                For<IAuthorizationTokenRepository>().Use<AuthorizationTokenRepositoryForTests>();
                For<IDBContext>().Use<MockedDBContext>();
                For<CloudFileManager>().Use<CloudFileManager>();

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }
    }
}