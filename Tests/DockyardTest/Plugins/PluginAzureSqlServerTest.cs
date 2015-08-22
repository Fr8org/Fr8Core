using System.Linq;
using System.Web.Http.Results;
using Core.Managers;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using pluginAzureSqlServer.Controllers;
using Data.Entities;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http;
using System.Net.Http;
using System;

namespace DockyardTest.Plugins
{
    [TestFixture]
    public class PluginAzureSqlServerTest : BaseTest
    {
        private ActionController _actionController;
        private EventReporter _eventReporter;
        private IncidentReporter _incidentReporter;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _actionController = new ActionController();
            _eventReporter = new EventReporter();
            _incidentReporter = new IncidentReporter();
        }

        [Test]
        [Category("pluginAzureSqlServer.Controllers.ActionController")]
        public void PluginAzureSqlServer_Action_Process_Test()
        {
            //Arrange with plugin incident
            _incidentReporter.SubscribeToAlerts();
            var eventDto = FixtureData.TestPluginIncidentDto();

            //Act
            _actionController.Request = new HttpRequestMessage { RequestUri = new Uri("http://localhost:46281/plugin_azure_sql_server/actions/Write_To_Sql_Server/available") };
            var result = _actionController.Process("available", null);

            //Assert
            Assert.Pass();

            var uow = ObjectFactory.GetInstance<IUnitOfWork>();
            var redfsasult = uow.IncidentRepository.GetAll();

            Assert.AreEqual(1, uow.IncidentRepository.GetAll().Count());
        }
    }
}
