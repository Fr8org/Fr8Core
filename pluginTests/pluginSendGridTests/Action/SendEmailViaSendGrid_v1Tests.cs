using NUnit.Framework;
using PluginBase.BaseClasses;
using pluginSendGrid;
using pluginSendGrid.Actions;
using pluginSendGrid.Infrastructure;
using pluginSendGrid.Services;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using pluginTests.pluginSendGridTests.Action;
using Utilities;

namespace pluginTests.pluginSendGridTests
{
    [TestFixture]
    public class SendEmailViaSendGrid_v1Tests : BaseTest
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            PluginSendGridStructureMapBootstrapper.ConfigureDependencies(PluginSendGridStructureMapBootstrapper.DependencyType.LIVE);
        }

        [Test]
        public void Configure_ReturnsCrateStorage()
        {
            var action = FixtureData.TestActionDTOForSendGrid();
            SendEmailViaSendGrid_v1 sendGridV1 = new SendEmailViaSendGrid_v1();

            action = sendGridV1.Configure(action).Result;

            Assert.IsNotNull(action.CrateStorage);
            Assert.Greater(action.CrateStorage.CrateDTO.Count, 0);
        }
    }
}
