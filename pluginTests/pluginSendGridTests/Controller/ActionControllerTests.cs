using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using pluginSendGrid.Controllers;
using pluginSendGrid;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
namespace pluginTests.pluginSendGridTests.Controller
{
    [TestFixture]
    public class SendGridActionControllerTests : BaseTest
    {
        ActionController _actionController;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _actionController = new ActionController();
            Core.StructureMap.StructureMapBootStrapper.ConfigureDependencies(Core.StructureMap.StructureMapBootStrapper.DependencyType.LIVE);
            PluginSendGridStructureMapBootstrapper.ConfigureDependencies(PluginSendGridStructureMapBootstrapper.DependencyType.LIVE);
        }

        [Test]
        public void Configure_ReturnsCrateStorage()
        {
            var action = FixtureData.TestActionDTOForSendGrid();
            ActionDTO result = (ActionDTO)_actionController.Configure(action).Result;
            Assert.IsNotNull(result);
            Assert.Greater(result.CrateStorage.CrateDTO.Count, 0);
        }
    }
}
