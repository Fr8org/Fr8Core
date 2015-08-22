using Core.Managers;
using Core.StructureMap;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using Web.App_Start;

namespace UtilitiesTesting
{
    [TestFixture]
    public class BaseTest
    {

        [SetUp]
        public virtual void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            MockedDBContext.WipeMockedDatabase();
            AutoMapperBootStrapper.ConfigureAutoMapper();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>()) //Get the seeding done first
                uow.SaveChanges();
        }
    }
}