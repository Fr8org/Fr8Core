using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using DocuSign.Integrations.Client;
using System.Collections.Generic;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
        public static ActivityTemplateDO TestActionTemplate1()
		{
            ActivityTemplateDO actionTemplateDo = new ActivityTemplateDO
			{
				Id = 1,
                Name = "Write to Sql Server",
                DefaultEndPoint = "pluginAzureSqlServer",
                Version="v3"
			};
            return actionTemplateDo;
		}

        public static ActivityTemplateDO TestActionTemplate2()
        {
            ActivityTemplateDO actionTemplateDo = new ActivityTemplateDO
            {
                Id = 1,
                Version = "v4"                
            };
            return actionTemplateDo;
        }
        public static ActivityTemplateDO TestActionTemplateDO1()
        {
            var curActionDO = new ActivityTemplateDO
            {
                Id = 1,
                Name = "Type1",
                DefaultEndPoint = "AzureSqlServer",
                Version = "1"
            };
            return curActionDO;
        }

	    public static ActivityTemplateDO TestActionTemplateV2()
	    {
	        var curActionTemplate = new ActivityTemplateDO
	        {
	            Id = 1,
                Name = "plugin_azure_sql_server",
                DefaultEndPoint = "http://localhost:46281/",
	            Version = "1"
	        };

	        return curActionTemplate;
	    }

        public static ActionTemplateDTO TestActionTemplateDTOV2()
        {
            var curActionTemplate = new ActionTemplateDTO
            {
                Id = 1,
                Name = "plugin_azure_sql_server",
                DefaultEndPoint = "http://localhost:46281/",
                Version = "1"
            };

            return curActionTemplate;
        }

        public static CrateStorageDTO TestConfigurationSettings()
        {
            return new CrateStorageDTO()
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

	    public static CrateStorageDTO TestConfigurationStore()
	    {
	        return new CrateStorageDTO
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

        public static ActivityTemplateDO TestActionTemplateDO2()
        {
            var curActionDO = new ActivityTemplateDO
            {
                Id = 1,
                Name = "Write to SQL",
                DefaultEndPoint = "AzureSqlServer",
                Version = "1"
            };
            return curActionDO;

        }
    }
}
