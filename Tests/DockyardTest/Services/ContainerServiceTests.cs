using System;
using System.Linq;
using System.Threading.Tasks;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using Data.Constants;
using InternalInterface = Hub.Interfaces;
using Hub.Interfaces;
// This alias is used to avoid ambiguity between StructureMap.Container and Core.Services.Container
using InternalClass = Hub.Services;
using Hub.Services;
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
using Newtonsoft.Json;
using Hub.Managers;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ContainerService")]
    public class ContainerServiceTests : BaseTest
    {
        
        private InternalInterface.IContainer _container;
        //private IDocuSignNotification _docuSignNotificationService;
        private EventReporter _eventReporter;
        private Fr8Account _userService;
        private string _testUserId = "testuser";
        private string xmlPayloadFullPath;
        ProcessNodeDO processNodeDO;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
            _userService = ObjectFactory.GetInstance<Fr8Account>();
            _eventReporter = ObjectFactory.GetInstance <EventReporter>();
            //_docuSignNotificationService = ObjectFactory.GetInstance<IDocuSignNotification>();

            xmlPayloadFullPath = FixtureData.FindXmlPayloadFullPath(Environment.CurrentDirectory);
            if (xmlPayloadFullPath == string.Empty)
                throw new Exception("XML payload file for testing DocuSign notification is not found.");

            processNodeDO = FixtureData.TestProcessNode2();

        }

        [Test]
        public void ContainerService_CanRetrieveValidContainers()
        {
            //Arrange 
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan5();
                uow.UserRepository.Add(plan.Fr8Account);
                uow.PlanRepository.Add(plan);
                foreach (var container in FixtureData.GetContainers())
                {
                    uow.ContainerRepository.Add(container);
                }
                uow.SaveChanges();
            }

            //Act
            var containerList = _userService.GetContainerList(_testUserId);

            //Assert
            Assert.AreEqual(2, containerList.Count());
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
                var plan = FixtureData.TestPlanWithStartingSubroutes();

                uow.EnvelopeRepository.Add(envelope);
                uow.RouteRepository.Add(plan);
                uow.SaveChanges();

                //Act
                ProcessDO curProcess = _processService.Create(plan.Id, FixtureData.GetEnvelopeIdCrate(envelope.DocusignEnvelopeId));

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
                var plan = FixtureData.TestPlan1();

                uow.EnvelopeRepository.Add(envelope);
                uow.RouteRepository.Add(plan);
                uow.SaveChanges();

                //Act
                ProcessDO curProcess = _processService.Create(plan.Id, FixtureData.GetEnvelopeIdCrate(envelope.DocusignEnvelopeId));

                //Assert
                int expectedProcessId = curProcess.ProcessNodes.First().ParentProcessId;
                int actualprocessId = uow.ProcessNodeRepository.GetByKey(curProcess.ProcessNodes.First().Id).Id;
                Assert.AreEqual(expectedProcessId, actualprocessId);
            }
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessService_CanNot_CreateProcessWithIncorrectPlan()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                const int incorrectRouteId = 2;

                var envelope = FixtureData.TestEnvelope1();

                uow.EnvelopeRepository.Add(envelope);
                uow.SaveChanges();
                _processService.Create(incorrectPlanId, FixtureData.GetEnvelopeIdCrate(envelope.DocusignEnvelopeId));
            }
        }
*/

        [Test]
        public async Task Execute_MoveToNextActivity_ProcessCurrentAndNextActivity()
        {
            var _activity = new Mock<IPlanNode>();
            _activity
                .Setup(c => c.Process(It.IsAny<Guid>(), It.IsAny<ActivityState>(), It.IsAny<ContainerDO>()))
                .Returns(Task.Delay(100))
                .Verifiable();
            ContainerDO containerDO = FixtureData.TestContainerWithCurrentActivityAndNextActivity();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlan2();

                plan.ChildNodes.AddRange(new[] {FixtureData.TestActivity10(), FixtureData.TestActivity7()});
                uow.PlanRepository.Add(plan);
                uow.ActivityTemplateRepository.Add(FixtureData.ActivityTemplate());
                uow.SaveChanges();
            }

            var originalCurrentActivityId = containerDO.CurrentPlanNodeId;
            _activity
                .Setup(c => c.GetNextSibling(It.Is<PlanNodeDO>((r) => r.Id == originalCurrentActivityId)))
                .Returns(FixtureData.TestActivity10());

            ObjectFactory.Configure(cfg => cfg.For<IPlanNode>().Use(_activity.Object));

            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _container.Run(uow, containerDO);
            }

            Assert.AreNotEqual(originalCurrentActivityId, containerDO.CurrentPlanNodeId);
            Assert.IsNull(containerDO.CurrentPlanNodeId);
            _activity.Verify(p => p.Process(It.IsAny<Guid>(), It.IsAny<ActivityState>(), It.IsAny<ContainerDO>()));
        }

        //        [Test]
        //        public async Task Execute_ProcessUntil3rdActivities_ProcessAllActivities()
        //        {
        //            var _activity = new Mock<IActivity>();
        //            _activity
        //                .Setup(c => c.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()))
        //                .Returns(Task.Delay(100))
        //                .Verifiable();
        //            //Setup 3rd ActivityDO
        //            _activity.Setup(c => c.GetNextActivities(It.IsAny<ActivityDO>())).Returns(new List<ActivityDO>() { FixtureData.TestAction9() });
        //            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
        //            _processService = ObjectFactory.GetInstance<IProcess>();
        //            ProcessDO processDO = FixtureData.TestProcesswithCurrentActivityAndNextActivity();
        //            ActivityDO originalCurrentActivity = processDO.CurrentActivity;
        //            ActivityDO originalNextActivity = processDO.NextActivity;
        //
        //            await _processService.Execute(processDO);
        //
        //            Assert.AreNotEqual(originalCurrentActivity, processDO.CurrentActivity);
        //            Assert.AreNotEqual(originalNextActivity, processDO.NextActivity);
        //            Assert.IsNull(processDO.CurrentActivity);
        //            _activity.Verify(p => p.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()));
        //        }

        //        [Test]
        //        public async Task Execute_SetNextActivityNull_ProcessCurrentActivity()
        //        {
        //            var _activity = new Mock<IActivity>();
        //            _activity
        //                .Setup(c => c.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()))
        //                .Returns(Task.Delay(100))
        //                .Verifiable();
        //            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
        //            _processService = ObjectFactory.GetInstance<IProcess>();
        //            ProcessDO processDO = FixtureData.TestProcesswithCurrentActivityAndNextActivity();
        //            processDO.NextActivity = null;
        //
        //            await _processService.Execute(processDO);
        //
        //            Assert.IsNull(processDO.CurrentActivity);
        //            _activity.Verify(p => p.Process(It.IsAny<int>(), It.IsAny<ProcessDO>()));
        //        }
        //
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Execute_SetCurrentActivityNull_ThrowsException()
        {
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _container.Run(uow, FixtureData.TestContainerCurrentActivityNULL());
            }
        }

    }
//
//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void SetProcessNextActivity_ProcessDOIsNull_ThorwArgumentNullException()
//        {
//            _processService = ObjectFactory.GetInstance<IProcess>();
//
//            _processService.SetProcessNextActivity(null);
//        }
//
//        [Test]
//        public void SetProcessNextActivity_ProcessDOCurrentActivityIsNull_NextActivityIsNull()
//        {
//            _processService = ObjectFactory.GetInstance<IProcess>();
//            ProcessDO process = FixtureData.TestProcessCurrentActivityNULL();
//            process.NextActivity = FixtureData.TestAction2();
//
//            _processService.SetProcessNextActivity(process);
//
//            Assert.IsNull(process.CurrentActivity);
//            Assert.IsNull(process.NextActivity);
//        }
//
//        [Test]
//        public void SetProcessNextActivity_ProcessDONextActivitySameAsCurrentActivity_NextActivityIsNull()
//        {
//            _processService = ObjectFactory.GetInstance<IProcess>();
//            ProcessDO process = FixtureData.TestProcesswithCurrentActivityAndNextActivityTheSame();
//
//            _processService.SetProcessNextActivity(process);
//
//            Assert.IsNull(process.NextActivity);
//        }
//
//        [Test]
//        public void SetProcessNextActivity_ProcessDOGetNextActivity_NewNextActivity()
//        {
//            var _activity = new Mock<IActivity>();
//            _activity
//                .Setup(c => c.GetNextActivities(It.IsAny<ActivityDO>()))
//                .Returns(new List<ActivityDO>() { FixtureData.TestAction8() });
//            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
//            _processService = ObjectFactory.GetInstance<IProcess>();
//            ProcessDO process = FixtureData.TestProcessSetNextActivity();
//
//            _processService.SetProcessNextActivity(process);
//
//            Assert.IsNotNull(process.NextActivity);
//            Assert.AreEqual(process.NextActivity.Id, FixtureData.TestAction8().Id);
//        }
//
//        [Test]
//        public void UpdateProcessNextActivity_ProcessDOCurrentActivityIsNull_NextActivityIsNull() 
//        {
//            //Arrange
//            _processService = ObjectFactory.GetInstance<IProcess>();
//            //Make use of PrivateObject class to reach the private method
//            var privateHelperProcessService = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(_processService);
//
//            //process with current activity being null
//            ProcessDO curProcess = FixtureData.TestProcessCurrentActivityNULL();
//            //and next activity being not null
//            curProcess.NextActivity = FixtureData.TestAction2();
//
//            var nextActivity = curProcess.NextActivity;
//            //Act
//            //Call private method to move next to current activity
//            privateHelperProcessService.Invoke("UpdateNextActivity", new ProcessDO[] { curProcess });
//
//            //Assert
//            Assert.IsNotNull(curProcess.CurrentActivity);
//            Assert.IsNull(curProcess.NextActivity);
//            Assert.AreEqual(nextActivity.Id, curProcess.CurrentActivity.Id);
//        }
//
//        [Test]
//        public void UpdateNextActivity_ActivityListIsNull_ProcessDONextActivitySameAsCurrentActivity_NextActivityIsNull()
//        {
//            //Arrange
//            _processService = ObjectFactory.GetInstance<IProcess>();
//            //Make use of PrivateObject class to reach the private method
//            var privateHelperProcessService = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(_processService);
//
//            //process with current activity being equal to the next activity
//            ProcessDO curProcess = FixtureData.TestProcesswithCurrentActivityAndNextActivityTheSame();
//
//            //Act
//            //Call private method to move next to current activity
//            privateHelperProcessService.Invoke("UpdateNextActivity", new ProcessDO[] { curProcess });
//
//            //Assert
//            Assert.IsNull(curProcess.NextActivity);
//        }
//
//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void UpdateNextActivity_ProcessDOIsNull_ThorwArgumentNullException()
//        {
//            _processService = ObjectFactory.GetInstance<IProcess>();
//            //Make use of PrivateObject class to reach the private method
//            var privateHelperProcessService = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(_processService);
//
//            //Call private method to move next to current activity
//            privateHelperProcessService.Invoke("UpdateNextActivity", new ProcessDO[] { null });
//        }
//
//        [Test]
//        public void UpdateNextActivity_ProcessDOCurrentActivitySameAsFirstOfGetNextActivity_NextActivityIsNull()
//        {
//            var curTestActivity = FixtureData.TestAction8();
//            //mock object that returns predefined value of TestAction8
//            var _activity = new Mock<IActivity>();
//            _activity
//                .Setup(c => c.GetNextActivities(It.IsAny<ActivityDO>()))
//                .Returns(new List<ActivityDO>() { curTestActivity });
//            ObjectFactory.Configure(cfg => cfg.For<IActivity>().Use(_activity.Object));
//
//            //Make use of PrivateObject class to reach the private method
//            var privateHelperProcessService = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(_processService);
//
//            //process with current activity being equal to the next activity
//            ProcessDO curProcess = FixtureData.TestProcessUpdateNextActivity();
//
//            //Act
//            //Call private method to move next to current activity
//            privateHelperProcessService.Invoke("UpdateNextActivity", new ProcessDO[] { curProcess });
//
//            //Assert
//            Assert.IsNull(curProcess.CurrentActivity);
//            Assert.IsNull(curProcess.NextActivity);
//        }
}
