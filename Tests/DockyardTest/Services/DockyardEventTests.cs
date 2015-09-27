using System.Linq;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Moq;
using Data.Entities;
using System.Collections.Generic;
using Data.Interfaces.ManifestSchemas;

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
        //    var processTemplateDO = FixtureData.TestProcessTemplateWithSubscribeEvent();
        //    var resultProcessTemplates = new List<ProcessTemplateDO>() { processTemplateDO };
        //    IProcessTemplate curProcessTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
        //    EventReportMS curEventReport = FixtureData.StandardEventReportFormat();
           
        //    Mock<IProcessTemplate> processTemplateMock = new Mock<IProcessTemplate>();
        //    processTemplateMock.Setup(a => a.LaunchProcess(It.IsAny<IUnitOfWork>(), It.IsAny<ProcessTemplateDO>(), null));
        //    processTemplateMock.Setup(a => a.GetMatchingProcessTemplates(It.IsAny<string>(), It.IsAny<EventReportMS>()))
        //        .Returns(resultProcessTemplates);
        //    ObjectFactory.Configure(cfg => cfg.For<IProcessTemplate>().Use(processTemplateMock.Object));
            
        //    IDockyardEvent curDockyardEvent = ObjectFactory.GetInstance<IDockyardEvent>();

        //    curDockyardEvent.ProcessInbound("testuser1", curEventReport);

        //    processTemplateMock.Verify(l => l.LaunchProcess(It.IsAny<IUnitOfWork>(), It.IsAny<ProcessTemplateDO>(), null));
        //}
    }
}
