using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Core.Interfaces;
using Core.Managers;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.PluginRegistrations;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using Moq;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Action = Core.Services.Action;
using System.Threading.Tasks;
using Utilities;
using Core.StructureMap;
using Data.Infrastructure.StructureMap;
using Web.App_Start;
using Data.Infrastructure;
using Core.Services;

namespace DockyardTest.Services
{
	[TestFixture]
	[Category("ActivityService")]
	public class ActivityServiceTests : BaseTest
	{
		private IActivity _activity;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			_activity = new Activity();
		}
		
		[Test]
		public void GetUpstreamActivities_ActionDOIsNull_ExpectedArgumentNullException()
		{
			var ex = Assert.Throws<ArgumentNullException>(() => _activity.GetUpstreamActivities(null));
			Assert.AreEqual("curActivityDO", ex.ParamName);
		}
		[Test]
		public void GetUpstreamActivities_1Level_ShoudBeOk()
		{
			var actionTempate = new ActivityTemplateDO()
			 {
				 Id = 1,
				 Version = "1"
			 };
			// Level 1
			ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			l1_aList.Activities.Add(l1_a1);
			l1_a1.ParentActivity = l1_aList;
			l1_aList.Activities.Add(l1_a2);
			l1_a2.ParentActivity = l1_aList;

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.ActionRepository.Add(l1_a1);
				uow.ActionRepository.Add(l1_a2);
				uow.ActionListRepository.Add(l1_aList);

				uow.SaveChanges();
			}
			var upstreamActivities = _activity.GetUpstreamActivities(l1_a2);

			Assert.AreEqual(2, upstreamActivities.Count);
			Assert.AreEqual(l1_a1, upstreamActivities[0]);
			Assert.AreEqual(l1_aList, upstreamActivities[1]);
		}
		[Test]
		public void GetUpstreamActivities_2Levels_ShoudBeOk()
		{
			var actionTempate = new ActivityTemplateDO()
			{
				Id = 1,
				Version = "1"
			};
			// Level 2
			ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			l2_aList.Activities.Add(l2_a1);
			l2_a1.ParentActivity = l2_aList;
			l2_aList.Activities.Add(l2_a2);
			l2_a2.ParentActivity = l2_aList;

			// Level 1
			ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			l1_aList.Activities.Add(l1_a1);
			l1_a1.ParentActivity = l1_aList;
			l1_aList.Activities.Add(l1_a2);
			l1_a2.ParentActivity = l1_aList;

			l2_aList.ParentActivity = l1_aList;

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.ActionRepository.Add(l2_a1);
				uow.ActionRepository.Add(l2_a2);
				uow.ActionListRepository.Add(l2_aList);

				uow.ActionRepository.Add(l1_a1);
				uow.ActionRepository.Add(l1_a2);
				uow.ActionListRepository.Add(l1_aList);

				uow.SaveChanges();
			}
			var upstreamActivities = _activity.GetUpstreamActivities(l2_a1);

			Assert.AreEqual(4, upstreamActivities.Count);
			Assert.AreEqual(l2_a1, upstreamActivities[0]);
			Assert.AreEqual(l2_aList, upstreamActivities[1]);
			Assert.AreEqual(l1_a1, upstreamActivities[2]);
			Assert.AreEqual(l1_aList, upstreamActivities[3]);
		}
		[Test]
		public void GetUpstreamActivities_3Levels_ShoudBeOk()
		{
			var actionTempate = new ActivityTemplateDO()
			{
				Id = 1,
				Version = "1"
			};
			// Level 3
			ActionDO l3_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l3_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionDO l3_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			ActionListDO l3_aList = new ActionListDO() { Ordering = 3, ActionListType = ActionListType.Immediate };
			l3_aList.Activities.Add(l3_a1);
			l3_a1.ParentActivity = l3_aList;
			l3_aList.Activities.Add(l3_a2);
			l3_a2.ParentActivity = l3_aList;
			l3_aList.Activities.Add(l3_a3);
			l3_a3.ParentActivity = l3_aList;

			// Level 2
			ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			l2_aList.Activities.Add(l2_a1);
			l2_a1.ParentActivity = l2_aList;
			l2_aList.Activities.Add(l2_a2);
			l2_a2.ParentActivity = l2_aList;

			// Level 1
			ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			l1_aList.Activities.Add(l1_a1);
			l1_a1.ParentActivity = l1_aList;
			l1_aList.Activities.Add(l1_a2);
			l1_a2.ParentActivity = l1_aList;

			l3_aList.ParentActivity = l2_aList;
			l2_aList.ParentActivity = l1_aList;

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.ActionRepository.Add(l3_a1);
				uow.ActionRepository.Add(l3_a2);
				uow.ActionRepository.Add(l3_a3);
				uow.ActionListRepository.Add(l3_aList);

				uow.ActionRepository.Add(l2_a1);
				uow.ActionRepository.Add(l2_a2);
				uow.ActionListRepository.Add(l2_aList);

				uow.ActionRepository.Add(l1_a1);
				uow.ActionRepository.Add(l1_a2);
				uow.ActionListRepository.Add(l1_aList);

				uow.SaveChanges();
			}
			var upstreamActivities = _activity.GetUpstreamActivities(l3_a3);

			Assert.AreEqual(6, upstreamActivities.Count);
			Assert.AreEqual(l3_a1, upstreamActivities[0]);
			Assert.AreEqual(l3_aList, upstreamActivities[1]);
			Assert.AreEqual(l2_a1, upstreamActivities[2]);
			Assert.AreEqual(l2_aList, upstreamActivities[3]);
			Assert.AreEqual(l1_a1, upstreamActivities[4]);
			Assert.AreEqual(l1_aList, upstreamActivities[5]);
		}
		[Test]
		public void GetUpstreamActivities_4Levels_ShoudBeOk()
		{
			var actionTempate = new ActivityTemplateDO()
			{
				Id = 1,
				Version = "1"
			};
			// Level 4
			ActionDO l4_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l4_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionDO l4_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			ActionListDO l4_aList = new ActionListDO() { Ordering = 4, ActionListType = ActionListType.Immediate };
			l4_aList.Activities.Add(l4_a1);
			l4_a1.ParentActivity = l4_aList;
			l4_aList.Activities.Add(l4_a2);
			l4_a2.ParentActivity = l4_aList;
			l4_aList.Activities.Add(l4_a3);
			l4_a3.ParentActivity = l4_aList;

			// Level 3
			ActionDO l3_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l3_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionDO l3_a3 = new ActionDO() { Id = 3, ActionTemplate = actionTempate };
			ActionListDO l3_aList = new ActionListDO() { Ordering = 3, ActionListType = ActionListType.Immediate };
			l3_aList.Activities.Add(l3_a1);
			l3_a1.ParentActivity = l3_aList;
			l3_aList.Activities.Add(l3_a2);
			l3_a2.ParentActivity = l3_aList;
			l3_aList.Activities.Add(l3_a3);
			l3_a3.ParentActivity = l3_aList;

			// Level 2
			ActionDO l2_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l2_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l2_aList = new ActionListDO() { Ordering = 2, ActionListType = ActionListType.Immediate };
			l2_aList.Activities.Add(l2_a1);
			l2_a1.ParentActivity = l2_aList;
			l2_aList.Activities.Add(l2_a2);
			l2_a2.ParentActivity = l2_aList;

			// Level 1
			ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate };
			ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate };
			ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			l1_aList.Activities.Add(l1_a1);
			l1_a1.ParentActivity = l1_aList;
			l1_aList.Activities.Add(l1_a2);
			l1_a2.ParentActivity = l1_aList;

			l4_aList.ParentActivity = l3_aList;
			l3_aList.ParentActivity = l2_aList;
			l2_aList.ParentActivity = l1_aList;

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.ActionRepository.Add(l3_a1);
				uow.ActionRepository.Add(l3_a2);
				uow.ActionRepository.Add(l3_a3);
				uow.ActionListRepository.Add(l3_aList);

				uow.ActionRepository.Add(l2_a1);
				uow.ActionRepository.Add(l2_a2);
				uow.ActionListRepository.Add(l2_aList);

				uow.ActionRepository.Add(l1_a1);
				uow.ActionRepository.Add(l1_a2);
				uow.ActionListRepository.Add(l1_aList);

				uow.SaveChanges();
			}
			var upstreamActivities = _activity.GetUpstreamActivities(l4_a3);

			Assert.AreEqual(8, upstreamActivities.Count);
			Assert.AreEqual(l4_a1, upstreamActivities[0]);
			Assert.AreEqual(l4_aList, upstreamActivities[1]);
			Assert.AreEqual(l3_a1, upstreamActivities[2]);
			Assert.AreEqual(l3_aList, upstreamActivities[3]);
			Assert.AreEqual(l2_a1, upstreamActivities[4]);
			Assert.AreEqual(l2_aList, upstreamActivities[5]);
			Assert.AreEqual(l1_a1, upstreamActivities[6]);
			Assert.AreEqual(l1_aList, upstreamActivities[7]);
		}
		[Test]
		public void GetUpstreamActivities_BigTreeFromWikiPageWithActivity46_ShoudBeOk()
		{
			List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var actionList in actionLists)
				{
					uow.ActivityRepository.Add(actionList);
				}
				uow.SaveChanges();

				var actionWithId46 = uow.ActionRepository.GetByKey(46);
				var downstreamActivities = _activity.GetUpstreamActivities(actionWithId46);

				Assert.AreEqual(4, downstreamActivities.Count);
				Assert.AreEqual(true, downstreamActivities[0] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[0].GetType().Name));
				Assert.AreEqual(44, downstreamActivities[0].Id, "Expected Action with Name '{0}' but got '{1}".format(44, downstreamActivities[0].Id));
				Assert.AreEqual(true, downstreamActivities[1] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[1].GetType().Name));
				Assert.AreEqual(43, downstreamActivities[1].Id, "Expected ActionListDO with Name '{0}' but got '{1}".format(43, downstreamActivities[1].Id));
				Assert.AreEqual(true, downstreamActivities[2] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[2].GetType().Name));
				Assert.AreEqual(23, downstreamActivities[2].Id, "Expected Action with Name '{0}' but got '{1}".format(23, downstreamActivities[2].Id));
				Assert.AreEqual(true, downstreamActivities[3] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[3].GetType().Name));
				Assert.AreEqual(1, downstreamActivities[3].Id, "Expected ActionListDO with Name '{0}' but got '{1}".format(1, downstreamActivities[3].Id));
			}
		}
		[Test, Ignore("https://maginot.atlassian.net/browse/DO-1008")]
		public void GetDownstreamActivities_1Levels_ShoudBeOk()
		{
			var actionTempate = new ActivityTemplateDO()
			{
				Id = 1,
				Version = "1"
			};
			// Level 1
			ActionDO l1_a1 = new ActionDO() { Id = 1, ActionTemplate = actionTempate, Ordering = 1 };
			ActionDO l1_a2 = new ActionDO() { Id = 2, ActionTemplate = actionTempate, Ordering = 2 };
			ActionListDO l1_aList = new ActionListDO() { Ordering = 1, ActionListType = ActionListType.Immediate };
			l1_aList.Activities.Add(l1_a1);
			l1_a1.ParentActivity = l1_aList;
			l1_aList.Activities.Add(l1_a2);
			l1_a2.ParentActivity = l1_aList;

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				uow.ActionRepository.Add(l1_a1);
				uow.ActionRepository.Add(l1_a2);
				uow.ActionListRepository.Add(l1_aList);

				uow.SaveChanges();
			}
			var downstreamActivities = _activity.GetDownstreamActivities(l1_a1);

			Assert.AreEqual(1, downstreamActivities.Count);
			Assert.AreEqual(l1_a2, downstreamActivities[0]);
		}
		[Test(Description = "Big tree from https://maginot.atlassian.net/wiki/display/SH/Getting+Upstream+and+Downstream+Activities+Lists"),
		Ignore("https://maginot.atlassian.net/browse/DO-1008")]
		public void GetDownstreamActivities_BigTreeFromWikiPageWithActivity46_ShoudBeOk()
		{
			List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var actionList in actionLists)
				{
					uow.ActivityRepository.Add(actionList);
				}
				uow.SaveChanges();
				var actionWithId46 = uow.ActionRepository.GetAll().Where(x => x.Name == "a_46").FirstOrDefault();
				var downstreamActivities = _activity.GetDownstreamActivities(actionWithId46);
				Func<ActivityDO, string> getName = (activityDo) =>
				{
					if (activityDo is ActionDO)
						return (activityDo as ActionDO).Name;
					else if (activityDo is ActionListDO)
						return (activityDo as ActionListDO).Name;
					return string.Empty;
				};
				Assert.AreEqual(15, downstreamActivities.Count);
				Assert.AreEqual(true, downstreamActivities[0] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[0].GetType().Name));
				Assert.AreEqual("a_48", getName(downstreamActivities[0]), "Expected Action with Name '{0}' but got '{1}".format("a_48", getName(downstreamActivities[0])));
				Assert.AreEqual(true, downstreamActivities[1] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[1].GetType().Name));
				Assert.AreEqual("al_52", getName(downstreamActivities[1]), "Expected ActionListDO with Name '{0}' but got '{1}".format("al_52", getName(downstreamActivities[1])));
				Assert.AreEqual(true, downstreamActivities[2] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[2].GetType().Name));
				Assert.AreEqual("a_53", (downstreamActivities[2] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_53", getName(downstreamActivities[2])));
				Assert.AreEqual(true, downstreamActivities[3] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[3].GetType().Name));
				Assert.AreEqual("al_54", (downstreamActivities[3] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_54", getName(downstreamActivities[3])));
				Assert.AreEqual(true, downstreamActivities[4] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[4].GetType().Name));
				Assert.AreEqual("a_56", (downstreamActivities[4] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_56", getName(downstreamActivities[4])));
				Assert.AreEqual(true, downstreamActivities[5] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[5].GetType().Name));
				Assert.AreEqual("a_57", (downstreamActivities[5] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_57", getName(downstreamActivities[5])));
				Assert.AreEqual(true, downstreamActivities[6] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[6].GetType().Name));
				Assert.AreEqual("a_58", (downstreamActivities[6] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_58", getName(downstreamActivities[6])));
				Assert.AreEqual(true, downstreamActivities[7] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[7].GetType().Name));
				Assert.AreEqual("a_55", (downstreamActivities[7] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_55", getName(downstreamActivities[7])));
				Assert.AreEqual(true, downstreamActivities[8] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[8].GetType().Name));
				Assert.AreEqual("al_59", (downstreamActivities[8] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_59", getName(downstreamActivities[8])));
				Assert.AreEqual(true, downstreamActivities[9] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[9].GetType().Name));
				Assert.AreEqual("a_60", (downstreamActivities[9] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_60", getName(downstreamActivities[9])));
				Assert.AreEqual(true, downstreamActivities[10] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[10].GetType().Name));
				Assert.AreEqual("al_61", (downstreamActivities[10] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_61", getName(downstreamActivities[10])));
				Assert.AreEqual(true, downstreamActivities[11] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[11].GetType().Name));
				Assert.AreEqual("a_63", (downstreamActivities[11] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_63", getName(downstreamActivities[11])));
				Assert.AreEqual(true, downstreamActivities[12] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[12].GetType().Name));
				Assert.AreEqual("a_64", (downstreamActivities[12] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_64", getName(downstreamActivities[12])));
				Assert.AreEqual(true, downstreamActivities[13] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[13].GetType().Name));
				Assert.AreEqual("a_65", (downstreamActivities[13] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_65", getName(downstreamActivities[13])));
				Assert.AreEqual(true, downstreamActivities[14] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[14].GetType().Name));
				Assert.AreEqual("a_62", (downstreamActivities[14] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_62", getName(downstreamActivities[14])));
			}
		}
		[Test(Description = "Big tree from https://maginot.atlassian.net/wiki/display/SH/Getting+Upstream+and+Downstream+Activities+Lists"),
		Ignore("https://maginot.atlassian.net/browse/DO-1008")]
		public void GetDownstreamActivities_BigTreeFromWikiPageWithActivityList59_ShoudBeOk()
		{
			List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var actionList in actionLists)
				{
					uow.ActivityRepository.Add(actionList);
				}
				uow.SaveChanges();
				var activityList59 = uow.ActionListRepository.GetAll().Where(x => x.Name == "al_59").FirstOrDefault();
				var downstreamActivities = _activity.GetDownstreamActivities(activityList59);
				Func<ActivityDO, string> getName = (activityDo) =>
				{
					if (activityDo is ActionDO)
						return (activityDo as ActionDO).Name;
					else if (activityDo is ActionListDO)
						return (activityDo as ActionListDO).Name;
					return string.Empty;
				};
				Assert.AreEqual(6, downstreamActivities.Count);
				Assert.AreEqual(true, downstreamActivities[0] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[0].GetType().Name));
				Assert.AreEqual("a_60", (downstreamActivities[0] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_60", getName(downstreamActivities[0])));
				Assert.AreEqual(true, downstreamActivities[1] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[1].GetType().Name));
				Assert.AreEqual("al_61", (downstreamActivities[1] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_61", getName(downstreamActivities[1])));
				Assert.AreEqual(true, downstreamActivities[2] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[2].GetType().Name));
				Assert.AreEqual("a_63", (downstreamActivities[2] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_63", getName(downstreamActivities[2])));
				Assert.AreEqual(true, downstreamActivities[3] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[3].GetType().Name));
				Assert.AreEqual("a_64", (downstreamActivities[3] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_64", getName(downstreamActivities[3])));
				Assert.AreEqual(true, downstreamActivities[4] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[4].GetType().Name));
				Assert.AreEqual("a_65", (downstreamActivities[4] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_65", getName(downstreamActivities[4])));
				Assert.AreEqual(true, downstreamActivities[5] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[5].GetType().Name));
				Assert.AreEqual("a_62", (downstreamActivities[5] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_62", getName(downstreamActivities[5])));
			}
		}
		[Test(Description = "Big tree from https://maginot.atlassian.net/wiki/display/SH/Getting+Upstream+and+Downstream+Activities+Lists"),
		Ignore("https://maginot.atlassian.net/browse/DO-1008")]
		public void GetDownstreamActivities_BigTreeFromWikiPageWithActivityList1_ShoudBeOk()
		{
			List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();

			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var actionList in actionLists)
				{
					uow.ActivityRepository.Add(actionList);
				}
				uow.SaveChanges();
				var activityList1 = uow.ActionListRepository.GetAll().Where(x => x.Name == "al_1").FirstOrDefault();
				var downstreamActivities = _activity.GetDownstreamActivities(activityList1);
				Func<ActivityDO, string> getName = (activityDo) =>
				{
					if (activityDo is ActionDO)
						return (activityDo as ActionDO).Name;
					else if (activityDo is ActionListDO)
						return (activityDo as ActionListDO).Name;
					return string.Empty;
				};
				Assert.AreEqual(19, downstreamActivities.Count);
				Assert.AreEqual(true, downstreamActivities[0] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[0].GetType().Name));
				Assert.AreEqual("a_23", getName(downstreamActivities[0]), "Expected Action with Name '{0}' but got '{1}".format("a_23", getName(downstreamActivities[0])));
				Assert.AreEqual(true, downstreamActivities[1] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[1].GetType().Name));
				Assert.AreEqual("al_43", getName(downstreamActivities[1]), "Expected Action with Name '{0}' but got '{1}".format("al_43", getName(downstreamActivities[1])));
				Assert.AreEqual(true, downstreamActivities[2] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[2].GetType().Name));
				Assert.AreEqual("a_44", getName(downstreamActivities[2]), "Expected Action with Name '{0}' but got '{1}".format("a_44", getName(downstreamActivities[2])));
				Assert.AreEqual(true, downstreamActivities[3] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[3].GetType().Name));
				Assert.AreEqual("a_46", getName(downstreamActivities[3]), "Expected Action with Name '{0}' but got '{1}".format("a_46", getName(downstreamActivities[3])));

				Assert.AreEqual(true, downstreamActivities[4] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[4].GetType().Name));
				Assert.AreEqual("a_48", getName(downstreamActivities[4]), "Expected Action with Name '{0}' but got '{1}".format("a_48", getName(downstreamActivities[4])));
				Assert.AreEqual(true, downstreamActivities[5] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[5].GetType().Name));
				Assert.AreEqual("al_52", getName(downstreamActivities[5]), "Expected ActionListDO with Name '{0}' but got '{1}".format("al_52", getName(downstreamActivities[5])));
				Assert.AreEqual(true, downstreamActivities[6] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[6].GetType().Name));
				Assert.AreEqual("a_53", (downstreamActivities[6] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_53", getName(downstreamActivities[6])));
				Assert.AreEqual(true, downstreamActivities[7] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[7].GetType().Name));
				Assert.AreEqual("al_54", (downstreamActivities[7] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_54", getName(downstreamActivities[7])));
				Assert.AreEqual(true, downstreamActivities[8] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[8].GetType().Name));
				Assert.AreEqual("a_56", (downstreamActivities[8] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_56", getName(downstreamActivities[8])));
				Assert.AreEqual(true, downstreamActivities[9] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[9].GetType().Name));
				Assert.AreEqual("a_57", (downstreamActivities[9] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_57", getName(downstreamActivities[9])));
				Assert.AreEqual(true, downstreamActivities[10] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[10].GetType().Name));
				Assert.AreEqual("a_58", (downstreamActivities[10] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_58", getName(downstreamActivities[10])));
				Assert.AreEqual(true, downstreamActivities[11] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[11].GetType().Name));
				Assert.AreEqual("a_55", (downstreamActivities[11] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_55", getName(downstreamActivities[11])));
				Assert.AreEqual(true, downstreamActivities[12] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[12].GetType().Name));
				Assert.AreEqual("al_59", (downstreamActivities[12] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_59", getName(downstreamActivities[12])));
				Assert.AreEqual(true, downstreamActivities[13] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[13].GetType().Name));
				Assert.AreEqual("a_60", (downstreamActivities[13] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_60", getName(downstreamActivities[13])));
				Assert.AreEqual(true, downstreamActivities[14] is ActionListDO, "Expected ActionListDO but got '{0}'".format(downstreamActivities[14].GetType().Name));
				Assert.AreEqual("al_61", (downstreamActivities[14] as ActionListDO).Name, "Expected ActionListDO with Name '{0}' but got '{1}".format("al_61", getName(downstreamActivities[14])));
				Assert.AreEqual(true, downstreamActivities[15] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[15].GetType().Name));
				Assert.AreEqual("a_63", (downstreamActivities[15] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_63", getName(downstreamActivities[15])));
				Assert.AreEqual(true, downstreamActivities[16] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[16].GetType().Name));
				Assert.AreEqual("a_64", (downstreamActivities[16] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_64", getName(downstreamActivities[16])));
				Assert.AreEqual(true, downstreamActivities[17] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[17].GetType().Name));
				Assert.AreEqual("a_65", (downstreamActivities[17] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_65", getName(downstreamActivities[17])));
				Assert.AreEqual(true, downstreamActivities[18] is ActionDO, "Expected ActionDO but got '{0}'".format(downstreamActivities[18].GetType().Name));
				Assert.AreEqual("a_62", (downstreamActivities[18] as ActionDO).Name, "Expected Action with Name '{0}' but got '{1}".format("a_62", getName(downstreamActivities[18])));
			}
		}

	}
}
