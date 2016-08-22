using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;
using Fr8.Testing.Integration;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Integration.Tools.Activities;
using Fr8.Testing.Integration.Tools.Plans;
using terminalFr8Core.Actions;
using terminalFr8Core.Activities;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services;
using terminalGoogle.Services.Authorization;

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

            var googleAuthTokenId = await new Fr8.Testing.Integration.Tools.Terminals.IntegrationTestTools_terminalGoogle(this).ExtractGoogleDefaultToken();
            var defaultGoogleAuthToken = GetGoogleAuthToken(googleAuthTokenId);

            //create a new plan
            var googleSheetApi = new GoogleSheet(new GoogleIntegration(ObjectFactory.GetInstance<IRestfulServiceClient>()), new GoogleDrive());
            var sourceSpreadsheetUri = string.Empty;
            var destinationSpreadsheetUri = string.Empty;
            var sourceSpreadsheetName = Guid.NewGuid().ToString();
            var destinationSpreadsheetName = Guid.NewGuid().ToString();
            try
            {
                //Save test data into test spreadsheet
                sourceSpreadsheetUri = await googleSheetApi.CreateSpreadsheet(sourceSpreadsheetName, defaultGoogleAuthToken);
                var sourceWorksheetUri = (await googleSheetApi.GetWorksheets(sourceSpreadsheetUri, defaultGoogleAuthToken)).Select(x => x.Key).First();
                await googleSheetApi.WriteData(sourceSpreadsheetUri, sourceWorksheetUri, GetTestSpreadsheetContent(), defaultGoogleAuthToken);
                var thePlan = await _plansHelper.CreateNewPlan();
                //Configure Get_Google_Sheet_Data activity to read data from this test spreadsheet
                await _googleActivityConfigurator.AddAndConfigureGetFromGoogleSheet(thePlan, 1, sourceSpreadsheetName, true);
                //Configure Build_Message activity to build message based on the data from this spreadsheet
                await _fr8ActivityConfigurator.AddAndConfigureBuildMessage(thePlan, 2, "message", "Email - [email], subject - [subject], body - [body]");
                //Configure Save_To_Google activity to save this message into new test spreadsheet
                destinationSpreadsheetUri = await googleSheetApi.CreateSpreadsheet(destinationSpreadsheetName, defaultGoogleAuthToken);

                await _googleActivityConfigurator.AddAndConfigureSaveToGoogleSheet(thePlan, 3, "Standard Payload Data", Build_Message_v1.RuntimeCrateLabel, destinationSpreadsheetName);
                //run the plan
                await _plansHelper.RunPlan(thePlan.Id);

                var googleSheets = await googleSheetApi.GetSpreadsheets(defaultGoogleAuthToken);

                Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == destinationSpreadsheetName), "New created spreadsheet was not found into existing google files.");
                var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == destinationSpreadsheetName).Key;

                //Checking that new test spreadsheet contains the same message that was generated
                var worksheets = await googleSheetApi.GetWorksheets(spreadSheeturl, defaultGoogleAuthToken);
                Assert.IsNotNull(worksheets.FirstOrDefault(x => x.Value == "Sheet1"), "Worksheet was not found into newly created google excel file.");
                var worksheetUri = worksheets.FirstOrDefault(x => x.Value == "Sheet1").Key;
                var dataRows = (await googleSheetApi.GetData(spreadSheeturl, worksheetUri, defaultGoogleAuthToken)).ToArray();

                Assert.AreEqual(1, dataRows.Length, "Only one data row is expected to be in crated spreadsheet");
                var storedData = dataRows[0].Row[0].Cell;
                Assert.AreEqual("message", storedData.Key, "Saved message header doesn't match the expected data");
                Assert.AreEqual("Email - fake@fake.com, subject - Fake Subject, body - Fake Body", storedData.Value, "Saved message body doesn't match the expected data");
            }
            finally
            {
                if (!string.IsNullOrEmpty(sourceSpreadsheetUri))
                {
                    await googleSheetApi.DeleteSpreadSheet(sourceSpreadsheetUri, defaultGoogleAuthToken);
                }
                if (!string.IsNullOrEmpty(destinationSpreadsheetUri))
                {
                    await googleSheetApi.DeleteSpreadSheet(destinationSpreadsheetUri, defaultGoogleAuthToken);
                }
            }

        }

        private StandardTableDataCM GetTestSpreadsheetContent()
        {
            return new StandardTableDataCM
            {
                FirstRowHeaders = true,
                Table = new List<TableRowDTO>
                {
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>
                        {
                            new TableCellDTO { Cell = new KeyValueDTO("email", "email") },
                            new TableCellDTO { Cell = new KeyValueDTO("subject", "subject") },
                            new TableCellDTO { Cell = new KeyValueDTO("body", "body") }
                        }
                    },
                    new TableRowDTO
                    {
                        Row = new List<TableCellDTO>
                        {
                            new TableCellDTO { Cell = new KeyValueDTO("email", "fake@fake.com") },
                            new TableCellDTO { Cell = new KeyValueDTO("subject", "Fake Subject") },
                            new TableCellDTO { Cell = new KeyValueDTO("body", "Fake Body") }
                        }
                    }

                }
            };
        }
        private GoogleAuthDTO GetGoogleAuthToken(Guid authorizationTokenId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.FindTokenById(authorizationTokenId);

                Assert.IsNotNull(validToken, "Reading default google token from AuthorizationTokenRepository failed. " +
                                             "Please provide default account for authenticating terminalGoogle.");

                return JsonConvert.DeserializeObject<GoogleAuthDTO>((validToken).Token);
            }
        }
    }
}
