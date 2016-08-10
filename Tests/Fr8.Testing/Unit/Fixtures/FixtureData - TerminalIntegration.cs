﻿using System;
using Data.Entities;
﻿using Data.States;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.TerminalBase.Models;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static string TestTerminal_DocuSign_EndPoint = "localhost:60001";
        public static string TestTerminal_Core_EndPoint = "localhost:60002";
        public static string TestTerminal_AzureSqlServer_EndPoint = "localhost:60003";
        public static string TestTerminal_ExtractData_EndPoint = "localhost:60004";
        public static string TestTerminal_FileServer_EndPoint = "localhost:60005";

        public static string TestTerminal_Core_EndPoint2 = "localhost:50705";

        public static AuthorizationToken AuthToken_TerminalIntegration()
        {
            return new AuthorizationToken()
            {
                Token = @"{""Email"":""freight.testing@gmail.com"",""ApiPassword"":""SnByDvZJ/fp9Oesd/a9Z84VucjU=""}"
            };
        }

        public static PlanDO Plan_TerminalIntegration()
        {
            return new PlanDO()
            {
                Id = GetTestGuidById(1000),
                Name = "Test Plan Name",
                Description = "Test Plan Description",
                PlanState = PlanState.Executing,
            };
        }

        public static SubplanDO SubPlan_TerminalIntegration()
        {
            return new SubplanDO()
            {
                Id = GetTestGuidById(1001),
                Name = "Test Subplan"
            };
        }

        // DO-1214
        public static ActionListDO TestActivityList_ImmediateActivities()
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
                Label = "DocuSign",
                TerminalStatus = TerminalStatus.Active,
                Endpoint = TestTerminal_DocuSign_EndPoint,
                Version = "1",
                AuthenticationType = AuthenticationType.Internal,
                Secret = Guid.NewGuid().ToString()
            };
        }

        public static TerminalDTO TestTerminal_Core_DTO()
        {
            return new TerminalDTO
            {
                Name = "terminalDockyardCore",
                Label = "DockyardCore",
                Endpoint = TestTerminal_Core_EndPoint2,
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                AuthenticationType = AuthenticationType.None
            };
        }

        public static TerminalDO TestTerminal_Core()
        {
            return new TerminalDO
            {
                Name = "terminalDockyardCore",
                Label = "DockyardCore",
                Endpoint = TestTerminal_Core_EndPoint,
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                AuthenticationType = AuthenticationType.None,
                Secret = Guid.NewGuid().ToString()
            };
        }

        public static TerminalDO TestTerminal_AzureSqlServer()
        {
            return new TerminalDO
            {
                Name = "terminalAzureSqlServer",
                Label = "AzureSqlServer",
                Endpoint = TestTerminal_AzureSqlServer_EndPoint,
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                AuthenticationType = AuthenticationType.None,
                Secret = Guid.NewGuid().ToString()
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
                Label = "Excel",
                TerminalStatus = TerminalStatus.Active,
                Version = "1",
                AuthenticationType = AuthenticationType.None,
                Secret = Guid.NewGuid().ToString()
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
                NeedsAuthentication = true
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_RecordDocuSignEvents()
        {
            return new ActivityTemplateDO()
            {
                Name = "Record_DocuSign_Events",
                Label = "Record DocuSign Events",
                Version = "1",
                Terminal = TestTerminal_DocuSign(),
                NeedsAuthentication = true,
                MinPaneWidth = 330
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_StoreMTData()
        {
            return new ActivityTemplateDO()
            {
                Id = Guid.NewGuid(),
                Name = "Save_To_Fr8_Warehouse",
                Label = "Save To Fr8 Warehouse",
                Terminal = TestTerminal_Core(),
                Version = "1"
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_TestIncomingData()
        {
            return new ActivityTemplateDO()
            {
                Name = "Test_Incoming_Data",
                Version = "1",
                Terminal = TestTerminal_Core()
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_WriteToSqlServer()
        {
            return new ActivityTemplateDO()
            {
                Name = "Write_To_Sql_Server",
                Version = "1",
                Terminal = TestTerminal_AzureSqlServer()
            };
        }
		  public static ActivityTemplateDO TestActivityTemplateDO_SendDocuSignEnvelope()
		  {
			  return new ActivityTemplateDO()
			  {
				  Name = "Send_DocuSign_Envelope",
				  Version = "1",
				  Terminal = TestTerminal_DocuSign(),
                  NeedsAuthentication = true
			  };
		  }

        public static ActivityTemplateDO TestActivityTemplateDO_ExtractData()
        {
            return new ActivityTemplateDO()
            {
                Name = "ExtractData",
                Version = "1",
                Terminal = TestTerminal_ExtractData()
            };
        }

        public static ActivityDO TestActivity_Blank()
        {
            return new ActivityDO()
            {
                CrateStorage = ""
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_MonitorFr8Events()
        {
            return new ActivityTemplateDO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Fr8_Events",
                Label = "Monitor Fr8 Events",
                Version = "1",
                NeedsAuthentication = false,
                Terminal = TestTerminal_Core(),
                MinPaneWidth = 380
            };
        }
    }
}
