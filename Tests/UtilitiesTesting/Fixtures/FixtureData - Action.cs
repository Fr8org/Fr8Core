using Data.Entities;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
	partial class FixtureData
	{
		public ActionDO TestAction1()
		{
			var curActionDO = new ActionDO
			{
				Id = 1,
				UserLabel = "Action 1"
			};
			return curActionDO;
		}

	    public TemplateDO TestTemplate1()
	    {
	        var curTemplateDO = new TemplateDO(new Template())
	        {
	            Id = 1
	        };

	        return curTemplateDO;
	    }

		public ActionDO TestAction2()
		{
			var curActionDO = new ActionDO
			{
				Id = 2,
				UserLabel = "Action 2"
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
				UserLabel = "Action 1",
				ActionListId = 1,
                Ordering = 1
			};

			var curActionDO2 = new ActionDO
			{
				Id = 2,
				UserLabel = "Action 2",
				ActionListId = 1,
                Ordering = 2
			};

			curActionListDO.ActionOrdering.Add( curActionDO );
			curActionListDO.ActionOrdering.Add( curActionDO2 );

			return curActionListDO;
		}
	}
}