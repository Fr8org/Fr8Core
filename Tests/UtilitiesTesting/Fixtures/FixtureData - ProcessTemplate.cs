using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static ProcessTemplateDO GetProcessTemplate1()
		{
			var processTemplate = new ProcessTemplateDO();
			processTemplate.Description = "descr 1";
			processTemplate.Name = "template1";
			processTemplate.ProcessTemplateState = ProcessTemplateState.Active;

			return processTemplate;
		}
	}
}