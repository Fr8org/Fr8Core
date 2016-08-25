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
        private const int Fr8CoreActivityCount = 21;
        private const string TestIncomingDataName = "Test_Incoming_Data";
        private const string AddPayloadManuallyName = "Add_Payload_Manually";
        private const string SaveToFr8WarehouseName = "Save_To_Fr8_Warehouse";
        private const string Select_Fr8_ObjectName = "Select_Fr8_Object";
        private const string ConnectToSqlName = "Connect_To_Sql";
        //private const string BuildQueryName = "Build_Query";
        private const string ExecuteSqlName = "Execute_Sql";
        private const string LoopName = "Loop";
        private const string SetDelayName = "Set_Delay";
        //private const string ConvertRelatedFieldsIntoTableName = "Convert_Related_Fields_Into_Table"; FR-4669
        //private const string QueryFr8WarehouseName = "Query_Fr8_Warehouse";
        //private const string ShowReportName = "Show_Report_Onscreen";
        private const string StoreFileName = "Store_File";
        private const string MonitorFr8Events = "Monitor_Fr8_Events";
        private const string GetFileFromFr8Store = "Get_File_From_Fr8_Store";
        private const string BuildMessage = "Build_Message";
        private const string SearchFr8Warehouse = "Search_Fr8_Warehouse";
        private const string MakeADecision = "Make_A_Decision";
        private const string ExtractTableField = "Extract_Table_Field";
        private const string AppBuilder = "App_Builder";
        private const string GetDataFromFr8Warehouse = "Get_Data_From_Fr8_Warehouse";
        private const string SendEmailViaSendGrid = "Send_Email";
        private const string SendSMSViaTwilio = "Send_SMS";
        private const string SaveAllPayload = "Save_All_Payload_To_Fr8_Warehouse";

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
            Assert.AreEqual(Fr8CoreActivityCount, terminalDiscoverResponse.Activities.Count, "Fr8CoreActivityCount not equal expected");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == TestIncomingDataName), "TestIncomingDataName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == AddPayloadManuallyName), "AddPayloadManuallyName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SaveToFr8WarehouseName), "SaveToFr8WarehouseName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Select_Fr8_ObjectName), "Select_Fr8_ObjectName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ConnectToSqlName), "ConnectToSqlName wasn`t found");
            // Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == BuildQueryName), "BuildQueryName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ExecuteSqlName), "ExecuteSqlName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == LoopName), "LoopName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SetDelayName), "SetDelayName wasn`t found");
            //Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ConvertRelatedFieldsIntoTableName), "ConvertRelatedFieldsIntoTableName wasn`t found");
            //Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == QueryFr8WarehouseName), "QueryFr8WarehouseName wasn`t found");
            //Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ShowReportName), "ShowReportName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == StoreFileName), "StoreFileName wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == MonitorFr8Events), "MonitorFr8Events wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == GetFileFromFr8Store), "GetFileFromFr8Store wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == BuildMessage), "BuildMessage wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SearchFr8Warehouse), "SearchFr8Warehouse wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == MakeADecision), "MakeADecision wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == ExtractTableField), "ExtractTableField wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == AppBuilder), "AppBuilder wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == GetDataFromFr8Warehouse), "GetDataFromFr8Warehouse wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SendEmailViaSendGrid), "SendEmailViaSendGrid wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SendSMSViaTwilio), "SendSMSViaTwilio wasn`t found");
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == SaveAllPayload), "Save_All_Payload_To_Fr8_Warehouse wasn`t found");
        }
    }
}
