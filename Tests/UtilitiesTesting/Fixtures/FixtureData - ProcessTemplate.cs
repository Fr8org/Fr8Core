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

                //UserId = "testUser1"
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };

            //add processnode to process
            var healthProcessNodeTemplateDO = FixtureData.TestProcessNodeTemplateHealthDemo();
            healthProcessNodeTemplateDO.ParentTemplateId = healthProcessTemplate.Id;

            //add criteria to processnode
            var healthCriteria = FixtureData.TestCriteriaHealthDemo();
            healthCriteria.ProcessNodeTemplate = healthProcessNodeTemplateDO;

            //add actionlist to processnode
            var healthActionList = FixtureData.TestActionListHealth1();
            healthActionList.ProcessNodeTemplateID = healthProcessNodeTemplateDO.Id;

            //add write action to actionlist
            var healthWriteAction = FixtureData.TestActionWriteSqlServer1();
            healthWriteAction.ActionListId = healthActionList.Id;

                //add field mappings to write action
                var health_FieldMappings = FixtureData.TestFieldMappingSettingsDTO_Health();
                healthWriteAction.FieldMappingSettings = health_FieldMappings.Serialize();

                //add configuration settings to write action
                var configuration_settings = FixtureData.TestConfigurationSettings_healthdemo();
                healthWriteAction.ConfigurationSettings = JsonConvert.SerializeObject(configuration_settings);

            //add a subscription to a specific template on the docusign platform
            var health_DocuSignTemplateSubscription = FixtureData.TestDocuSignTemplateSubscription_medical_form_v2();
            health_DocuSignTemplateSubscription.ProcessTemplate = healthProcessTemplate;




            return healthProcessTemplate;
        }
    }
}