using Data.Entities;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
        public static ActionTemplateDO TestActionTemplate1()
		{
            ActionTemplateDO actionTemplateDo = new ActionTemplateDO
			{
				Id = 1,
                ActionType = "Write to Sql Server",
                ParentPluginRegistration = "pluginAzureSqlServer",
                Version="v3"
			};
            return actionTemplateDo;
		}

        public static ActionTemplateDO TestActionTemplate2()
        {
            ActionTemplateDO actionTemplateDo = new ActionTemplateDO
            {
                Id = 1,
                Version = "v4"                
            };
            return actionTemplateDo;
        }
        public static ActionTemplateDO TestActionTemplateDO1()
        {
            var curActionDO = new ActionTemplateDO
            {
                Id = 1,
                ActionType = "Type1",
                ParentPluginRegistration = "AzureSqlServer",
                Version = "1"
            };
            return curActionDO;
        }
	}
}
