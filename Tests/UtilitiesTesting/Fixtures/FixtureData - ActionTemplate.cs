using Core.Interfaces;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using DocuSign.Integrations.Client;
using StructureMap;
using System.Collections.Generic;

namespace UtilitiesTesting.Fixtures
{
	public partial class FixtureData
	{
        public static ActivityTemplateDO TestActivityTemplate1()
		{
            ActivityTemplateDO activityTemplateDo = new ActivityTemplateDO
			{
				Id = 1,
                Name = "Write to Sql Server",
                Plugin = new PluginDO { Name = "pluginAzureSqlServer", Version = "1", Endpoint = "pluginAzureSqlServer", PluginStatus = PluginStatus.Active },
                Version = "v3"
			};
            return activityTemplateDo;
		}

        public static ActivityTemplateDO TestActivityTemplate2()
        {
            ActivityTemplateDO activityTemplateDo = new ActivityTemplateDO
            {
                Id = 1,
                Version = "v4"                
            };
            return activityTemplateDo;
        }
        public static ActivityTemplateDO TestActivityTemplateDO1()
        {
            var curActivityDO = new ActivityTemplateDO
            {
                Id = 1,
                Name = "Type1",
                Plugin = new PluginDO { Name = "AzureSqlServer", Version = "1", Endpoint = "AzureSqlServer", PluginStatus = PluginStatus.Active },
                Version = "1"
            };
            return curActivityDO;
        }

	    public static ActivityTemplateDO TestActivityTemplateV2()
	    {
	        var curActivityTemplate = new ActivityTemplateDO
	        {
	            Id = 1,
                Name = "plugin_azure_sql_server",
                Plugin = new PluginDO { Name = "http://localhost:46281/", Version = "1", Endpoint = "http://localhost:46281/", PluginStatus = PluginStatus.Active },
	            Version = "1"
	        };

	        return curActivityTemplate;
	    }

        public static ActivityTemplateDTO TestActionTemplateDTOV2()
        {
            var curActionTemplate = new ActivityTemplateDTO
            {
                Id = 1,
                Name = "plugin_azure_sql_server",
                Version = "1"
            };

            return curActionTemplate;
        }

        public static CrateStorageDTO TestConfigurationSettings()
        {
            return FixtureData.CrateStorageDTO();
        }

	    public static CrateStorageDTO TestConfigurationStore()
	    {
            ICrate _crate = ObjectFactory.GetInstance<ICrate>();
	        return new CrateStorageDTO
	        {
	            CratesDTO = new List<CrateDTO>
	            {
                    _crate.Create("SQL Connection String", "{type: 'textField', name: 'connection_string', required: true, fieldLabel: 'SQL Connection String'}"),
                    _crate.Create("Custom SQL Query", "{type: 'textField', name: 'query', required: true, fieldLabel: 'Custom SQL Query'}"),
	                _crate.Create("Log All Transactions", "{type: 'checkboxField', name: 'log_transactions', required: true, fieldLabel: 'Log All Transactions?'}"),
	                _crate.Create("Log Some Transactions", "{type: 'checkboxField', name: 'log_transactions1', required: true, fieldLabel: 'Log Some Transactions?'}"),
	                _crate.Create("Log No Transactions", "{type: 'checkboxField', name: 'log_transactions2', required: true, fieldLabel: 'Log No Transactions?'}"),
                    _crate.Create("Log Failed Transactions", "{type: 'checkboxField', name: 'log_transactions3', required: true, fieldLabel: 'Log Failed Transactions?'}")
	            }
	        };
        }

        public static ActivityTemplateDO TestActivityTemplateDO2()
        {
            var curActivityDO = new ActivityTemplateDO
            {
                Id = 1,
                Name = "Write to SQL",
                Plugin = new PluginDO { Name = "AzureSqlServer", Version = "1", Endpoint = "AzureSqlServer", PluginStatus = PluginStatus.Active },
                Version = "1"
            };
            return curActivityDO;

        }
    }
}
