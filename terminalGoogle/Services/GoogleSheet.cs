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
using System.Threading.Tasks;

namespace terminalGoogle.Services
{
    public class GoogleSheet : IGoogleSheet
    {
        private readonly IGoogleIntegration _googleIntegration;
        public GoogleSheet()
        {
            _googleIntegration = ObjectFactory.GetInstance<IGoogleIntegration>();
        }

        private IEnumerable<SpreadsheetEntry> EnumerateSpreadsheets(GoogleAuthDTO authDTO)
        {
            GOAuth2RequestFactory requestFactory = _googleIntegration.CreateRequestFactory(authDTO);
            SpreadsheetsService service = new SpreadsheetsService("fr8");
            service.RequestFactory = requestFactory;
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);
            return feed.Entries.Cast<SpreadsheetEntry>();
        }

        public Dictionary<string, string> EnumerateSpreadsheetsUris(GoogleAuthDTO authDTO)
        {
            return EnumerateSpreadsheets(authDTO)
                .ToDictionary(entry => entry.Id.AbsoluteUri, entry => entry.Title.Text);
        }

        public SpreadsheetEntry FindSpreadsheet(string spreadsheetUri, GoogleAuthDTO authDTO)
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

        public IDictionary<string, string> EnumerateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            Dictionary<string, string> worksheet = new Dictionary<string, string>();

            SpreadsheetEntry spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            SpreadsheetsService service = (SpreadsheetsService)spreadsheet.Service;

            WorksheetFeed wsFeed = spreadsheet.Worksheets;

            foreach (var item in wsFeed.Entries)
            {
                worksheet.Add(item.Id.ToString(), item.Title.Text);
            }

            return worksheet;
        }

        public void CreateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO, string worksheetname)
        {
            SpreadsheetEntry spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            SpreadsheetsService service = (SpreadsheetsService)spreadsheet.Service;

            WorksheetEntry newWorksheet = new WorksheetEntry(0, 0, worksheetname);

            WorksheetFeed wfeed = spreadsheet.Worksheets;
            service.Insert(wfeed, newWorksheet);
        }

        public async Task<string> CreateSpreadsheet(string spreadsheetname, GoogleAuthDTO authDTO)
        {
            GoogleDrive googleDrive = new GoogleDrive();
            var driveService = await googleDrive.CreateDriveService(authDTO);

            var file = new Google.Apis.Drive.v2.Data.File();
            file.Title = "Test spreadsheet";
            file.Description = string.Format("Created via Fr8 at {0}", DateTime.Now.ToString());
            file.MimeType = "application/vnd.google-apps.spreadsheet";

            var request = driveService.Files.Insert(file);
            var result = request.Execute();

            return result.Id;
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
