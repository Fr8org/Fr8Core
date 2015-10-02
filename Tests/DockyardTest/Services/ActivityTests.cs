using Core.Interfaces;
using Core.Services;
using Core.StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Moq;
using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;

namespace DockyardTest.Services
{
    [TestFixture]
    [Category("Activity")]
    public class ActivityTests : BaseTest
    {
        private IActivity _activity;
        private Mock<IAction> _actionMock;
        private ProcessNodeTemplateDO _curProcessNodeTemplate;
        private ActionListDO _curActionList;
        [SetUp]
        public override void SetUp()
        {
 	        base.SetUp();
            _activity = ObjectFactory.GetInstance<IActivity>();

        }


        [Test]
        
        public void Activity_CanGetUpstreamActivities()
        {
            //load tree
            //set cur activity to id 57
            //verify that the count of upstream activities is 10
            //print out the list of activity ids
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityRepository.Add(FixtureData.TestActivityTree());
                uow.SaveChanges();


                ActivityDO curActivity = FixtureData.TestActivity57();

                List<ActivityDO> upstreamActivities = _activity.GetUpstreamActivities(uow, curActivity);
                foreach (var activity in upstreamActivities)
                {
                    Debug.WriteLine(activity.Id);
                }

                Assert.AreEqual(10, upstreamActivities.Count());

            }
        }

        [Test]
        public void Activity_CanGetDownstreamActivities()
        {
            //load tree
            //set cur activity to id 57
            //verify that the count of upstream activities is 10
            //print out the list of activity ids
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityRepository.Add(FixtureData.TestActivityTree());
                uow.SaveChanges();


                ActivityDO curActivity = FixtureData.TestActivity57();

                List<ActivityDO> downstreamActivities = _activity.GetDownstreamActivities(uow, curActivity);
                foreach (var activity in downstreamActivities)
                {
                    Debug.WriteLine(activity.Id);
                }

                Assert.AreEqual(9, downstreamActivities.Count());

            }
        }


        private void InitializeActionList()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Add a template
                _curProcessNodeTemplate = FixtureData.TestProcessNodeTemplateDO1();
                uow.ProcessNodeTemplateRepository.Add(_curProcessNodeTemplate);
                uow.SaveChanges();

                _curActionList = FixtureData.TestActionList();
                _curActionList.ActionListType = ActionListType.Immediate;
                _curActionList.CurrentActivity = null;
                _curActionList.ProcessNodeTemplateID = _curProcessNodeTemplate.Id;

                uow.ActionListRepository.Add(_curActionList);
                uow.SaveChanges();
            }
        }

    }
}
