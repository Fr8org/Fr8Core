using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using StructureMap;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using System.Threading.Tasks;

namespace terminalGoogle.Services
{
    public class GoogleSheet : IGoogleSheet
    {
        private readonly IGoogleIntegration _googleIntegration;

        public GoogleSheet(IGoogleIntegration googleIntegration)
        {
            _googleIntegration = googleIntegration;
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

        public Task<Dictionary<string, string>> GetSpreadsheetsAsync(GoogleAuthDTO authDTO)
        {
            return Task.Run(() => EnumerateSpreadsheets(authDTO).ToDictionary(x => x.Id.AbsoluteUri, x => x.Title.Text));
        }

        public Task<Dictionary<string, string>> GetWorksheetsAsync(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => EnumerateWorksheet(spreadsheetUri, authDTO));
        }

        public Task<Dictionary<string, string>> GetWorksheetHeadersAsync(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => EnumerateColumnHeaders(spreadsheetUri, worksheetUri, authDTO));
        }

        public Task<IEnumerable<TableRowDTO>> GetDataAsync(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => EnumerateDataRows(spreadsheetUri, worksheetUri, authDTO).ToArray() as IEnumerable<TableRowDTO>);
        }

        public Dictionary<string, string> EnumerateSpreadsheetsUris(GoogleAuthDTO authDTO)
        {
            return EnumerateSpreadsheets(authDTO)
                .ToDictionary(entry => entry.Id.AbsoluteUri, entry => entry.Title.Text);
        }

        public SpreadsheetEntry FindSpreadsheet(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheets = EnumerateSpreadsheets(authDTO);
            if(spreadsheetUri.ToLower().Contains("http"))
                return spreadsheets.SingleOrDefault(ae => string.Equals(ae.Id.AbsoluteUri, spreadsheetUri));
            else
                return spreadsheets.SingleOrDefault(ae => ae.Id.AbsoluteUri.Contains(spreadsheetUri));
        }

        private IEnumerable<ListEntry> EnumerateRows(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
            {
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            }
            var service = (SpreadsheetsService)spreadsheet.Service;

            // Get the first worksheet of the spreadsheet.
            var wsFeed = spreadsheet.Worksheets;
            var worksheet = string.IsNullOrEmpty(worksheetUri) ? (WorksheetEntry)wsFeed.Entries[0] : (WorksheetEntry)wsFeed.Entries.FindById(new AtomId(worksheetUri));
            worksheet = worksheet ?? (WorksheetEntry)wsFeed.Entries[0];

            // Define the URL to request the list feed of the worksheet.
            AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            // Fetch the list feed of the worksheet.
            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = service.Query(listQuery);
            return listFeed.Entries.Cast<ListEntry>();
        }

        public Dictionary<string, string> EnumerateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            Dictionary<string, string> worksheet = new Dictionary<string, string>();

            SpreadsheetEntry spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            SpreadsheetsService service = (SpreadsheetsService)spreadsheet.Service;

            WorksheetFeed wsFeed = spreadsheet.Worksheets;

            foreach (var item in wsFeed.Entries)
            {
                worksheet.Add(item.Id.AbsoluteUri, item.Title.Text);
            }

            return worksheet;
        }

        public string CreateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO, string worksheetname)
        {
            SpreadsheetEntry spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
                throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
            SpreadsheetsService service = (SpreadsheetsService)spreadsheet.Service;

            WorksheetEntry newWorksheet = new WorksheetEntry(2000,100, worksheetname);

            WorksheetFeed wfeed = spreadsheet.Worksheets;
            newWorksheet = service.Insert(wfeed, newWorksheet);

            return newWorksheet.Id.AbsoluteUri;
        }

        public async Task DeleteSpreadSheet(string spreadSheetId, GoogleAuthDTO authDTO)
        {
            GoogleDrive googleDrive = new GoogleDrive();
            var driveService = await googleDrive.CreateDriveService(authDTO);
            driveService.Files.Delete(spreadSheetId).Execute();
        }

        public async Task<string> CreateSpreadsheet(string spreadsheetname, GoogleAuthDTO authDTO)
        {
            GoogleDrive googleDrive = new GoogleDrive();
            var driveService = await googleDrive.CreateDriveService(authDTO);

            var file = new Google.Apis.Drive.v2.Data.File();
            file.Title = spreadsheetname;
            file.Description = $"Created via Fr8 at {DateTime.Now}";
            file.MimeType = "application/vnd.google-apps.spreadsheet";

            var request = driveService.Files.Insert(file);
            var result = request.Execute();

            return result.Id;
        }

        public Dictionary<string, string> EnumerateColumnHeaders(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            var firstRow = EnumerateRows(spreadsheetUri, worksheetUri, authDTO).FirstOrDefault();
            if (firstRow == null)
                return new Dictionary<string, string>();
            return firstRow.Elements.Cast<ListEntry.Custom>().ToDictionary(e => e.LocalName, e => e.Value);
        }

        public IEnumerable<TableRowDTO> EnumerateDataRows(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            foreach (var row in EnumerateRows(spreadsheetUri, worksheetUri, authDTO))
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

        public bool WriteData(string spreadsheetUri, string worksheetUri, StandardTableDataCM data, GoogleAuthDTO authDTO)
        {
            if (String.IsNullOrEmpty(spreadsheetUri))
                throw new ArgumentNullException("Spreadsheet Uri parameter is required.");
            if (string.IsNullOrEmpty(worksheetUri))
                throw new ArgumentNullException("Worksheet Uri parameter is required.");

            if (data != null && data.Table.Count > 0)
            {
                int MAX_ROWS = data.Table.Count;
                int MAX_COLS = data.Table[0].Row.Count;

                SpreadsheetEntry spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
                if (spreadsheet == null)
                    throw new ArgumentException("Cannot find a spreadsheet", "spreadsheetUri");
                SpreadsheetsService service = (SpreadsheetsService)spreadsheet.Service;

                string worksheetId = worksheetUri.Substring(worksheetUri.LastIndexOf('/') + 1, worksheetUri.Length - (worksheetUri.LastIndexOf('/') + 1));
                string spreadSheetId = spreadsheetUri;
                if(spreadSheetId.ToLower().Contains("http"))//remove http url
                    spreadSheetId = spreadSheetId.Substring(spreadSheetId.LastIndexOf('/') + 1, spreadSheetId.Length - (spreadSheetId.LastIndexOf('/') + 1));

                CellQuery cellQuery = new CellQuery(spreadSheetId, worksheetId, "private", "full");
                CellFeed cellFeed = service.Query(cellQuery);

                // Build list of cell addresses to be filled in
                List<CellAddress> cellAddrs = new List<CellAddress>();
                for (int row = 0; row < MAX_ROWS; row++)
                {
                    for (int col = 0; col < MAX_COLS; col++)
                    {
                        cellAddrs.Add(new CellAddress(row+1, col+1, data.Table[row].Row[col].Cell.Value));
                    }
                }

                // Prepare the update
                // GetCellEntryMap is what makes the update fast.
                Dictionary<String, CellEntry> cellEntries = GetCellEntryMap(service, cellFeed, cellAddrs);

                CellFeed batchRequest = new CellFeed(cellQuery.Uri, service);
                foreach (CellAddress cellAddr in cellAddrs)
                {
                    CellEntry batchEntry = cellEntries[cellAddr.IdString];
                    batchEntry.InputValue = cellAddr.InputValue;
                    batchEntry.BatchData = new GDataBatchEntryData(cellAddr.IdString, GDataBatchOperationType.update);
                    batchRequest.Entries.Add(batchEntry);
                }

                // Submit the update
                CellFeed batchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

                // Check the results
                foreach (CellEntry entry in batchResponse.Entries)
                {
                    string batchId = entry.BatchData.Id;
                    if (entry.BatchData.Status.Code != 200)
                    {
                        GDataBatchStatus status = entry.BatchData.Status;
                        throw new Exception(string.Format("{0} failed ({1})", batchId, status.Reason));
                    }
                }
            }

            return true;
        }

        /**
     * Connects to the specified {@link SpreadsheetsService} and uses a batch
     * request to retrieve a {@link CellEntry} for each cell enumerated in {@code
     * cellAddrs}. Each cell entry is placed into a map keyed by its RnCn
     * identifier.
     *
     * @param service the spreadsheet service to use.
     * @param cellFeed the cell feed to use.
     * @param cellAddrs list of cell addresses to be retrieved.
     * @return a dictionary consisting of one {@link CellEntry} for each address in {@code
     *         cellAddrs}
     */
        private static Dictionary<String, CellEntry> GetCellEntryMap(
            SpreadsheetsService service, CellFeed cellFeed, List<CellAddress> cellAddrs)
        {
            CellFeed batchRequest = new CellFeed(new Uri(cellFeed.Self), service);
            foreach (CellAddress cellId in cellAddrs)
            {
                CellEntry batchEntry = new CellEntry((uint)cellId.Row, (uint)cellId.Col, cellId.InputValue);
                batchEntry.Id = new AtomId(string.Format("{0}/{1}", cellFeed.Self, cellId.IdString));
                batchEntry.BatchData = new GDataBatchEntryData(cellId.IdString, GDataBatchOperationType.query);
                batchRequest.Entries.Add(batchEntry);
            }

            CellFeed queryBatchResponse = (CellFeed)service.Batch(batchRequest, new Uri(cellFeed.Batch));

            Dictionary<String, CellEntry> cellEntryMap = new Dictionary<String, CellEntry>();
            foreach (CellEntry entry in queryBatchResponse.Entries)
            {
                cellEntryMap.Add(entry.BatchData.Id, entry);
                Console.WriteLine("batch {0} (CellEntry: id={1} editLink={2} inputValue={3})",
                    entry.BatchData.Id, entry.Id, entry.EditUri,
                    entry.InputValue);
            }

            return cellEntryMap;
        }



        class CellAddress
        {
            public int Row;
            public int Col;
            public string InputValue;
            public string IdString;

            /**
             * Constructs a CellAddress representing the specified {@code row} and
             * {@code col}. The IdString will be set in 'RnCn' notation.
             */
            public CellAddress(int row, int col, string inputValue)
            {
                this.Row = row;
                this.Col = col;
                this.InputValue = inputValue;
                this.IdString = string.Format("R{0}C{1}", row, col);
            }
        }
    }
}
