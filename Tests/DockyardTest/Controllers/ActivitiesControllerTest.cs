using System.Linq;
using Core.Services;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;
using UtilitiesTesting.Fixtures;
using Web.Controllers;
using Web.ViewModels;
using Moq;
using Core.PluginRegistrations;
using System;
using Core.Interfaces;
using System.Collections.Generic;
using System.Web.Http.Results;

namespace DockyardTest.Controllers
{
	[TestFixture]
	public class ActivitiesControllerTest : BaseTest
	{
		public ActivitiesControllerTest()
		{

		}
		public override void SetUp()
		{
			base.SetUp();

			List<ActionListDO> actionLists = FixtureData.TreeFromWikiPage();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				foreach (var actionList in actionLists)
				{
					foreach (var action in actionList.Actions)
						uow.ActionRepository.Add(action);
					uow.ActionListRepository.Add(actionList);
				}
				uow.SaveChanges();
			}
		}

		[Test]
		public void GetUpstreamActivities_IdDoesntExist_ExpectedArgumentNullException()
		{
			var controller = new ActivitiesController();
			var ex = Assert.Throws<ArgumentNullException>(() => controller.GetUpstreamActivities(-1));
		}
		[Test]
		public void GetUpstreamActivities_ExistId_ShouldBeOk()
		{
			int[] expectedIds = new int[]
			{
				44, 43, 23, 1
			};
			var controller = new ActivitiesController();

			var downstreams = controller.GetUpstreamActivities(46) as OkNegotiatedContentResult<IEnumerable<ActionDTOBase>>;
			var ids = downstreams.Content.Select(x => x.Id).ToArray();

			Assert.AreEqual(expectedIds, ids);
		}
		[Test]
		public void GetDownstreamActivities_IdDoesntExist_ExpectedArgumentNullException()
		{
			var controller = new ActivitiesController();
			var ex = Assert.Throws<ArgumentNullException>(() => controller.GetDownstreamActivities(-1));
		}
		[Test]
		public void GetDownstreamActivities_ExistId_ShoudBeOk()
		{
			int[] expectedIds = new int[]
			{
				48, 52, 53, 54, 56, 57, 58, 55, 59, 60, 61, 63, 64, 65, 62
			};
			var controller = new ActivitiesController();
			
			var downstreams = controller.GetDownstreamActivities(46) as OkNegotiatedContentResult<IEnumerable<ActionDTOBase>>;
			var ids = downstreams.Content.Select(x => x.Id).ToArray();

			Assert.AreEqual(expectedIds, ids);
		}
	}
}
