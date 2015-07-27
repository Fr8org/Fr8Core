using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
	partial class FixtureData
	{
		public ActionDO TestAction1()
		{
			var curActionDO = new ActionDO
			{
				Id = 1,
				Name = "Action 1"
			};
			return curActionDO;
		}

		public ActionDO TestAction2()
		{
			var curActionDO = new ActionDO
			{
				Id = 2,
				Name = "Action 2"
			};
			return curActionDO;
		}

		public ActionListDO TestEmptyActionList()
		{
			var curActionListDO = new ActionListDO
			{
				Id = 4,
				TemplateId = 1,
				Name = "list1"
			};
			return curActionListDO;
		}

		public ActionListDO TestActionList()
		{
			var curActionListDO = new ActionListDO
			{
				Id = 1,
				TemplateId = 1,
				Name = "list1"
			};

			var curActionDO = new ActionDO
			{
				Id = 1,
				Name = "Action 1",
				ActionListID = 1
			};

			var curActionDO2 = new ActionDO
			{
				Id = 2,
				Name = "Action 2",
				ActionListID = 1
			};

			curActionListDO.ActionOrdering.Add( curActionDO );
			curActionListDO.ActionOrdering.Add( curActionDO2 );

			return curActionListDO;
		}
	}
}