using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static ProcessTemplateDO CreateProcessTemplate()
		{
			return new ProcessTemplateDO { Description = "descr 1", Name = "template1", ProcessTemplateState = ProcessTemplateState.Active };
		}
	}
}