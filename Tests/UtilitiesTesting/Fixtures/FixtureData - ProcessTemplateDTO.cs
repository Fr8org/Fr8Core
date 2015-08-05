using Web.ViewModels;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ProcessTemplateDTO TestProcessTemplateDTO()
        {
            return new ProcessTemplateDTO()
            {
                Name = "processtemplate1",
                Description = "Description for test process template",
                ProcessTemplateState = 1
            };
        }

           
    }
}