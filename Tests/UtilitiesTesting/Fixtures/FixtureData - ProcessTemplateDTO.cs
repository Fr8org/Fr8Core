using Web.ViewModels;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public ProcessTemplateDTO TestProcessTemplateDTO()
        {
            return new ProcessTemplateDTO()
            {
                Name = "processtemplate1",
                Description = "Description for test process template",
                ProcessTemplateState = 1
            };
        }

        public ProcessTemplateDTO TestEmptyNameProcessTemplateDTO()
        {
            return new ProcessTemplateDTO()
            {
                Name = "",
                Description = "Description for test process template",
                ProcessTemplateState = 1
            };
        }       
    }
}