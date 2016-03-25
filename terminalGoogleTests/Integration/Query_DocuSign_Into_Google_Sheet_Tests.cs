using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using HealthMonitor.Utility;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
using terminalGoogle;
using terminalGoogle.Interfaces;
using terminalGoogle.Services;
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
            Hub.StructureMap.StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.LIVE).ConfigureGoogleDependencies(StructureMapBootStrapper.DependencyType.LIVE);

            var activityConfigurator = new ActivityConfigurator(this);
            //await RevokeTokens();
           // var googleAuthTokenId = await ExtractGoogleDefaultToken();

            //create a new plan
            var thePlan = await activityConfigurator.CreateNewPlan();

            //configure an query_DocuSign activity
            await activityConfigurator.AddAndConfigure_QueryDocuSign(thePlan, 1);

            //login to google
            //configure a save_to google activity
            var newSpeadsheetName = Guid.NewGuid().ToString();
            var googleSheetApi = new GoogleSheet();
            var spreadsheetId = await googleSheetApi.CreateSpreadsheet(newSpeadsheetName, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());
            await activityConfigurator.AddAndConfigure_SaveToGoogleSheet(thePlan, 2, "Docusign Envelope", "DocuSign Envelope Data", newSpeadsheetName, Guid.Empty);

            //run the plan
            await HttpPostAsync<string, string>(_baseUrl + "plans/run?planId=" + thePlan.Plan.Id, null);

            //add asserts here
            var googleSheets = googleSheetApi.EnumerateSpreadsheetsUris(HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());

            Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName));
            var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == newSpeadsheetName).Key;
            
            //find spreadsheet
            var dataRows = googleSheetApi.EnumerateDataRows(spreadSheeturl, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO(), "Sheet1");

            //file should contain 11 envelopes saved
            var numberOfEvelopes = dataRows.ToList().Count();
            Assert.AreEqual(11, numberOfEvelopes);
            
            //cleanup. erase the sheet
            await googleSheetApi.DeleteSpreadSheet(spreadsheetId, HealthMonitor_FixtureData.NewGoogle_AuthToken_As_GoogleAuthDTO());
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
