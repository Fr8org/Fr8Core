using NUnit.Framework;
using Hub.StructureMap;
using UtilitiesTesting;

namespace DockyardTest.Managers
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
