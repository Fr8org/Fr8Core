using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using System.Collections.Generic;

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

        public static ConfigurationSettingsDTO TestConfigurationSettings()
        {
            return new ConfigurationSettingsDTO()
            {
                Fields = new List<FieldDefinitionDTO>(){
                    new FieldDefinitionDTO()
                    {
                        Type= "textField",
                        FieldLabel = "SQL Connection String",
                        Value = "",
                        Name = "connection_string",
                        Required = true,
                        Selected = false
                    }
                }
            };

        }
    }
}
