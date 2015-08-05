using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static ProcessTemplateDO GetProcessTemplate1()
		{
		    var processTemplate = new ProcessTemplateDO
		    {
		        Description = "descr 1",
		        Name = "template1",
		        ProcessTemplateState = ProcessTemplateState.Active,
                UserId ="testUser1"
		    };

			return processTemplate;
		}
	}
}