using System.Collections.Generic;
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
                Token = @"{""Email"":""freight.testing@gmail.com"",""ApiPassword"":""SnByDvZJ/fp9Oesd/a9Z84VucjU=""}"
            };
        }

        public static RouteDO Route_PluginIntegration()
        {
            return new RouteDO()
            {
                Id=1000,
                Name = "Test Route Name",
                Description = "Test Route Description",
                RouteState = RouteState.Active,
            };
        }

        public static SubrouteDO Subroute_PluginIntegration()
        {
            return new SubrouteDO()
            {
                Id = 1001,
                Name = "Test Subroute"
            };
        }

        // DO-1214
        public static ActionListDO TestActionList_ImmediateActions()
        {
            return new ActionListDO()
            {
                //ActionListType = ActionListType.Immediate,
                //Name = "ImmediateActions"
            };
        }

        public static PluginDO TestPlugin_DocuSign()
        {
            return new PluginDO
            {
                Name = "pluginDocuSign",
                PluginStatus = PluginStatus.Active,
                Endpoint = TestPlugin_DocuSign_EndPoint,
                Version = "1"
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
                Label = "Monitor DocuSign",
                Version = "1",
                Plugin = TestPlugin_DocuSign(),
                AuthenticationType = AuthenticationType.Internal
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_FilterUsingRunTimeData()
        {
            return new ActivityTemplateDO()
            {
                Name = "FilterUsingRunTimeData",
                Version = "1",
                Plugin = TestPlugin_Core(),
                AuthenticationType = AuthenticationType.None
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_WriteToSqlServer()
        {
            return new ActivityTemplateDO()
            {
                Name = "Write_To_Sql_Server",
                Version = "1",
                Plugin = TestPlugin_AzureSqlServer(),
                AuthenticationType = AuthenticationType.None
            };
        }
		  public static ActivityTemplateDO TestActivityTemplateDO_SendDocuSignEnvelope()
		  {
			  return new ActivityTemplateDO()
			  {
				  Name = "Send_DocuSign_Envelope",
				  Version = "1",
				  Plugin = TestPlugin_DocuSign(),
                  AuthenticationType = AuthenticationType.Internal
			  };
		  }

        public static ActivityTemplateDO TestActivityTemplateDO_ExtractData()
        {
            return new ActivityTemplateDO()
            {
                Name = "ExtractData",
                Version = "1",
                Plugin = TestPlugin_ExtractData(),
                AuthenticationType = AuthenticationType.None
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
