
﻿using System.Collections.Generic;
using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static string TestTerminal_DocuSign_EndPoint = "localhost:60001";
        public static string TestTerminal_Core_EndPoint = "localhost:60002";
        public static string TestTerminal_AzureSqlServer_EndPoint = "localhost:60003";
        public static string TestTerminal_ExtractData_EndPoint = "localhost:60004";
        public static string TestTerminal_FileServer_EndPoint = "localhost:60005";

        public static AuthorizationTokenDO AuthToken_TerminalIntegration()
        {
            return new AuthorizationTokenDO()
            {
                Token = @"{""Email"":""64684b41-bdfd-4121-8f81-c825a6a03582"",""ApiPassword"":""HyCXOBeGl/Ted9zcMqd7YEKoN0Q=""}"
            };
        }

        public static RouteDO Route_TerminalIntegration()
        {
            return new RouteDO()
            {
                Id=1000,
                Name = "Test Route Name",
                Description = "Test Route Description",
                RouteState = RouteState.Active,
            };
        }

        public static SubrouteDO Subroute_TerminalIntegration()
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

        public static TerminalDO TestTerminal_DocuSign()
        {
            return new TerminalDO
            {
                Name = "terminalDocuSign",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = TestTerminal_DocuSign_EndPoint,
                Version = "1"
            };
        }

        public static TerminalDO TestTerminal_Core()
        {
            return new TerminalDO
            {
                Name = "terminalDockyardCore",
                Endpoint = TestTerminal_Core_EndPoint,
                TerminalStatus = TerminalStatus.Active,
                Version = "1"
            };
        }

        public static TerminalDO TestTerminal_AzureSqlServer()
        {
            return new TerminalDO
            {
                Name = "terminalAzureSqlServer",
                Endpoint = TestTerminal_AzureSqlServer_EndPoint,
                TerminalStatus = TerminalStatus.Active,
                Version = "1"
            };
        }

        public static TerminalDO TestTerminal_ExtractData()
        {
            var TerminalDO = TestTerminal_Excel();
            TerminalDO.Endpoint = TestTerminal_ExtractData_EndPoint;

            return TerminalDO;
        }

        public static TerminalDO TestTerminal_Excel()
        {
            return new TerminalDO
            {
                Name = "terminalExcel",
                TerminalStatus = TerminalStatus.Active,
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
                Terminal = TestTerminal_DocuSign(),
                AuthenticationType = AuthenticationType.Internal
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_FilterUsingRunTimeData()
        {
            return new ActivityTemplateDO()
            {
                Name = "FilterUsingRunTimeData",
                Version = "1",
                Terminal = TestTerminal_Core(),
                AuthenticationType = AuthenticationType.None
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_WriteToSqlServer()
        {
            return new ActivityTemplateDO()
            {
                Name = "Write_To_Sql_Server",
                Version = "1",
                Terminal = TestTerminal_AzureSqlServer(),
                AuthenticationType = AuthenticationType.None
            };
        }
		  public static ActivityTemplateDO TestActivityTemplateDO_SendDocuSignEnvelope()
		  {
			  return new ActivityTemplateDO()
			  {
				  Name = "Send_DocuSign_Envelope",
				  Version = "1",
				  Terminal = TestTerminal_DocuSign(),
                  AuthenticationType = AuthenticationType.Internal
			  };
		  }

        public static ActivityTemplateDO TestActivityTemplateDO_ExtractData()
        {
            return new ActivityTemplateDO()
            {
                Name = "ExtractData",
                Version = "1",
                Terminal = TestTerminal_ExtractData(),
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