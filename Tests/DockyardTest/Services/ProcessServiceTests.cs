using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Data.Interfaces.DataTransferObjects;
using System.Collections.Generic;
using Moq;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ProcessService")]
    public class ProcessServiceTests : BaseTest
    {
        private IProcess _processService;
        //private IDocuSignNotification _docuSignNotificationService;
        private DockyardAccount _userService;
        private string _testUserId = "testuser";
        private string xmlPayloadFullPath;
        DocuSignEventDO docusignEventDO;
        ProcessNodeDO processNodeDO;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _processService = ObjectFactory.GetInstance<IProcess>();
            _userService = ObjectFactory.GetInstance<DockyardAccount>();
            //_docuSignNotificationService = ObjectFactory.GetInstance<IDocuSignNotification>();

            xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
            if (xmlPayloadFullPath == string.Empty)
                throw new Exception("XML payload file for testing DocuSign notification is not found.");

            docusignEventDO = FixtureData.TestDocuSignEvent1();
            processNodeDO = FixtureData.TestProcessNode2();
        }

        [Test]
        public void ProcessService_CanRetrieveValidProcesses()
        {
            //Arrange 
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var processTemplate = FixtureData.TestProcessTemplate2();
                uow.ProcessTemplateRepository.Add(processTemplate);
                foreach (var p in FixtureData.GetProcesses())
                {
                    uow.ProcessRepository.Add(p);
                }
                uow.SaveChanges();
            }

            //Act
            var processList = _userService.GetProcessList(_testUserId);

            //Assert
            Assert.AreEqual(2, processList.Count());
        }

        //get this working again once 1124 is merged
        [Test]
        public void ProcessService_Can_CreateProcess()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var envelopeCrate = FixtureData.EnvelopeIdCrateJson();
                var processTemplate = FixtureData.TestProcessTemplateWithStartingProcessNodeTemplateAndActionList();

                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.SaveChanges();

                var process = _processService.Create(processTemplate.Id, FixtureData.GetEnvelopeIdCrate());
                Assert.IsNotNull(process);
                Assert.IsTrue(process.Id > 0);
            }
        }

/*
        //get this working again once 1124 is merged
        [Test,Ignore]
        public void Process_CanAccessProcessNodes()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                var envelope = FixtureData.TestEnvelope1();
                var processTemplate = FixtureData.TestProcessTemplateWithStartingProcessNodeTemplates();

                uow.EnvelopeRepository.Add(envelope);
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.SaveChanges();

                //Act
                ProcessDO curProcess = _processService.Create(processTemplate.Id, FixtureData.GetEnvelopeIdCrate(envelope.DocusignEnvelopeId));

                //Assert
                int expectedProcessNodeCount = uow.ProcessNodeRepository.GetAll().Count();
                int actualprocessNodeCount = curProcess.ProcessNodes.Count;
                Assert.AreEqual(expectedProcessNodeCount, actualprocessNodeCount);
            }
        }

        public void ProcessNode_CanAccessParentProcess()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                var envelope = FixtureData.TestEnvelope1();
                var processTemplate = FixtureData.TestProcessTemplate1();

                uow.EnvelopeRepository.Add(envelope);
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.SaveChanges();

                //Act
                ProcessDO curProcess = _processService.Create(processTemplate.Id, FixtureData.GetEnvelopeIdCrate(envelope.DocusignEnvelopeId));

                //Assert
                int expectedProcessId = curProcess.ProcessNodes.First().ParentProcessId;
                int actualprocessId = uow.ProcessNodeRepository.GetByKey(curProcess.ProcessNodes.First().Id).Id;
                Assert.AreEqual(expectedProcessId, actualprocessId);
            }
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessService_CanNot_CreateProcessWithIncorrectProcessTemplate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                const int incorrectProcessTemplateId = 2;

                var envelope = FixtureData.TestEnvelope1();

                uow.EnvelopeRepository.Add(envelope);
                uow.SaveChanges();
                _processService.Create(incorrectProcessTemplateId, FixtureData.GetEnvelopeIdCrate(envelope.DocusignEnvelopeId));
            }
        }
*/

        [Test]
        public void ProcessService_Can_LaunchWithoutExceptions()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Create a process template
                var curProcessTemplate = FixtureData.TestProcessTemplateWithSubscribeEvent();
                var curEvent = FixtureData.TestDocuSignEvent1();

                //Create activity mock to process the actions
                Mock<IActivity> activityMock = new Mock<IActivity>(MockBehavior.Default);
                activityMock.Setup(a => a.Process(1, It.IsAny<ProcessDO>())).Returns(Task.Delay(2));
                ObjectFactory.Container.Inject(typeof(IActivity), activityMock.Object);

                //Act
                _processService = new Process();
                _processService.Launch(curProcessTemplate, FixtureData.DocuSignEventToCrate(curEvent));

                //Assert
                //since we have only one action in the template, the process should be called exactly once
                activityMock.Verify(activity => activity.Process(1, It.IsAny<ProcessDO>()), Times.Exactly(1));
            }
        }

        [Test, Ignore("Process Execute has new implementation that uses CurrentActivity and NextActivity")]
        [ExpectedException(ExpectedMessage = "ProcessNode.NodeTransitions did not have a key matching the returned transition target from Critera")]
        public void Execute_NoMatchedNodeTransition_ThrowExceptionProcessNodeTransitions()
        {
            docusignEventDO = FixtureData.TestDocuSignEvent1();
            processNodeDO = FixtureData.TestProcessNode3();
            //mock processnode
            var processNodeMock = new Mock<IProcessNode>();
            processNodeMock
                .Setup(c => c.Execute(It.IsAny<List<EnvelopeDataDTO>>(), It.IsAny<ProcessNodeDO>()))
                .Returns("true1");
            ObjectFactory.Configure(cfg => cfg.For<IProcessNode>().Use(processNodeMock.Object));

            _processService = ObjectFactory.GetInstance<IProcess>();

            _processService.Execute(FixtureData.TestProcesswithCurrentActivityAndNextActivity());
        }

        [Test, Ignore("Process Execute has new implementation that uses CurrentActivity and NextActivity")]
        public void Execute_MatchedNodeTransition_ProcessNodeNull()
        {
            //mock processnode
            var processNodeMock = new Mock<IProcessNode>();
            processNodeMock
                .Setup(c => c.Execute(It.IsAny<List<EnvelopeDataDTO>>(), It.IsAny<ProcessNodeDO>()))
                .Returns("true");
            ObjectFactory.Configure(cfg => cfg.For<IProcessNode>().Use(processNodeMock.Object));
            //setup the next transition node during lookup key
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ProcessTemplateRepository.Add(FixtureData.TestProcessTemplate2());
                uow.ActionListRepository.Add(FixtureData.TestActionList6());
                uow.SaveChanges();

                uow.ProcessRepository.Add(FixtureData.TestProcess1());
                uow.SaveChanges();
                uow.ProcessNodeRepository.Add(FixtureData.TestProcessNode4());
                uow.SaveChanges();
            }
            _processService = ObjectFactory.GetInstance<IProcess>();

            docusignEventDO = FixtureData.TestDocuSignEvent1();
            var processNodeDO = FixtureData.TestProcessNode3();


            _processService.Execute(FixtureData.TestProcesswithCurrentActivityAndNextActivity());

            Assert.Pass();//just set to pass because processNodeDo parameter will be set to null(where caller object is unaware) and reaching this line is success
        }

        [Test]
        public async void Execute_MoveToNextActivity_ProcessCurrentAndNextActivity()
        {
            var _activity = new Mock<IActivity>();
            _activity
                .Setup(c => c.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()))
                .Returns(Task.Delay(100))
                .Verifiable();
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
            _processService = ObjectFactory.GetInstance<IProcess>();
            ProcessDO processDO = FixtureData.TestProcesswithCurrentActivityAndNextActivity();
            ActivityDO originalCurrentActivity = processDO.CurrentActivity;
            ActivityDO originalNextActivity = processDO.NextActivity;

            await _processService.Execute(processDO);

            Assert.AreNotEqual(originalCurrentActivity, processDO.CurrentActivity);
            Assert.AreNotEqual(originalNextActivity, processDO.NextActivity);
            Assert.IsNull(processDO.CurrentActivity);
            _activity.Verify(p => p.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()));
        }

        [Test]
        public async void Execute_ProcessUntil3rdActivities_ProcessAllActivities()
        {
            var _activity = new Mock<IActivity>();
            _activity
                .Setup(c => c.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()))
                .Returns(Task.Delay(100))
                .Verifiable();
            //Setup 3rd ActivityDO
            _activity.Setup(c => c.GetNextActivities(It.IsAny<ActivityDO>())).Returns(new List<ActivityDO>() { FixtureData.TestAction9() });
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
            _processService = ObjectFactory.GetInstance<IProcess>();
            ProcessDO processDO = FixtureData.TestProcesswithCurrentActivityAndNextActivity();
            ActivityDO originalCurrentActivity = processDO.CurrentActivity;
            ActivityDO originalNextActivity = processDO.NextActivity;

            await _processService.Execute(processDO);

            Assert.AreNotEqual(originalCurrentActivity, processDO.CurrentActivity);
            Assert.AreNotEqual(originalNextActivity, processDO.NextActivity);
            Assert.IsNull(processDO.CurrentActivity);
            _activity.Verify(p => p.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()));
        }

        [Test]
        public async void Execute_SetNextActivityNull_ProcessCurrentActivity()
        {
            var _activity = new Mock<IActivity>();
            _activity
                .Setup(c => c.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()))
                .Returns(Task.Delay(100))
                .Verifiable();
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
            _processService = ObjectFactory.GetInstance<IProcess>();
            ProcessDO processDO = FixtureData.TestProcesswithCurrentActivityAndNextActivity();
            processDO.NextActivity = null;

            await _processService.Execute(processDO);

            Assert.IsNull(processDO.CurrentActivity);
            _activity.Verify(p => p.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async void Execute_SetCurrentActivityNull_ThrowsException()
        {
            _processService = ObjectFactory.GetInstance<IProcess>();

            await _processService.Execute(FixtureData.TestProcessCurrentActivityNULL());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetProcessNextActivity_ProcessDOIsNull_ThorwArgumentNullException()
        {
            _processService = ObjectFactory.GetInstance<IProcess>();

            _processService.SetProcessNextActivity(null);
        }

        [Test]
        public void SetProcessNextActivity_ProcessDOCurrenctActivityIsNull_NextActivityIsNull()
        {
            _processService = ObjectFactory.GetInstance<IProcess>();
            ProcessDO process = FixtureData.TestProcessCurrentActivityNULL();
            process.NextActivity = FixtureData.TestAction2();

            _processService.SetProcessNextActivity(process);

            Assert.IsNull(process.CurrentActivity);
            Assert.IsNull(process.NextActivity);
        }

        [Test]
        public void SetProcessNextActivity_ProcessDONextActivitySameAsCurrentActivity_NextActivityIsNull()
        {
            _processService = ObjectFactory.GetInstance<IProcess>();
            ProcessDO process = FixtureData.TestProcesswithCurrentActivityAndNextActivityTheSame();

            _processService.SetProcessNextActivity(process);

            Assert.IsNull(process.NextActivity);
        }

        [Test]
        public void SetProcessNextActivity_ProcessDOGetNextActivity_NewNextActivity()
        {
            var _activity = new Mock<IActivity>();
            _activity
                .Setup(c => c.GetNextActivities(It.IsAny<ActivityDO>()))
                .Returns(new List<ActivityDO>() { FixtureData.TestAction8() });
            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
            _processService = ObjectFactory.GetInstance<IProcess>();
            ProcessDO process = FixtureData.TestProcessSetNextActivity();

            _processService.SetProcessNextActivity(process);

            Assert.IsNotNull(process.NextActivity);
            Assert.AreEqual(process.NextActivity.Id, FixtureData.TestAction8().Id);
        }
    }
}