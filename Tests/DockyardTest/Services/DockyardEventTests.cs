using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.Manifests;

namespace DockyardTest.Services
{
    [TestFixture]
    public class DockyardEventTests : BaseTest
    {
        //[Test]
        //[ExpectedException(ExpectedException = typeof(System.ArgumentNullException))]
        //public void ProcessInbound_EmptyUserID()
        //{
        //    IDockyardEvent curDockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();

        //    curDockyardEvent.ProcessInbound("", new EventReportMS());
        //}

        //[Test]
        //public void ProcessInbound_CorrectStandardEventReportLabel_CallLaunchProcess()
        //{
        //    var processTemplateDO = FixtureData.TestRouteWithSubscribeEvent();
        //    var resultRoutes = new List<RouteDO>() { processTemplateDO };
        //    IRoute curRoute = ObjectFactory.GetInstance<IRoute>();
        //    EventReportMS curEventReport = FixtureData.StandardEventReportFormat();
           
        //    Mock<IRoute> processTemplateMock = new Mock<IRoute>();
        //    processTemplateMock.Setup(a => a.LaunchProcess(It.IsAny<IUnitOfWork>(), It.IsAny<RouteDO>(), null));
        //    processTemplateMock.Setup(a => a.GetMatchingRoutes(It.IsAny<string>(), It.IsAny<EventReportMS>()))
        //        .Returns(resultRoutes);
        //    ObjectFactory.Configure(cfg => cfg.For<IRoute>().Use(processTemplateMock.Object));
            
        //    IDockyardEvent curDockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();

        //    curDockyardEvent.ProcessInbound("testuser1", curEventReport);

        //    processTemplateMock.Verify(l => l.LaunchProcess(It.IsAny<IUnitOfWork>(), It.IsAny<RouteDO>(), null));
        //}
    }
}
