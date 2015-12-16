using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.Manifests;
using HealthMonitor.Utility;
using NUnit.Framework;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseHealthMonitorTest
    {
        private const int ActionCount = 9;
        private const string Extract_Data_From_Envelopes_Name = "Extract_Data_From_Envelopes";
        private const string Mail_Merge_Into_DocuSign_Name = "Mail_Merge_Into_DocuSign";
        private const string Monitor_DocuSign_Name = "Monitor_DocuSign_Envelope_Activity";
        private const string Receive_DocuSign_Envelope_Name = "Receive_DocuSign_Envelope";
        private const string Record_DocuSign_Events_Name = "Record_DocuSign_Events";
        private const string Rich_Document_Notification_Name = "Rich_Document_Notifications";
        private const string Send_DocuSign_Envelope_Name = "Send_DocuSign_Envelope";
        private const string Query_DocuSign_Name = "Query_DocuSign";
        private const string Search_DocuSign_History_Name = "Search_DocuSign_History";

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async void Discover_Check_Returned_Actions()
        {
            //Arrange
            var discoverUrl = GetTerminalDiscoverUrl();

            //Act
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            //Assert
            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(ActionCount, terminalDiscoverResponse.Actions.Count);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Extract_Data_From_Envelopes_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Mail_Merge_Into_DocuSign_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Monitor_DocuSign_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Receive_DocuSign_Envelope_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Record_DocuSign_Events_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Rich_Document_Notification_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Send_DocuSign_Envelope_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Query_DocuSign_Name), true);
            Assert.AreEqual(terminalDiscoverResponse.Actions.Any(a => a.Name == Search_DocuSign_History_Name), true);
        }
    }
}
