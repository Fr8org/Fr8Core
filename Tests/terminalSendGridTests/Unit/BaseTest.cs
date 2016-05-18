using Fr8Infrastructure.StructureMap;
using NUnit.Framework;
using StructureMap;

namespace terminalSendGrid.Tests
{
    public class BaseTest
    {
        [SetUp]
        public virtual void SetUp()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            MockedDBContext.WipeMockedDatabase();
            DataAutoMapperBootStrapper.ConfigureAutoMapper();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.SaveChanges();
            }
        }
    }
}
