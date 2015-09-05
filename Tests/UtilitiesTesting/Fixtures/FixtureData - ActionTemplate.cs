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

	    public static ActionTemplateDO TestActionTemplateV2()
	    {
	        var curActionTemplate = new ActionTemplateDO
	        {
	            Id = 1,
                ActionType = "plugin_azure_sql_server",
                ParentPluginRegistration = "http://localhost:46281/",
	            Version = "1"
	        };

	        return curActionTemplate;
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

	    public static ConfigurationSettingsDTO TestConfigurationStore()
	    {
	        return new ConfigurationSettingsDTO
	        {
	            Fields = new List<FieldDefinitionDTO>
	            {
	                new FieldDefinitionDTO
	                {
	                    Type = "textField",
	                    Name = "connection_string",
	                    Required = true,
	                    Value = "",
	                    FieldLabel = "SQL Connection String"
	                },

	                new FieldDefinitionDTO
	                {
	                    Type = "textField",
	                    Name = "query",
	                    Required = true,
	                    Value = "",
	                    FieldLabel = "Custom SQL Query"
	                },

	                new FieldDefinitionDTO
	                {
	                    Type = "checkboxField",
	                    Name = "log_transactions",
	                    Selected = false,
	                    FieldLabel = "Log All Transactions?"
	                },

	                new FieldDefinitionDTO
	                {
	                    Type = "checkboxField",
	                    Name = "log_transactions1",
	                    Selected = false,
	                    FieldLabel = "Log Some Transactions?"
	                },

	                new FieldDefinitionDTO
	                {
	                    Type = "checkboxField",
	                    Name = "log_transactions2",
	                    Selected = false,
	                    FieldLabel = "Log No Transactions?"
	                },

	                new FieldDefinitionDTO
	                {
	                    Type = "checkboxField",
	                    Name = "log_transactions3",
	                    Selected = false,
	                    FieldLabel = "Log Failed Transactions?"
	                }
	            }
	        };
	    }
    }
}
