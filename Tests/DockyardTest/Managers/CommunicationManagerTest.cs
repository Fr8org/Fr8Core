using Core.StructureMap;
using NUnit.Framework;
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
