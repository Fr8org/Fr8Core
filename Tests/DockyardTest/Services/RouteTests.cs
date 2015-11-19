using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("Route")]
    public class RouteTests : BaseTest
    {
        private IRoute _processTemplateService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _processTemplateService = ObjectFactory.GetInstance<IRoute>();
        }



        [Test]
        public void RouteService_GetSubroutes()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curRouteDO = FixtureData.TestRouteWithSubroutes();
                uow.RouteRepository.Add(curRouteDO);
                uow.SaveChanges();

                var curSubroutes = _processTemplateService.GetSubroutes(curRouteDO);

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
                _processTemplateService.CreateOrUpdate(uow, curRouteDO, false);
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
                _processTemplateService.Delete(uow, curRouteDO.Id);
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
            
            string result = _processTemplateService.Activate(curRouteDO).Result;

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
    }
}