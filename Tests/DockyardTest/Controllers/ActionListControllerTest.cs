using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;
using DockyardTest.Controllers.Api;

namespace DockyardTest.Controllers
{
    [TestFixture]
    [Category("ActionListController")]
    public class ActionListControllerTest : ApiControllerTestBase
    {
        private ProcessNodeTemplateDO _curProcessNodeTemplate;
        private ActionListDO _curActionList;
        private ActionListController _actionListController;

        public override void SetUp()
        {
            base.SetUp();
            InitializeActionList();
            _actionListController = CreateController<ActionListController>();
        }

        [Test]
        public void ActionListController_CanGetByProcessNodeTemplateId()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {


                var actionResult = _actionListController.GetByProcessNodeTemplateId(
                    _curProcessNodeTemplate.Id, ActionListType.Immediate);

                var okResult = actionResult as OkNegotiatedContentResult<ActionListDTO>;

                Assert.IsNotNull(okResult);
                Assert.IsNotNull(okResult.Content);
                Assert.AreEqual(okResult.Content.Id, _curActionList.Id);
            }
        }

        #region Private methods
        private void InitializeActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Add a template
                var processTemplate = FixtureData.TestProcessTemplate1();
                uow.ProcessTemplateRepository.Add(processTemplate);
                uow.SaveChanges();
                //Add a processnodetemplate to processtemplate 
                _curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                _curProcessNodeTemplate.StartingProcessNodeTemplate = true;
                _curProcessNodeTemplate.ParentTemplateId = processTemplate.Id;
                uow.ProcessNodeTemplateRepository.Add(_curProcessNodeTemplate);
                uow.SaveChanges();

                /*_curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                uow.ProcessNodeTemplateRepository.Add(_curProcessNodeTemplate);
                uow.SaveChanges();*/

                _curActionList = FixtureData.TestActionList();
                _curActionList.ActionListType = ActionListType.Immediate;
                _curActionList.CurrentActivity = null;
                _curActionList.ProcessNodeTemplateID = _curProcessNodeTemplate.Id;

                uow.ActionListRepository.Add(_curActionList);
                uow.SaveChanges();
            }
        }

        #endregion
    }

}
