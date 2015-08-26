using Data.Entities;
using Data.States;
using Newtonsoft.Json;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
		public static ProcessTemplateDO TestProcessTemplate1()
		{          
			var processTemplate = new ProcessTemplateDO
			{
				Description = "descr 1",
				Name = "template1",
				ProcessTemplateState = ProcessTemplateState.Active,

                //UserId = "testUser1"
                //DockyardAccount = FixtureData.TestDockyardAccount1()
			};
			return processTemplate;
		}

        public static ProcessTemplateDO TestProcessTemplateHealthDemo()
        {
            var healthProcessTemplate = new ProcessTemplateDO
            {
                Id = 23,
                Description = "DO-866 HealthDemo Integration Test",
                Name = "HealthDemoIntegrationTest",
                ProcessTemplateState = ProcessTemplateState.Active,

            };


            



            return healthProcessTemplate;
        }
    }
}