using System;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using HealthMonitor.Utility;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminaBaselTests.Tools.Activities;
using terminaBaselTests.Tools.Plans;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services;

namespace terminalIntegrationTests.EndToEnd
{
    [Explicit]
    public class GoogleIntoGoogleTests : BaseHubIntegrationTest
    {
        private readonly IntegrationTestTools _plansHelper;

        private readonly IntegrationTestTools_terminalGoogle _googleActivityConfigurator;

        private readonly IntegrationTestTools_terminalFr8 _fr8ActivityConfigurator;
        public override string TerminalName => "terminalGoogle";

        public GoogleIntoGoogleTests()
        {
            _googleActivityConfigurator = new IntegrationTestTools_terminalGoogle(this);
            _plansHelper = new IntegrationTestTools(this);
            _fr8ActivityConfigurator = new IntegrationTestTools_terminalFr8(this);
        }

        [Test, Category("Integration.terminalGoogle")]
        public async Task GoogleIntoGoogleEndToEnd()
        {
            var googleAuthTokenId = await new terminaBaselTests.Tools.Terminals.IntegrationTestTools_terminalGoogle(this).ExtractGoogleDefaultToken();
            var defaultGoogleAuthToken = GetGoogleAuthToken(googleAuthTokenId);

            //create a new plan
            var thePlan = await _plansHelper.CreateNewPlan();
            //Configure Get_Google_Sheet_Data activity
            await _googleActivityConfigurator.AddAndConfigureGetFromGoogleSheet(thePlan, 1, "EmailList");
            //Configure Build_Message activity
            await _fr8ActivityConfigurator.AddAndConfigureBuildMessage(thePlan, 2, "message", "Email - [email], subject - [subject], body - [body]");
            //Configure Save_To_Google activity
            var googleSheetApi = new GoogleSheet(new GoogleIntegration());
            var newSpreadsheetName = Guid.NewGuid().ToString();
            var spreasheetUri = await googleSheetApi.CreateSpreadsheet(newSpreadsheetName, defaultGoogleAuthToken);
            try
            {
                await _googleActivityConfigurator.AddAndConfigureSaveToGoogleSheet(thePlan, 3, "Field Description", "Build Message", newSpreadsheetName);
                //run the plan
                await _plansHelper.RunPlan(thePlan.Plan.Id);

                var googleSheets = await googleSheetApi.GetSpreadsheets(defaultGoogleAuthToken);

                Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == newSpreadsheetName), "New created spreadsheet was not found into existing google files.");
                var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == newSpreadsheetName).Key;

                //find spreadsheet
                var worksheets = await googleSheetApi.GetWorksheets(spreadSheeturl, defaultGoogleAuthToken);
                Assert.IsNotNull(worksheets.FirstOrDefault(x => x.Value == "Sheet1"), "Worksheet was not found into newly created google excel file.");
                var worksheetUri = worksheets.FirstOrDefault(x => x.Value == "Sheet1").Key;
                var dataRows = (await googleSheetApi.GetData(spreadSheeturl, worksheetUri, defaultGoogleAuthToken)).ToArray();

                //file should contain 11 envelopes saved
                Assert.AreEqual(1, dataRows.Length, "Only one data row is expected to be in crated spreadsheet");
                var storedData = dataRows[0].Row[0].Cell;
                Assert.AreEqual("message", storedData.Key, "Saved message header doesn't match the expected data");
                Assert.AreEqual("Email - fake@fake.com, subject - Fake Subject, body - Fake Body", storedData.Value, "Saved message body doesn't match the expected data");
            }
            finally
            {
                //cleanup. erase the sheet
                await googleSheetApi.DeleteSpreadSheet(spreasheetUri, defaultGoogleAuthToken);
            }
        }
        private GoogleAuthDTO GetGoogleAuthToken(Guid authorizationTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.FindTokenById(authorizationTokenId);

                Assert.IsNotNull(validToken, "Reading default google token from AuthorizationTokenRepository failed. Please provide default account for authenticating terminalGoogle.");

                return JsonConvert.DeserializeObject<GoogleAuthDTO>((validToken).Token);
            }
        }


    }
}
