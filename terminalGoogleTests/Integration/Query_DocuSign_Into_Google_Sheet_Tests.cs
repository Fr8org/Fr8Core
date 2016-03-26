using System;
using System.Linq;
using System.Threading.Tasks;
using HealthMonitor.Utility;
using NUnit.Framework;
using terminaBaselTests.Tools.Activities;
using terminaBaselTests.Tools.Plans;
using terminalGoogle.Services;
using terminalGoogleTests.Unit;

namespace terminalGoogleTests.Integration
{
    [Explicit]
    public class Query_DocuSign_Into_Google_Sheet_Tests : BaseHubIntegrationTest
    {
        #region Properties

        private readonly IntegrationTestTools plansHelper;
        private readonly IntegrationTestTools_terminalDocuSign docuSignActivityConfigurator;
        private readonly IntegrationTestTools_terminalGoogle googleActivityConfigurator;
        public override string TerminalName => "terminalGoogle";

        #endregion

        public Query_DocuSign_Into_Google_Sheet_Tests()
        {
            plansHelper = new IntegrationTestTools(this);
            docuSignActivityConfigurator = new IntegrationTestTools_terminalDocuSign(this);
            googleActivityConfigurator = new IntegrationTestTools_terminalGoogle(this);
        }
        
        [Test, Category("Integration.terminalGoogle")]
        public async Task Query_DocuSign_Into_Google_Sheet_End_To_End()
        {
            //create a new plan
            var thePlan = await plansHelper.CreateNewPlan();
            
            //configure an query_DocuSign activity
            await docuSignActivityConfigurator.AddAndConfigure_QueryDocuSign(thePlan, 1);
          
            //configure a save_to google activity
            var newSpeadsheetName = Guid.NewGuid().ToString();
            await googleActivityConfigurator.AddAndConfigure_SaveToGoogleSheet(thePlan, 2, "Docusign Envelope", "DocuSign Envelope Data", newSpeadsheetName);

            var googleSheetApi = new GoogleSheet(new GoogleIntegration());
            var spreadsheetId = await googleSheetApi.CreateSpreadsheet(newSpeadsheetName, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            //run the plan
            await plansHelper.RunPlan(thePlan.Plan.Id);
                
            //add asserts here
            var googleSheets = googleSheetApi.EnumerateSpreadsheetsUris(HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName),"New created spreadsheet was not found into existing google files.");
            var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName).Key;
            
            //find spreadsheet
            var dataRows = googleSheetApi.EnumerateDataRows(spreadSheeturl, "Sheet1", HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            //file should contain 11 envelopes saved
            var numberOfEnvelopes = dataRows.ToList().Count();
            Assert.AreNotEqual(0, numberOfEnvelopes, "Failed to read any envelope data from excel rows. Run method may failed to write data into excel file");
            Assert.AreEqual(11, numberOfEnvelopes, "Number of readed rows/envelopes was not in the correct count");
            
            //cleanup. erase the sheet
            await googleSheetApi.DeleteSpreadSheet(spreadsheetId, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());
        }
    }
}
