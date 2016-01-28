using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseHealthMonitorTest
    {
        private const int Fr8CoreActionCount = 18;

        private const string TestIncomingDataName = "TestIncomingData";
        private const string MapFieldsName = "MapFields";
        private const string AddPayloadManuallyName = "AddPayloadManually";
        private const string SaveToFr8WarehouseName = "SaveToFr8Warehouse";
        private const string Select_Fr8_ObjectName = "Select_Fr8_Object";
        private const string ConnectToSqlName = "ConnectToSql";
        private const string BuildQueryName = "BuildQuery";
        private const string ExecuteSqlName = "ExecuteSql";
        private const string ManageRouteName = "ManageRoute";
        private const string FindObjectsSolutionName = "FindObjects_Solution";
        private const string LoopName = "Loop";
        private const string SetDelayName = "SetDelay";
        private const string ConvertCratesName = "ConvertCrates";
        private const string ConvertRelatedFieldsIntoTableName = "ConvertRelatedFieldsIntoTable";
        private const string QueryMTDatabaseName = "QueryMTDatabase";
        private const string ShowReportName = "Show_Report_Onscreen";
        private const string StoreFileName = "StoreFile";
        private const string MonitorFr8Events = "Monitor_Fr8_Events";

        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public async void Discover_Check_Returned_Actions()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(Fr8CoreActionCount, terminalDiscoverResponse.Actions.Count);
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == TestIncomingDataName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == MapFieldsName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == AddPayloadManuallyName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == SaveToFr8WarehouseName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == Select_Fr8_ObjectName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == ConnectToSqlName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == BuildQueryName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == ExecuteSqlName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == ManageRouteName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == FindObjectsSolutionName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == LoopName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == SetDelayName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == ConvertCratesName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == ConvertRelatedFieldsIntoTableName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == QueryMTDatabaseName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == ShowReportName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == StoreFileName));
            Assert.AreEqual(true, terminalDiscoverResponse.Actions.Any(a => a.Name == MonitorFr8Events));
            
        }
    }
}
