using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Core.Services;
using Core.StructureMap;
using StructureMap;
using Core.Interfaces;
using System.IO;
using Moq;
using Data.Interfaces;
using Data.Entities;
using Data.States;
using DockyardTest.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ProcessService")]
    public class ProcessServiceTests : BaseTest
    {
        private IProcess _processService;
        private string _testUserId = "testuser";

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _processService = ObjectFactory.GetInstance<IProcess>();
        }

        [Test]
        public void ProcessService_CanExtractEnvelopeData()
        {
            string xmlPayLoadLocation = "../../Content/DocusignXmlPayload.xml";
            string envelopeId = _processService.GetEnvelopeIdFromXml(File.ReadAllText(xmlPayLoadLocation));
            Assert.AreEqual("0aa561b8-b4d9-47e0-a615-2367971f876b", envelopeId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ProcessService_ThrowsIfXmlInvalid()
        {
            string xmlPayloadLocation = "../../Content/DocusignXmlPayload_invalid.xml";
            _processService.HandleDocusignNotification(_testUserId, File.ReadAllText(xmlPayloadLocation));
        }

        [Test]
        public void ProcessService_NotificationReceivedAlertCreated()
        {
            //Arrange 
            string xmlPayloadLocation = "../../Content/DocusignXmlPayload.xml";

            //Act
            _processService.HandleDocusignNotification(_testUserId, File.ReadAllText(xmlPayloadLocation));

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                FactDO fact = uow.FactRepository.GetAll().Where(f => f.Activity == "Received").SingleOrDefault();
                Assert.IsNotNull(fact);
            }
        }

        [Test]
        public void ProcessService_CanRetrieveValidProcesses()
        {
            //Arrange 
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (ProcessDO p in FixtureData.GetProcesses())
                {
                    uow.ProcessRepository.Add(p);
                }
                uow.SaveChanges();
            }

            //Act
            var processList = _processService.GetProcessListForUser(_testUserId);

            //Assert
            Assert.AreEqual(2, processList.Count());
        }

        [Test]
        public void ProcessService_CanCreateProcessProcessingAlert()
        {
            //Arrange 
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (ProcessDO p in FixtureData.GetProcesses())
                {
                    uow.ProcessRepository.Add(p);
                }
                uow.SaveChanges();
            }
            string xmlPayloadLocation = "../../Content/DocusignXmlPayload.xml";

            //Act
            _processService.HandleDocusignNotification(_testUserId, File.ReadAllText(xmlPayloadLocation));

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<FactDO> fact = uow.FactRepository.GetAll().Where(f => f.Activity == "Processed");
                Assert.AreEqual(2, fact.Count());
            }
        }
    }
}
