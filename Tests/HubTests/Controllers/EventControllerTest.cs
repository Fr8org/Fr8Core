using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Hub.Managers;
using HubWeb.Controllers;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using System.Threading.Tasks;
using System;
using Fr8Data.DataTransferObjects;
using Fr8Data.Crates.Helpers;
using Fr8Data.Managers;

namespace HubTests.Controllers
{
    [TestFixture]
    public class EventControllerTest : BaseTest
    {
        private EventsController _eventController;
        private EventReporter _eventReporter;
        private IncidentReporter _incidentReporter;
        private EventReportCrateFactory _eventReportCrateFactoryHelper;
        private ICrateManager _crate;


        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _eventController = new EventsController();
            _eventReporter = ObjectFactory.GetInstance <EventReporter>();
            _incidentReporter = ObjectFactory.GetInstance <IncidentReporter>();
            _eventReportCrateFactoryHelper = new EventReportCrateFactory();
            _crate = ObjectFactory.GetInstance<ICrateManager>();

        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task Events_NullCrateDTO_ThrowsException()
        {
            await _eventController.Post(null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task Events_NotStandardEventReport_ThrowsException()
        {
            var crateDTO = new CrateDTO();
            await _eventController.Post(crateDTO);
        }
        
    }
}
