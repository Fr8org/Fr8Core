using System.Threading.Tasks;
using NUnit.Framework;
using Fr8.Testing.Integration;
using Fr8.Testing.Integration.Tools.Activities;
using Fr8.Testing.Integration.Tools.Plans;
 
namespace terminalDropboxTests.Integration
{
    /// <summary>
    /// Mark test case class with [Explicit] attiribute.
    /// It prevents test case from running when CI is building the solution,
    /// but allows to trigger that class from HealthMonitor.
    /// </summary>
    [Explicit]
    [Category("terminalDropboxTests.Integration")]
    public class Get_File_List_v1_EndToEnd_Tests : BaseHubIntegrationTest
    {
        public override string TerminalName => "terminalDropbox";

        private readonly IntegrationTestTools _plansHelper;
        private readonly IntegrationTestTools_terminalGoogle _googleActivityConfigurator;
        private readonly IntegrationTestTools_terminalDropbox _terminalDropboxConfigurator;

        public Get_File_List_v1_EndToEnd_Tests()
        {
            _plansHelper = new IntegrationTestTools(this);
            _googleActivityConfigurator = new IntegrationTestTools_terminalGoogle(this);
            _terminalDropboxConfigurator = new IntegrationTestTools_terminalDropbox(this);
        }
        
        [Test]
        public async Task Get_File_List_v1_EndToEnd()
        {
            /*
            // Create plan
            var plan = await _plansHelper.CreateNewPlan();

            // Configure a Get_File_List Dropbox actitivy
            await _terminalDropboxConfigurator.AddAndConfigure_GetFileList(plan, 1);

            // Google auth
            var terminalGoogleTestTools = new Fr8.Testing.Integration.Tools.Terminals.IntegrationTestTools_terminalGoogle(this);
            var googleAuthTokenId = await terminalGoogleTestTools.ExtractGoogleDefaultToken();

            var speadsheetName = Guid.NewGuid().ToString();
            string worksheetName = "Sheet1";
            string spreadsheetId = string.Empty;

            try
            {
                spreadsheetId = await terminalGoogleTestTools.CreateNewSpreadsheet(
                   googleAuthTokenId,
                   speadsheetName,
                   worksheetName,
                   new StandardTableDataCM());

                // Configure a Save_To_Google_Sheet activity
                await
                    _googleActivityConfigurator.AddAndConfigureSaveToGoogleSheet(plan, 2, "Standard File List",
                        "Dropbox file list", speadsheetName);

                // Execute plan
                var container =
                    await HttpPostAsync<string, ContainerDTO>(_baseUrl + "plans/run?planId=" + plan.Plan.Id, null);

                // Validate containter payload
                Assert.AreEqual(container.State, State.Completed,
                    "Container state is not equal to completed on Get_File_List e2e test");

                // Check that data saved into spreadsheet equals data from Dropbox
                var data = await terminalGoogleTestTools.GetSpreadsheetIfExist(googleAuthTokenId, speadsheetName);
                Assert.NotNull(data);
                Assert.AreEqual(4, data.Table.Count);
            }
            finally
            {
                // Cleanup
                // Delete spreadsheet
                if (!string.IsNullOrEmpty(spreadsheetId))
                {
                    await terminalGoogleTestTools.DeleteSpreadSheet(googleAuthTokenId, spreadsheetId);
                }

                // Deactivate plan
                await HttpPostAsync<string, string>(_baseUrl + "plans/deactivate?planId=" + plan.Plan.Id, null);

                // Delete plan
                await HttpDeleteAsync(_baseUrl + "plans?id=" + plan.Plan.Id);
                */
        }

    }
    
}