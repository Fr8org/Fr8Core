using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.Interfaces;
using Newtonsoft.Json;
using NUnit.Framework;
using StructureMap;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Services;
using terminalGoogle.Services.Authorization;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Interfaces;

namespace Fr8.Testing.Integration.Tools.Terminals
{
    public class IntegrationTestTools_terminalGoogle
    {
        private readonly BaseHubIntegrationTest _baseHubITest;

        public IntegrationTestTools_terminalGoogle(BaseHubIntegrationTest baseHubIntegrationTest)
        {
            _baseHubITest = baseHubIntegrationTest;
        }

        /// <summary>
        /// For a given google account check for a spreasheet file existence and return the content from that spreadsheet
        /// </summary>
        /// <param name="authorizationTokenId"></param>
        /// <param name="spreadsheetName"></param>
        /// <returns></returns>
        public async Task<StandardTableDataCM> GetSpreadsheetIfExist(Guid authorizationTokenId, string spreadsheetName)
        {
            var defaultGoogleAuthToken = GetGoogleAuthToken(authorizationTokenId);

            var googleSheetApi = new GoogleSheet(new GoogleIntegration(ObjectFactory.GetInstance<IRestfulServiceClient>()), new GoogleDrive());
            var googleSheets = await googleSheetApi.GetSpreadsheets(defaultGoogleAuthToken);

            Assert.IsNotNull(googleSheets.FirstOrDefault(x => x.Value == spreadsheetName), "Selected spreadsheet was not found into existing google files.");
            var spreadSheeturl = googleSheets.FirstOrDefault(x => x.Value == spreadsheetName).Key;

            var worksheets = await googleSheetApi.GetWorksheets(spreadSheeturl, defaultGoogleAuthToken);
            Assert.IsNotNull(worksheets.FirstOrDefault(x => x.Value == "Sheet1"), "Worksheet was not found into google excel file.");
            var worksheetUri = worksheets.FirstOrDefault(x => x.Value == "Sheet1").Key;
            var dataRows = (await googleSheetApi.GetData(spreadSheeturl, worksheetUri, defaultGoogleAuthToken)).ToList();

            var hasHeaderRow = false;
            //Adding header row if possible
            if (dataRows.Count > 0)
            {
                dataRows.Insert(0, new TableRowDTO { Row = dataRows.First().Row.Select(x => new TableCellDTO { Cell = new KeyValueDTO(x.Cell.Key, x.Cell.Key) }).ToList() });
                hasHeaderRow = true;
            }

            return new StandardTableDataCM { Table = dataRows, FirstRowHeaders = hasHeaderRow };
        }

        /// <summary>
        /// Create new spreadsheet file for a given google account, and write data inside the speadsheet
        /// </summary>
        /// <param name="authorizationTokenId"></param>
        /// <param name="spreadsheetName"></param>
        /// <param name="worksheetName"></param>
        /// <param name="tableData"></param>
        /// <returns></returns>
        public async Task<string> CreateNewSpreadsheet(Guid authorizationTokenId, string spreadsheetName, string worksheetName, StandardTableDataCM tableData)
        {
            var googleSheetApi = new GoogleSheet(new GoogleIntegration(ObjectFactory.GetInstance<IRestfulServiceClient>()), new GoogleDrive());
            var defaultGoogleAuthToken = GetGoogleAuthToken(authorizationTokenId);
            var spreadsheetId = await googleSheetApi.CreateSpreadsheet(spreadsheetName, defaultGoogleAuthToken);

            var googleSheets = await googleSheetApi.GetSpreadsheets(defaultGoogleAuthToken);
            var spreadsheetUri = googleSheets.FirstOrDefault(x => x.Value == spreadsheetName).Key;
            //create worksheet for this new created spreadsheet
            var existingWorksheets = await googleSheetApi.GetWorksheets(spreadsheetUri, defaultGoogleAuthToken);
            var existingWorksheetUri = existingWorksheets.Where(x => string.Equals(x.Value.Trim(), worksheetName, StringComparison.InvariantCultureIgnoreCase))
                                          .Select(x => x.Key).FirstOrDefault();
            if (string.IsNullOrEmpty(existingWorksheetUri))
            {
                existingWorksheetUri = await googleSheetApi.CreateWorksheet(spreadsheetUri, defaultGoogleAuthToken, worksheetName);
            }

            await googleSheetApi.WriteData(spreadsheetUri, existingWorksheetUri, tableData, defaultGoogleAuthToken);

            return spreadsheetId;
        }

        /// <summary>
        /// Delete from existence spreadsheet file
        /// </summary>
        /// <param name="authorizationTokenId"></param>
        /// <param name="spreadsheetId"></param>
        /// <returns></returns>
        public async Task DeleteSpreadSheet(Guid authorizationTokenId, string spreadsheetId)
        {
            var googleSheetApi = new GoogleSheet(new GoogleIntegration(ObjectFactory.GetInstance<IRestfulServiceClient>()), new GoogleDrive());
            var defaultGoogleAuthToken = GetGoogleAuthToken(authorizationTokenId);
            await googleSheetApi.DeleteSpreadSheet(spreadsheetId, defaultGoogleAuthToken);
        }

        /// <summary>
        /// Extract default google auth token from repository for future usage.
        /// </summary>
        /// <returns></returns>
        public async Task<Guid> ExtractGoogleDefaultToken()
        {
            var tokens = await _baseHubITest.HttpGetAsync<IEnumerable<AuthenticationTokenTerminalDTO>>(
                _baseHubITest.GetHubApiBaseUrl() + "authentication/tokens"
            );

            Assert.NotNull(tokens, "No authorization tokens were found for the integration testing user.");

            var terminal = tokens.FirstOrDefault(x => x.Name == "terminalGoogle");
            Assert.NotNull(terminal, "No authorization tokens were found for the terminalGoogle.");

            var token = terminal.AuthTokens.FirstOrDefault(x => x.IsMain);
            Assert.NotNull(token, "Authorization token for Google is not found for the integration testing user.Please go to the target instance of fr8 and log in with the integration testing user credentials.Then add a Google action to any plan and be sure to set the 'Use for all Activities' checkbox on the Authorize Accounts dialog while authenticating") ;

            return token.Id;
        }

        /// <summary>
        /// Based on authorizationTokenId returns GoogleAuthToken from the tokenRepository
        /// </summary>
        /// <param name="authorizationTokenId"></param>
        /// <returns></returns>
        public GoogleAuthDTO GetGoogleAuthToken(Guid authorizationTokenId)
        {
            Debug.WriteLine($"Getting google auth token for authorizationTokenId: {authorizationTokenId}");
            Assert.IsNotNull(authorizationTokenId, "The google authorization token is null");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var validToken = uow.AuthorizationTokenRepository.FindTokenById(authorizationTokenId);

                Assert.IsNotNull(validToken, "Reading default google token from AuthorizationTokenRepository failed. Please provide default account for authenticating terminalGoogle.");

                return JsonConvert.DeserializeObject<GoogleAuthDTO>((validToken).Token);
            }
        }
    }
}
