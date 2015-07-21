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
        private string _xmlPayloadFullPath;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _processService = ObjectFactory.GetInstance<IProcess>();
            _xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
            if (_xmlPayloadFullPath == string.Empty)
                throw new Exception("XML payload file for testing DocuSign notification is not found.");

        }

        [Test]
        [Ignore]
        public void ProcessService_CanExtractEnvelopeData()
        {
            string envelopeId = _processService.GetEnvelopeIdFromXml(File.ReadAllText(_xmlPayloadFullPath));
            Assert.AreEqual("0aa561b8-b4d9-47e0-a615-2367971f876b", envelopeId);
        }

        [Test]
        [Ignore]
        [ExpectedException(typeof(ArgumentException))]
        public void ProcessService_ThrowsIfXmlInvalid()
        {
            _processService.HandleDocusignNotification(_testUserId, File.ReadAllText(_xmlPayloadFullPath.Replace(".xml", "_invalid.xml")));
        }

        [Test]
        [Ignore]
        public void ProcessService_NotificationReceivedAlertCreated()
        {
            _processService.HandleDocusignNotification(_testUserId, File.ReadAllText(_xmlPayloadFullPath));

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
        [Ignore]
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

            //Act
            _processService.HandleDocusignNotification(_testUserId, File.ReadAllText(_xmlPayloadFullPath));

            //Assert
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                IEnumerable<FactDO> fact = uow.FactRepository.GetAll().Where(f => f.Activity == "Processed");
                Assert.AreEqual(2, fact.Count());
            }
        }
    }
}
