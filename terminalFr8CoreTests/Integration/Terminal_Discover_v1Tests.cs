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
        private const int Fr8CoreActionCount = 10;

        private const string FilterUsingRunTimeDataName = "FilterUsingRunTimeData";
        private const string MapFieldsName = "MapFields";
        private const string AddPayloadManuallyName = "AddPayloadManually";
        private const string StoreMTDataName = "StoreMTData";
        private const string Select_Fr8_ObjectName = "Select_Fr8_Object";
        private const string ConnectToSqlName = "ConnectToSql";
        private const string BuildQueryName = "BuildQuery";
        private const string ExecuteSqlName = "ExecuteSql";
        private const string ManageRouteName = "ManageRoute";
        private const string FindObjectsSolutionName = "FindObjects_Solution";

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
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == FilterUsingRunTimeDataName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == MapFieldsName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == AddPayloadManuallyName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == StoreMTDataName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Select_Fr8_ObjectName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == ConnectToSqlName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == BuildQueryName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == ExecuteSqlName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == ManageRouteName), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == FindObjectsSolutionName), true);
        }
    }
}
