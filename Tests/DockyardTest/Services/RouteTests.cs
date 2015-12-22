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
    [Category("Route")]
    public class RouteTests : BaseTest
    {
        private IRoute _routeService;
        private InternalInterface.IContainer _container;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _container = ObjectFactory.GetInstance<InternalInterface.IContainer>();
            _routeService = ObjectFactory.GetInstance<IRoute>();
           
        }

        [Test]
        public void RouteService_GetSubroutes()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRouteDO = FixtureData.TestRouteWithSubroutes();
                uow.RouteRepository.Add(curRouteDO);
                uow.SaveChanges();

                var curSubroutes = _routeService.GetSubroutes(curRouteDO);

                Assert.IsNotNull(curSubroutes);
                Assert.AreEqual(curRouteDO.Subroutes.Count(), curSubroutes.Count);
            }
        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to RouteRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void RouteService_CanCreate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRouteDO = FixtureData.TestRoute_CanCreate();
                var curUserAccount = FixtureData.TestDockyardAccount1();
                curRouteDO.Fr8Account = curUserAccount;
                _routeService.CreateOrUpdate(uow, curRouteDO, false);
                uow.SaveChanges();

                var result = uow.RouteRepository.GetByKey(curRouteDO.Id);
                Assert.NotNull(result);
                Assert.AreNotEqual(result.Id, 0);
                Assert.NotNull(result.StartingSubroute);
                Assert.AreEqual(result.Subroutes.Count(), 1);
                Assert.AreEqual(result.StartingSubroute.ChildNodes.Count, 2);
            }
        }

        [Test]
        public void RouteService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRouteDO = FixtureData.TestRouteWithStartingSubroutes_ID0();
                uow.RouteRepository.Add(curRouteDO);
                uow.SaveChanges();

                Assert.AreNotEqual(curRouteDO.Id, 0);

                var currRouteDOId = curRouteDO.Id;
                _routeService.Delete(uow, curRouteDO.Id);
                var result = uow.RouteRepository.GetByKey(currRouteDOId);

                Assert.NotNull(result);
            }
        }


//        [Test]
//        [Ignore("ActionState will be removed and is not used")]
//        public void Activate_HasParentActivity_SetActionStateActive()
//        {
//            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
//            var _action = new Mock<IAction>();
//            _action
//                .Setup(c => c.Activate(It.IsAny<ActionDO>()))
//                .Returns(Task.FromResult(new ActionDTO()));
//            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_action.Object));
//            _processTemplateService = ObjectFactory.GetInstance<IProcessTemplate>();
//
//            string result = _processTemplateService.Activate(curProcessTemplateDO);
//
//
//            Assert.AreEqual(result, "success");
//            var activities = curProcessTemplateDO.ProcessNodeTemplates.SelectMany(s => s.Activities).SelectMany(s => s.Activities);
//            foreach (ActionDO curActionDO in activities)
//            {
//                Assert.AreEqual(curActionDO.ActionState, ActionState.Active);
//            }
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void Activate_SubroutesIsNULL_ThrowsArgumentNULLException()
//        {
//            var curRouteDO = FixtureData.TestRoute3();
//            curRouteDO.Subroutes = null;
//
//            string result = _processTemplateService.Activate(curRouteDO);
//        }

        [Test]
        [Ignore("ActivityTemplates are not being added to ActivityTemplate respository. Should be fixed if test is needed")]
        public void Activate_NoMatchingParentActivityId_ReturnsNoAction()
        {
            var curRouteDO = FixtureData.TestRouteNoMatchingParentActivity();
            
            string result = _routeService.Activate(curRouteDO).Result;

            Assert.AreEqual(result, "no action");
        }



        //[Test]
        //      public void TemplateRegistrationCollections_ShouldMakeIdentical()
        //      {
        //          var curSubscriptions = FixtureData.DocuSignTemplateSubscriptionList1();
        //          var newSubscriptions = FixtureData.DocuSignTemplateSubscriptionList2();
        //          using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //          {
        //              _processTemplateService.MakeCollectionEqual(uow, curSubscriptions, newSubscriptions);
        //          }
        //          CollectionAssert.AreEquivalent(newSubscriptions, curSubscriptions);
        //      }

        [Test]
        public void RouteService_Can_RunWithoutExceptions()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Arrange
                //Create a Route 
                
                var curRoute = FixtureData.TestRouteWithSubscribeEvent();

                //Create activity mock to process the actions
                Mock<IRouteNode> activityMock = new Mock<IRouteNode>(MockBehavior.Default);
                activityMock.Setup(a => a.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActionState>(), It.IsAny<ContainerDO>())).Returns(Task.Delay(1));
                activityMock.Setup(a => a.HasChildren(It.IsAny<RouteNodeDO>())).Returns(true);
                activityMock.Setup(a => a.GetFirstChild(It.IsAny<RouteNodeDO>())).Returns(curRoute.ChildNodes.First().ChildNodes.First());
                ObjectFactory.Container.Inject(typeof(IRouteNode), activityMock.Object);

                //Act
                _routeService = new InternalClasses.Route();
                _routeService.Run(curRoute, FixtureData.TestDocuSignEventCrate());

                //Assert
                //since we have only one action in the template, the process should be called exactly once
                activityMock.Verify(activity => activity.Process(FixtureData.GetTestGuidById(1), It.IsAny<ActionState>(), It.IsAny<ContainerDO>()), Times.Exactly(1));
            }
        }

        //get this working again once 1124 is merged
        [Test]
        public void RouteService_Can_CreateContainer()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var route = FixtureData.TestRouteWithStartingSubrouteAndActionList();

                uow.RouteRepository.Add(route);
                uow.SaveChanges();

                var container = _routeService.Create(uow, route.Id, FixtureData.GetEnvelopeIdCrate());
                Assert.IsNotNull(container);
                Assert.IsTrue(container.Id != Guid.Empty);
            }
        }
    }
}