using Core.Interfaces;
using Data.Entities;
using Data.Exceptions;
using Data.Interfaces;
using Data.States;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using System.Linq;
using Moq;
using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;
using System.Threading.Tasks;
using System;
namespace DockyardTest.Services
{
    [TestFixture]
    [Category("ProcessTemplate")]
    public class ProcessTemplateTests : BaseTest
    {
        private IProcessTemplate _processTemplateService;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _processTemplateService = ObjectFactory.GetInstance<IProcessTemplate>();
        }



        [Test]
        public void ProcessTemplateService_GetProcessNodeTemplates()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplateDO = FixtureData.TestProcessTemplateWithProcessNodeTemplates();
                uow.ProcessTemplateRepository.Add(curProcessTemplateDO);
                uow.SaveChanges();

                var curProcessNodeTemplates = _processTemplateService.GetProcessNodeTemplates(curProcessTemplateDO);

                Assert.IsNotNull(curProcessNodeTemplates);
                Assert.AreEqual(curProcessTemplateDO.ProcessNodeTemplates.Count(), curProcessNodeTemplates.Count);
            }
        }

        // MockDB has boken logic when working with collections of objects of derived types
        // We add object to ProcessTemplateRepository but Delete logic recusively traverse Activity repository.
        [Ignore("MockDB behavior is incorrect")]
        [Test]
        public void ProcessTemplateService_CanCreate()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplateDO = FixtureData.TestProcessTemplate_CanCreate();
                var curUserAccount = FixtureData.TestDockyardAccount1();
                curProcessTemplateDO.DockyardAccount = curUserAccount;
                _processTemplateService.CreateOrUpdate(uow, curProcessTemplateDO, false);
                uow.SaveChanges();

                var result = uow.ProcessTemplateRepository.GetByKey(curProcessTemplateDO.Id);
                Assert.NotNull(result);
                Assert.AreNotEqual(result.Id, 0);
                Assert.NotNull(result.StartingProcessNodeTemplate);
                Assert.AreEqual(result.ProcessNodeTemplates.Count(), 1);
                Assert.AreEqual(result.StartingProcessNodeTemplate.Activities.Count, 2);
            }
        }

        [Test]
        public void ProcessTemplateService_CanDelete()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessTemplateDO = FixtureData.TestProcessTemplateWithStartingProcessNodeTemplates_ID0();
                uow.ProcessTemplateRepository.Add(curProcessTemplateDO);
                uow.SaveChanges();

                Assert.AreNotEqual(curProcessTemplateDO.Id, 0);

                var currProcessTemplateDOId = curProcessTemplateDO.Id;
                _processTemplateService.Delete(uow, curProcessTemplateDO.Id);
                var result = uow.ProcessTemplateRepository.GetByKey(currProcessTemplateDOId);

                Assert.NotNull(result);
            }
        }


        [Test]
        [Ignore("ActionState will be removed and is not used")]
        public void Activate_HasParentActivity_SetActionStateActive()
        {
            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
            var _action = new Mock<IAction>();
            _action
                .Setup(c => c.Activate(It.IsAny<ActionDO>()))
                .Returns(Task.FromResult(new ActionDTO()));
            ObjectFactory.Configure(cfg => cfg.For<IAction>().Use(_action.Object));
            _processTemplateService = ObjectFactory.GetInstance<IProcessTemplate>();

            string result = _processTemplateService.Activate(curProcessTemplateDO);


            Assert.AreEqual(result, "success");
            var activities = curProcessTemplateDO.ProcessNodeTemplates.SelectMany(s => s.Activities).SelectMany(s => s.Activities);
            foreach (ActionDO curActionDO in activities)
            {
                Assert.AreEqual(curActionDO.ActionState, ActionState.Active);
            }
        }

//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void Activate_ProcessNodeTemplatesIsNULL_ThrowsArgumentNULLException()
//        {
//            var curProcessTemplateDO = FixtureData.TestProcessTemplate3();
//            curProcessTemplateDO.ProcessNodeTemplates = null;
//
//            string result = _processTemplateService.Activate(curProcessTemplateDO);
//        }

        [Test]
        [Ignore("ActivityTemplates are not being added to ActivityTemplate respository. Should be fixed if test is needed")]
        public void Activate_NoMatchingParentActivityId_ReturnsNoAction()
        {
            var curProcessTemplateDO = FixtureData.TestProcessTemplateNoMatchingParentActivity();
            
            string result = _processTemplateService.Activate(curProcessTemplateDO);

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