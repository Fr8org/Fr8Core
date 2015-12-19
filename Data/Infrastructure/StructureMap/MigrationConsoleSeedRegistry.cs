using System.Data.Entity;
using StructureMap.Configuration.DSL;
using Data.Interfaces;
using Data.Infrastructure;

namespace Data.Infrastructure
{
    public class MigrationConsoleSeedRegistry : Registry
    {
        public MigrationConsoleSeedRegistry()
        {
            For<DbContext>().Use<DockyardDbContext>();
            For<IDBContext>().Use<DockyardDbContext>();

            For<IUnitOfWork>().Use(_ => new UnitOfWork(_.GetInstance<IDBContext>()));
        }
    }
}
