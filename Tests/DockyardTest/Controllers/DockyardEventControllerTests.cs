using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AutoMapper;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Hub.Services;
using HubWeb.Controllers;
using HubWeb.ViewModels;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.ViewModels;
using Data.Interfaces.Manifests;

namespace DockyardTest.Controllers
{
   
    [TestFixture]
    [Category("DockyardEventController")]
    public class DockyardEventControllerTests : BaseTest
    {
        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task dockyard_events_NullCrateDTO_ThrowsException()
        {
            var dockyardEventController = new DockyardEventController();

             await dockyardEventController.ProcessDockyardEvents(null);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(ArgumentNullException))]
        public async Task dockyard_events_NotStandardEventReport_ThrowsException()
        {
            var dockyardEventController = new DockyardEventController();
            CrateDTO crateDTO = new CrateDTO();


            await dockyardEventController.ProcessDockyardEvents(crateDTO);
        }

        //[Test]
        //public void dockyard_events_CorrectStandardEventReport_ReturnsOK()
        //{
        //    Mock<IDockyardEvent> dockyardEventMock = new Mock<IDockyardEvent>();
        //    dockyardEventMock.Setup(a => a.ProcessInbound("1", It.IsAny<EventReportMS>()));
        //    ObjectFactory.Configure(cfg => cfg.For<IDockyardEvent>().Use(dockyardEventMock.Object));
        //    var dockyardEventController = new DockyardEventController();

        //    var actionResult = dockyardEventController.ProcessDockyardEvents(FixtureData.RawStandardEventReportFormat());

        //    Assert.IsNotNull(actionResult);
        //}
    }
}
