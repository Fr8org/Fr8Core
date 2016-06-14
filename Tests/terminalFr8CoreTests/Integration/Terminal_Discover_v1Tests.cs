using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Testing.Integration;
using NUnit.Framework;

namespace terminalFr8CoreTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int Fr8CoreActivityCount = 25;
        private const string TestIncomingDataName = "TestIncomingData";
        private const string AddPayloadManuallyName = "AddPayloadManually";
        private const string SaveToFr8WarehouseName = "SaveToFr8Warehouse";
        private const string Select_Fr8_ObjectName = "Select_Fr8_Object";
        private const string ConnectToSqlName = "ConnectToSql";
        private const string BuildQueryName = "BuildQuery";
        private const string ExecuteSqlName = "ExecuteSql";
        private const string ManagePlanName = "ManagePlan";        
        private const string LoopName = "Loop";
        private const string SetDelayName = "SetDelay";
        private const string ConvertRelatedFieldsIntoTableName = "ConvertRelatedFieldsIntoTable";
        private const string QueryFr8WarehouseName = "QueryFr8Warehouse";
        private const string ShowReportName = "Show_Report_Onscreen";
        private const string StoreFileName = "StoreFile";
        private const string MonitorFr8Events = "Monitor_Fr8_Events";
        private const string GetFileFromFr8Store = "GetFileFromFr8Store";
        private const string BuildMessage = "Build_Message";
        private const string SearchFr8Warehouse = "SearchFr8Warehouse";
        private const string MakeADecision = "MakeADecision";
        private const string ExtractTableField = "ExtractTableField";
        private const string AppBuilder = "AppBuilder";
        private const string GetDataFromFr8Warehouse = "GetDataFromFr8Warehouse";
        private const string SendEmailViaSendGrid = "Send_Email";
        private const string SendSMSViaTwilio = "Send_SMS";


        public override string TerminalName
        {
            get { return "terminalFr8Core"; }
        }

        [Test]
        public async Task Discover_Check_Returned_Activities()
        {
            var discoverUrl = GetTerminalDiscoverUrl();

            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(Fr8CoreActivityCount, terminalDiscoverResponse.Activities.Count);
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == TestIncomingDataName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == AddPayloadManuallyName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SaveToFr8WarehouseName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Select_Fr8_ObjectName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ConnectToSqlName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == BuildQueryName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ExecuteSqlName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ManagePlanName));            
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == LoopName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SetDelayName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ConvertRelatedFieldsIntoTableName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == QueryFr8WarehouseName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ShowReportName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == StoreFileName));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == MonitorFr8Events));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == GetFileFromFr8Store));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == BuildMessage));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SearchFr8Warehouse));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == MakeADecision));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ExtractTableField));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == AppBuilder));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == GetDataFromFr8Warehouse));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SendEmailViaSendGrid));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SendSMSViaTwilio));
        }
    }
}
