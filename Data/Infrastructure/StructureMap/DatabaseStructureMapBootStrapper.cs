using System.Data.Entity;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces;
using Data.Wrappers;
using DocuSign.Integrations.Client;
using StructureMap.Configuration.DSL;

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
                For<IDockyardAccountDO>().Use<DockyardAccountDO>();
                For<IAspNetRolesDO>().Use<AspNetRolesDO>();
                For<IDocuSignTemplate>().Use<DocuSignTemplate>();
                For<IAspNetUserRolesDO>().Use<AspNetUserRolesDO>();
                For<IUnitOfWork>().Use(_ => new UnitOfWork(_.GetInstance<IDBContext>()));

            }
        }

        public class LiveMode : CoreRegistry 
        {
            public LiveMode()
            {
                For<DbContext>().Use<DockyardDbContext>();
                For<IDBContext>().Use<DockyardDbContext>();

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }

        public class TestMode : CoreRegistry
        {
            public TestMode()
            {
                For<IDBContext>().Use<MockedDBContext>();

                DataAutoMapperBootStrapper.ConfigureAutoMapper();
            }
        }
    }
}