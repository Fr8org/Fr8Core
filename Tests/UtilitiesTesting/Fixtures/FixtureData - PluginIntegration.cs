using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static string TestPlugin_DocuSign_EndPoint = "localhost:60001";
        public static string TestPlugin_Core_EndPoint = "localhost:60002";
        public static string TestPlugin_AzureSqlServer_EndPoint = "localhost:60003";
        public static string TestPlugin_ExtractData_EndPoint = "localhost:60004";
        public static string TestPlugin_FileServer_EndPoint = "localhost:60005";

        public static AuthorizationTokenDO AuthToken_PluginIntegration()
        {
            return new AuthorizationTokenDO()
            {
                Token = @"{""Email"":""64684b41-bdfd-4121-8f81-c825a6a03582"",""ApiPassword"":""H1e0D79tpJ3a/7klfhPkPxNMcOo=""}"
            };
        }

        public static ProcessTemplateDO ProcessTemplate_PluginIntegration()
        {
            return new ProcessTemplateDO()
            {
                Name = "Test ProcessTemplate Name",
                Description = "Test ProcessTemplate Description",
                ProcessTemplateState = ProcessTemplateState.Active
            };
        }

        public static ProcessNodeTemplateDO ProcessNodeTemplate_PluginIntegration()
        {
            return new ProcessNodeTemplateDO()
            {
                Name = "Test ProcessNodeTemplate"
            };
        }

        public static ActionListDO TestActionList_ImmediateActions()
        {
            return new ActionListDO()
            {
                ActionListType = ActionListType.Immediate,
                Name = "ImmediateActions"
            };
        }

        public static PluginDO TestPlugin_DocuSign()
        {
            return new PluginDO
            {
                Name = "pluginDocuSign",
                PluginStatus = PluginStatus.Active,
                Endpoint = TestPlugin_DocuSign_EndPoint,
                Version = "1",
                RequiresAuthentication = true
            };
        }

        public static PluginDO TestPlugin_Core()
        {
            return new PluginDO
            {
                Name = "pluginDockyardCore",
                Endpoint = TestPlugin_Core_EndPoint,
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static PluginDO TestPlugin_AzureSqlServer()
        {
            return new PluginDO
            {
                Name = "pluginAzureSqlServer",
                Endpoint = TestPlugin_AzureSqlServer_EndPoint,
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static PluginDO TestPlugin_ExtractData()
        {
            var pluginDO = TestPlugin_Excel();
            pluginDO.Endpoint = TestPlugin_ExtractData_EndPoint;

            return pluginDO;
        }

        public static PluginDO TestPlugin_Excel()
        {
            return new PluginDO
            {
                Name = "pluginExcel",
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_WaitForDocuSignEvent()
        {
            return new ActivityTemplateDO()
            {
                Name = "Monitor_DocuSign",
                Version = "1",
                Plugin = TestPlugin_DocuSign()
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_FilterUsingRunTimeData()
        {
            return new ActivityTemplateDO()
            {
                Name = "FilterUsingRunTimeData",
                Version = "1",
                Plugin = TestPlugin_Core()
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_WriteToSqlServer()
        {
            return new ActivityTemplateDO()
            {
                Name = "Write_To_Sql_Server",
                Version = "1",
                Plugin = TestPlugin_AzureSqlServer()
            };
        }
		  public static ActivityTemplateDO TestActivityTemplateDO_SendDocuSignEnvelope()
		  {
			  return new ActivityTemplateDO()
			  {
				  Name = "Send_DocuSign_Envelope",
				  Version = "1",
				  Plugin = TestPlugin_DocuSign()
			  };
		  }

        public static ActivityTemplateDO TestActivityTemplateDO_ExtractData()
        {
            return new ActivityTemplateDO()
            {
                Name = "ExtractData",
                Version = "1",
                Plugin = TestPlugin_ExtractData()
            };
        }

        public static ActionDO TestAction_Blank()
        {
            return new ActionDO()
            {
                Name = "New Action #1",
                CrateStorage = ""
            };
        }
    }
}
