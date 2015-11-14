using System;
using UtilitiesTesting;
using NUnit.Framework;
using TerminalBase.BaseClasses;
using Data.Interfaces.DataTransferObjects;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;

namespace pluginBaseTests.BaseClasses
{
    [TestFixture]
    [Category("BaseTerminalController")]
    public class BaseTerminalControllerTests : BaseTest
    {
        IDisposable _coreServer;
        BaseTerminalController _basePluginController;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _basePluginController = new BaseTerminalController();
            _coreServer = terminalFr8CoreTests.Fixtures.FixtureData.CreateCoreServer_ActivitiesController();
        }

        [TearDown]
        public void TearDown()
        {
            if (_coreServer != null)
            {
                _coreServer.Dispose();
                _coreServer = null;
            }
        }

    }
}