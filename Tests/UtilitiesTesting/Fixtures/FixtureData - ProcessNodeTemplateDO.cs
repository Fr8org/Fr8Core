using Data.Entities;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ProcessNodeTemplateDO TestProcessNodeTemplateDO1()
        {
            var ProcessNodeTemplateDO = new ProcessNodeTemplateDO
            {
                Id = 50,
                NodeTransitions = "[{'Flag':'true','Id':'1'},{'Flag':'false','Id':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }
        public static ProcessNodeTemplateDO TestProcessNodeTemplateHealthDemo()
        {
            var ProcessNodeTemplateDO = new ProcessNodeTemplateDO
            {
                Id = 50,
                ParentTemplateId = 23,
                NodeTransitions = "[{'Flag':'true','Id':'2'}]"
            };
            return ProcessNodeTemplateDO;
        }
    }
}