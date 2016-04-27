using System;
using System.Linq;
using System.Web.Http.Results;
using NUnit.Framework;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Services;
using HubWeb.Controllers;
using HubWeb.ViewModels;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using HubTests.Controllers.Api;

namespace HubTests.Controllers
{
    [TestFixture]
    [Category("ActionListController")]
    public class ActivityListControllerTest : ApiControllerTestBase
    {
        private SubPlanDO _curSubPlan;
        private ActionListController _activityListController;

        public override void SetUp()
        {
            base.SetUp();
            // DO-1214
            //InitializeActionList();
            _activityListController = CreateController<ActionListController>();
        }

        [Test]
        public void ActivityListController_ShouldHaveFr8ApiAuthorize()
        {
            ShouldHaveFr8ApiAuthorize(typeof(ActionListController));
        }

        // DO-1214
        //        [Test]
        //        public void ActionListController_CanGetBySubrouteId()
        //        {
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //
        //
        //                var actionResult = _actionListController.GetBySubrouteId(
        //                    _curSubroute.Id, ActionListType.Immediate);
        //
        //                var okResult = actionResult as OkNegotiatedContentResult<ActionListDTO>;
        //
        //                Assert.IsNotNull(okResult);
        //                Assert.IsNotNull(okResult.Content);
        //                Assert.AreEqual(okResult.Content.Id, _curActionList.Id);
        //            }
        //        }
        //
        //        #region Private methods
        //        private void InitializeActionList()
        //        {
        //            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //            {
        //                //Add a template
        //                var curPlan = FixtureData.TestRoute1();
        //                uow.RouteRepository.Add(curPlan);
        //                uow.SaveChanges();
        //
        //                _curSubroute = FixtureData.TestSubrouteDO1();
        //                _curSubroute.ParentTemplateId = curPlan.Id;
        //                uow.SubrouteRepository.Add(_curSubroute);
        //                uow.SaveChanges();
        //
        //                /*_curSubroute = FixtureData.TestSubrouteDO1();
        //                uow.SubrouteRepository.Add(_curSubroute);
        //                uow.SaveChanges();*/
        //
        //                _curActionList = FixtureData.TestActionList();
        //                _curActionList.ActionListType = ActionListType.Immediate;
        //                _curActionList.CurrentActivity = null;
        //                _curActionList.SubrouteID = _curSubroute.Id;
        //
        //                uow.ActionListRepository.Add(_curActionList);
        //                uow.SaveChanges();
        //            }
        //        }
        //
        //        #endregion
    }

}
