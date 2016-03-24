using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HealthMonitor.Utility;
using NUnit.Framework;
using StructureMap;
using terminalGoogle.Interfaces;
using terminalGoogleTests.Integration.terminalIntegrationTests.Helpers;
using terminalGoogleTests.Unit;

namespace terminalGoogleTests.Integration
{
    [Explicit]
    public class Query_DocuSign_Into_Google_Sheet_Tests : BaseHubIntegrationTest
    {
        #region Properties

        public override string TerminalName { get { return "terminalGoogle"; } }

        #endregion

        [Test, Category("Integration.terminalGoogle")]
        public async Task Query_DocuSign_Into_Google_Sheet_End_To_End()
        {
            //Debugger.Launch();

            var activityConfigurator = new ActivityConfigurator(this);
            await RevokeTokens();

            //create a new envelope that will be put into drafts.

            //create a new plan
            var thePlan = await activityConfigurator.CreateNewPlan();

            //configure an query_DocuSign activity
            await activityConfigurator.AddAndConfigure_QueryDocuSign(thePlan, 1);

            //login to google
            //configure a save_to google activity
            var newSpeadsheetName = Guid.NewGuid().ToString();
            await activityConfigurator.AddAndConfigure_SaveToGoogleSheet(thePlan, 2, "Docusign Envelope", "DocuSign Envelope Data", newSpeadsheetName);

            //run the plan
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + thePlan.Plan.Id, null);


            //add asserts here
            var googleSheetApi = ObjectFactory.GetInstance<IGoogleSheet>();

            var googleSheets = googleSheetApi.EnumerateSpreadsheetsUris(HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName));
            var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName).Key;
            
            //find spreadsheet
            var dataRows = googleSheetApi.EnumerateDataRows(spreadSheeturl, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            //cleanup. erase the sheet
            Assert.Equals(12, dataRows.Count());
        }

    }
}
