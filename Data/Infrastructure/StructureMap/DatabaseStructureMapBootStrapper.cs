using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Infrastructure.MultiTenant;
using Data.Interfaces;
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

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }

        public class TestMode : CoreRegistry
        {
            public TestMode()
            {
                For<IDBContext>().Use<MockedDBContext>();
                For<CloudFileManager>().Use<CloudFileManager>();

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }
    }
}