using Data.Interfaces.DataTransferObjects;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ProcessTemplateOnlyDTO CreateTestProcessTemplateDTO()
        {
            return new ProcessTemplateOnlyDTO()
            {
                Name = "processtemplate1",
                Description = "Description for test process template",
                ProcessTemplateState = 1
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
        }

           
    }
}