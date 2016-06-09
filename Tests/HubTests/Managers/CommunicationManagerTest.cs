using NUnit.Framework;
using Hub.StructureMap;
using Fr8.Testing.Unit;

namespace HubTests.Managers
{
    [TestFixture]
    public class CommunicationManagerTest : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

       
    }
}
