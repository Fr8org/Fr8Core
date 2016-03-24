using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
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
            var googleAuthTokenId = await ExtractGoogleDefaultToken();
            //create a new envelope that will be put into drafts.

            //create a new plan
            var thePlan = await activityConfigurator.CreateNewPlan();

            //configure an query_DocuSign activity
            await activityConfigurator.AddAndConfigure_QueryDocuSign(thePlan, 1);

            //login to google
            //configure a save_to google activity
            var newSpeadsheetName = Guid.NewGuid().ToString();
            await activityConfigurator.AddAndConfigure_SaveToGoogleSheet(thePlan, 2, "Docusign Envelope", "DocuSign Envelope Data", newSpeadsheetName, googleAuthTokenId);

            //run the plan
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + thePlan.Plan.Id, null);


            //add asserts here
            var googleSheetApi = ObjectFactory.GetInstance<IGoogleSheet>();

            var googleSheets = googleSheetApi.EnumerateSpreadsheetsUris(HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName));
            var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName).Key;
            
            //find spreadsheet
            var dataRows = googleSheetApi.EnumerateDataRows(spreadSheeturl, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            //file should contain 11 envelopes saved plus one row for the headers
            Assert.Equals(11, dataRows.Count());
            
            //cleanup. erase the sheet
            await googleSheetApi.DeleteSpreadSheet(newSpeadsheetName, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());
        }

        private async Task<Guid> ExtractGoogleDefaultToken()
        {
            var tokens = await HttpGetAsync<IEnumerable<ManageAuthToken_Terminal>>(
                _baseUrl + "manageauthtoken/"
            );

            Assert.NotNull(tokens);

            var terminal = tokens.FirstOrDefault(x => x.Name == "terminalGoogle");
            Assert.NotNull(terminal);

            var token = terminal.AuthTokens.FirstOrDefault(x => x.IsMain);
            Assert.NotNull(token);

            return token.Id;
        }

    }
}
