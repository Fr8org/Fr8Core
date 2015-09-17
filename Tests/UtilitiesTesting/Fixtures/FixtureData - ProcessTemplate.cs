using Data.Entities;
using Data.States;
using System.Collections.Generic;

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

        public static ProcessTemplateDO TestProcessTemplate2()
        {
            var processTemplate = new ProcessTemplateDO
            {
                Id = 50,
                Description = "descr 2",
                Name = "template2",
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

        public static ProcessTemplateDO TestProcessTemplateWithProcessNodeTemplates()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-982 Process Node Template Test",
                Name = "ProcessTemplateWithProcessNodeTemplates",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            for(int i = 1; i <= 4; ++i)
            {
                var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    Id = i,
                    Name = string.Format("curProcessNodeTemplateDO-{0}", i),
                    ProcessTemplate = curProcessTemplateDO,
                };
                curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);
            }

            return curProcessTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplate3()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-982 Process Node Template Test",
                Name = "ProcessTemplateWithProcessNodeTemplates",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            for (int i = 1; i <= 4; ++i)
            {
                var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    Id = i,
                    Name = string.Format("curProcessNodeTemplateDO-{0}", i),
                    ProcessTemplate = curProcessTemplateDO,
                };
                curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);
                curProcessTemplateDO.ProcessNodeTemplates[0].ActionLists.Add(FixtureData.TestActionList7());

            }

            return curProcessTemplateDO;
        }
    }
}