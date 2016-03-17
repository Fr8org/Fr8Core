using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Constants;
using Moq;
using StructureMap;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using NUnit.Framework;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Hub.Managers;
using InternalInterface = Hub.Interfaces;
using InternalClasses = Hub.Services;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("Plan")]
    public class PlanTests : BaseTest
    {
        private Hub.Interfaces.IPlan _planService;
        private InternalInterface.IContainer _container;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
            _planService = ObjectFactory.GetInstance<IPlan>();

        }

        //        [Test]
        //        public void PlanService_GetSubPlans()
        //        {
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                var curPlanDO = FixtureData.TestPlanWithSubroutes();
        //                uow.PlanRepository.Add(curPlanDO);
        //                uow.SaveChanges();
        //
        //                var curSubroutes = _planService.GetSubroutes(curPlanDO);
        //
        //                Assert.IsNotNull(curSubroutes);
        //                Assert.AreEqual(curPlanDO.Subroutes.Count(), curSubroutes.Count);
        //            }
        //        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to PlanRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void PlanService_CanCreate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlanDO = FixtureData.TestPlan_CanCreate();
                var curUserAccount = FixtureData.TestDockyardAccount1();
                curPlanDO.Fr8Account = curUserAccount;

                _planService.CreateOrUpdate(uow, curPlanDO, false);

                uow.SaveChanges();

                var result = uow.PlanRepository.GetById<PlanDO>(curPlanDO.Id);
                Assert.NotNull(result);
                Assert.AreNotEqual(result.Id, 0);
                Assert.NotNull(result.StartingSubPlan);
                Assert.AreEqual(result.SubPlans.Count(), 1);
                Assert.AreEqual(result.StartingSubPlan.ChildNodes.Count, 2);
            }
        }

        [Test]
        public void PlanService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curPlanDO = FixtureData.TestPlanWithStartingSubPlans_ID0();
                uow.PlanRepository.Add(curPlanDO);
                uow.SaveChanges();

                Assert.AreNotEqual(curPlanDO.Id, 0);

                var currPlanDOId = curPlanDO.Id;
                _planService.Delete(uow, curPlanDO.Id);
                var result = uow.PlanRepository.GetById<PlanDO>(currPlanDOId);

                Assert.NotNull(result);
            }
        }

        [Test]
        [Ignore("ActivityTemplates are not being added to ActivityTemplate respository. Should be fixed if test is needed")]
        public void Activate_NoMatchingParentActivityId_ReturnsNoActivity()
        {
            var curPlanDO = FixtureData.TestPlanNoMatchingParentActivity();

            var result = _planService.Activate(curPlanDO.Id, true).Result;

            Assert.AreEqual(result.Status, "no activity");
        }

        [Test]
        public void PlanService_Can_RunWithoutExceptions()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Create a Plan 

                var curPlan = FixtureData.TestPlanWithSubscribeEvent();

                //Create activity mock to process the actions
                Mock<IPlanNode> activityMock = new Mock<IPlanNode>(MockBehavior.Default);
                activityMock.Setup(a => a.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActivityState>(), It.IsAny<ContainerDO>())).Returns(Task.Delay(1));
                activityMock.Setup(a => a.HasChildren(It.Is<PlanNodeDO>(r => r.Id == curPlan.StartingSubPlan.Id))).Returns(true);
                activityMock.Setup(a => a.HasChildren(It.Is<PlanNodeDO>(r => r.Id != curPlan.StartingSubPlan.Id))).Returns(false);
                activityMock.Setup(a => a.GetFirstChild(It.IsAny<PlanNodeDO>())).Returns(curPlan.ChildNodes.First().ChildNodes.First());
                ObjectFactory.Container.Inject(typeof(IPlanNode), activityMock.Object);

                //Act
                _planService = ObjectFactory.GetInstance<IPlan>();// new InternalClasses.Plan();
                _planService.Run(curPlan, FixtureData.TestDocuSignEventCrate());

                //Assert
                //since we have only one action in the template, the process should be called exactly once
                activityMock.Verify(activity => activity.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActivityState>(), It.IsAny<ContainerDO>()), Times.Exactly(1));
            }
        }

        //get this working again once 1124 is merged
        [Test]
        public void PlanService_Can_CreateContainer()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = FixtureData.TestPlanWithStartingSubPlanAndActivityList();

                uow.PlanRepository.Add(plan);
                uow.SaveChanges();

                var container = _planService.Create(uow, plan.Id, FixtureData.GetEnvelopeIdCrate());
                Assert.IsNotNull(container);
                Assert.IsTrue(container.Id != Guid.Empty);
            }
        }
    }
}