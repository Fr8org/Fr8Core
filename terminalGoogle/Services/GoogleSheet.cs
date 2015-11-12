using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using StructureMap;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using Utilities;
using Utilities.Configuration.Azure;

namespace terminalGoogle.Services
{
    public class GoogleSheet : IGoogleSheet
    {
        private OAuth2Parameters CreateOAuth2Parameters(
            string accessCode = null,
            string accessToken = null,
            string refreshToken = null,
            string state = null)
        {
            return new OAuth2Parameters
            {
                ClientId = CloudConfigurationManager.GetSetting("GoogleClientId"),
                ClientSecret = CloudConfigurationManager.GetSetting("GoogleClientSecret"),
                Scope = CloudConfigurationManager.GetSetting("GoogleScope"),
                RedirectUri = CloudConfigurationManager.GetSetting("GoogleRedirectUri"),
                AccessCode = accessCode,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                State = state,
                AccessType = "offline",
                ApprovalPrompt = "force"
            };
        }

        private GOAuth2RequestFactory CreateRequestFactory(GoogleAuthDTO authDTO)
        {
            var parameters = CreateOAuth2Parameters(accessToken: authDTO.AccessToken, refreshToken: authDTO.RefreshToken);
            // Initialize the variables needed to make the request
            return new GOAuth2RequestFactory(null, "fr8", parameters);
        }

        private IEnumerable<SpreadsheetEntry> EnumerateSpreadsheets(GoogleAuthDTO authDTO)
        {
            GOAuth2RequestFactory requestFactory = CreateRequestFactory(authDTO);
            SpreadsheetsService service = new SpreadsheetsService("fr8");
            service.RequestFactory = requestFactory;
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);
            return feed.Entries.Cast<SpreadsheetEntry>();
        }

        public string CreateOAuth2AuthorizationUrl(string state = null)
        {
            var parameters = CreateOAuth2Parameters(state: state);
            return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
        }

        public GoogleAuthDTO GetToken(string code)
        {
            var parameters = CreateOAuth2Parameters(accessCode: code);
            OAuthUtil.GetAccessToken(parameters);
            return new GoogleAuthDTO()
            {
                AccessToken = parameters.AccessToken,
                Expires = parameters.TokenExpiry,
                RefreshToken = parameters.RefreshToken
            };
        }

        public Dictionary<string, string> EnumerateSpreadsheetsUris(GoogleAuthDTO authDTO)
        {
            return EnumerateSpreadsheets(authDTO)
                .ToDictionary(entry => entry.Id.AbsoluteUri, entry => entry.Title.Text);
        }

        private SpreadsheetEntry FindSpreadsheet(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheets = EnumerateSpreadsheets(authDTO);
            return spreadsheets.SingleOrDefault(ae => string.Equals(ae.Id.AbsoluteUri, spreadsheetUri));
        }

        private IEnumerable<ListEntry> EnumerateRows(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            SpreadsheetEntry spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            SpreadsheetsService service = (SpreadsheetsService)spreadsheet.Service;

            // Get the first worksheet of the spreadsheet.
            WorksheetFeed wsFeed = spreadsheet.Worksheets;
            WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];

            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            return listFeed.Entries.Cast<ListEntry>();
        } 

        public IDictionary<string, string> EnumerateColumnHeaders(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            var firstRow = EnumerateRows(spreadsheetUri, authDTO).FirstOrDefault();
            if (firstRow == null)
                return new Dictionary<string, string>();
            return firstRow.Elements.Cast<ListEntry.Custom>().ToDictionary(e => e.LocalName, e => e.Value);
        }

        public IEnumerable<TableRowDTO> EnumerateDataRows(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            foreach (var row in EnumerateRows(spreadsheetUri, authDTO))
            {
                yield return new TableRowDTO
                {
                    Row = row.Elements
                        .Cast<ListEntry.Custom>()
                        .Select(c => new TableCellDTO
                        {
                            Cell = new FieldDTO(c.LocalName, c.Value)
                        })
                        .ToList()
                };
            }
        }

        /// <summary>
        /// Exports Google Spreadsheet data to CSV file and save it on storage
        /// </summary>
        /// <param name="spreadsheetUri">Spreadsheet URI</param>
        /// <param name="userId">User ID</param>
        /// <returns>Returns a link to CSV file on storage</returns>
        public string ExtractData(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            if (spreadsheetUri == null)
                throw new ArgumentNullException("spreadsheetUri");
            var spreadsheets = EnumerateSpreadsheets(authDTO);

            SpreadsheetEntry spreadsheet = spreadsheets.SingleOrDefault(ae => string.Equals(ae.Id.AbsoluteUri, spreadsheetUri));
            if (spreadsheet == null)
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            SpreadsheetsService service = (SpreadsheetsService) spreadsheet.Service;

            // Get the first worksheet of the spreadsheet.
            WorksheetFeed wsFeed = spreadsheet.Worksheets;
            WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries[0];

            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);

            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                // Iterate through each row
                foreach (ListEntry row in listFeed.Entries)
                {
                    // Iterate over the columns
                    var line = string.Join(",", row.Elements.Cast<ListEntry.Custom>().Select(e => e.Value));
                    sw.WriteLine(line);
                }
                sw.Flush();

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    ms.Position = 0;
                    var url = uow.FileRepository.SaveRemoteFile(ms,
                        string.Format("GoogleSpreadsheets/{0}.csv",
                            spreadsheetUri.Substring(spreadsheetUri.LastIndexOf("/") + 1)));
                    try
                    {
                        FileDO fileDo = new FileDO();
                        fileDo.CloudStorageUrl = url;
                        fileDo.OriginalFileName = spreadsheetUri;
                        uow.FileRepository.Add(fileDo);
                        uow.SaveChanges();
                        return url;
                    }
                    catch (Exception)
                    {
                        uow.FileRepository.DeleteRemoteFile(url);
                        throw;
                    }
                }
            }
        }
    }
}
