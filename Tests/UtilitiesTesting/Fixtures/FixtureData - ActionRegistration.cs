using Data.Entities;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
        public static ActionRegistrationDO TestActionRegistration1()
		{
            ActionRegistrationDO actionRegistrationDO = new ActionRegistrationDO
			{
				Id = 1,
                ActionType = "Write to Sql Server",
                ParentPluginRegistration = "pluginAzureSqlServer",
                Version="v3"
			};
            return actionRegistrationDO;
		}

        public static ActionRegistrationDO TestActionRegistration2()
        {
            ActionRegistrationDO actionRegistrationDO = new ActionRegistrationDO
            {
                Id = 1,
                Version = "v4"                
            };
            return actionRegistrationDO;
        }
        public static ActionRegistrationDO TestActionRegistrationDO1()
        {
            var curActionDO = new ActionRegistrationDO
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
