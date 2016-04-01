using System;
using System.Linq;
// This alias is used to avoid ambiguity between StructureMap.IContainer and Core.Interfaces.IContainer
using InternalInterface = Hub.Interfaces;
// This alias is used to avoid ambiguity between StructureMap.Container and Core.Services.Container
using InternalClass = Hub.Services;
using Hub.Services;
using Data.Entities;
using Data.Interfaces;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
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
        [Test]
        public async Task Execute_MoveToNextActivity_ProcessCurrentAndNextActivity()
        {
            var _activity = new Mock<IPlanNode>();
            _activity
                .Setup(c => c.Process(It.IsAny<Guid>(), It.IsAny<ActivityExecutionMode>(), It.IsAny<ContainerDO>()))
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
            _activity.Verify(p => p.Process(It.IsAny<Guid>(), It.IsAny<ActivityExecutionMode>(), It.IsAny<ContainerDO>()));
        }
        
        
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Execute_SetCurrentActivityNull_ThrowsException()
        {
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                await _container.Run(uow, FixtureData.TestContainerCurrentActivityNULL());
            }
        }*/

    }
}
