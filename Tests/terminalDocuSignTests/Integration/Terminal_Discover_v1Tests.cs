using System.Linq;
using System.Threading.Tasks;
using Fr8.Testing.Integration;
using Fr8.Infrastructure.Data.Manifests;
using NUnit.Framework;

namespace terminalDocuSignTests.Integration
{
    [Explicit]
    public class Terminal_Discover_v1Tests : BaseTerminalIntegrationTest
    {
        private const int ActivityCount = 13;
        private const string Extract_Data_From_Envelopes_Name = "Extract_Data_From_Envelopes";
        private const string Use_DocuSign_Template_With_New_Document = "Use_DocuSign_Template_With_New_Document";
        private const string Mail_Merge_Into_DocuSign_Name = "Mail_Merge_Into_DocuSign";
        private const string Monitor_DocuSign_Name = "Monitor_DocuSign_Envelope_Activity";
        private const string Prepare_DocuSign_Events_For_Storage_Name = "Prepare_DocuSign_Events_For_Storage";
        private const string Track_DocuSign_Recipients_Name = "Track_DocuSign_Recipients";
        private const string Send_DocuSign_Envelope_Name = "Send_DocuSign_Envelope";
        private const string Query_DocuSign_Name = "Query_DocuSign";
        private const string Get_DocuSign_Template_Name = "Get_DocuSign_Template";
        private const string Get_DocuSign_Envelope_Name = "Get_DocuSign_Envelope";

        public override string TerminalName
        {
            get { return "terminalDocuSign"; }
        }

        [Test]
        public async Task Discover_Check_Returned_Activities()
        {
            //Arrange
            var discoverUrl = GetTerminalDiscoverUrl();

            //Act
            var terminalDiscoverResponse = await HttpGetAsync<StandardFr8TerminalCM>(discoverUrl);

            //Assert
            Assert.NotNull(terminalDiscoverResponse);
            Assert.AreEqual(ActivityCount, terminalDiscoverResponse.Activities.Count);
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Extract_Data_From_Envelopes_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Mail_Merge_Into_DocuSign_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Monitor_DocuSign_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Prepare_DocuSign_Events_For_Storage_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Track_DocuSign_Recipients_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Send_DocuSign_Envelope_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Query_DocuSign_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Use_DocuSign_Template_With_New_Document));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Get_DocuSign_Envelope_Name));
            Assert.AreEqual(true, terminalDiscoverResponse.Activities.Any(a => a.Name == Get_DocuSign_Template_Name));
        }
    }
}
