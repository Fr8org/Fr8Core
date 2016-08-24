using System;
using System.Collections.Generic;
using System.Linq;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using terminalGoogle.DataTransferObjects;
using terminalGoogle.Interfaces;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalGoogle.Services
{
    public class GoogleSheet : IGoogleSheet
    {
        private readonly IGoogleIntegration _googleIntegration;
        private readonly IGoogleDrive _googleDrive;

        public GoogleSheet(IGoogleIntegration googleIntegration, IGoogleDrive googleDrive)
        {
            _googleIntegration = googleIntegration;
            _googleDrive = googleDrive;
        }

        private IEnumerable<SpreadsheetEntry> GetSpreadsheetsImpl(GoogleAuthDTO authDTO)
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

        public Task<Dictionary<string, string>> GetSpreadsheets(GoogleAuthDTO authDTO)
        {
            return Task.Run(() => GetSpreadsheetsImpl(authDTO).ToDictionary(x => x.Id.AbsoluteUri, x => x.Title.Text));
        }

        public Task<Dictionary<string, string>> GetWorksheets(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => GetWorksheetsImpl(spreadsheetUri, authDTO));
        }

        public Task<Dictionary<string, string>> GetWorksheetHeaders(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => EnumerateColumnHeaders(spreadsheetUri, worksheetUri, authDTO));
        }

        public Task<IEnumerable<TableRowDTO>> GetData(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => GetDataImpl(spreadsheetUri, worksheetUri, authDTO).ToArray() as IEnumerable<TableRowDTO>);
        }

        private SpreadsheetEntry FindSpreadsheet(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheets = GetSpreadsheetsImpl(authDTO);
            return spreadsheetUri.ToLower().Contains("http") 
                ? spreadsheets.SingleOrDefault(ae => string.Equals(ae.Id.AbsoluteUri, spreadsheetUri)) 
                : spreadsheets.SingleOrDefault(ae => ae.Id.AbsoluteUri.Contains(spreadsheetUri));
        }

        private IEnumerable<ListEntry> EnumerateRows(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
            {
                throw new ArgumentException("Cannot find a spreadsheet", nameof(spreadsheetUri));
            }
            var service = (SpreadsheetsService)spreadsheet.Service;

            // Get the first worksheet of the spreadsheet.
            var wsFeed = spreadsheet.Worksheets;
            var worksheet = string.IsNullOrEmpty(worksheetUri) ? (WorksheetEntry)wsFeed.Entries[0] : (WorksheetEntry)wsFeed.Entries.FindById(new AtomId(worksheetUri));
            worksheet = worksheet ?? (WorksheetEntry)wsFeed.Entries[0];

            // Define the URL to request the list feed of the worksheet.
            var listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            // Fetch the list feed of the worksheet.
            var listQuery = new ListQuery(listFeedLink.HRef.ToString());
            var listFeed = service.Query(listQuery);
            return listFeed.Entries.Cast<ListEntry>();
        }

        private Dictionary<string, string> GetWorksheetsImpl(string spreadsheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
            {
                throw new ArgumentException("Cannot find a spreadsheet", nameof(spreadsheetUri));
            }
            var wsFeed = spreadsheet.Worksheets;
            return wsFeed.Entries.ToDictionary(item => item.Id.AbsoluteUri, item => item.Title.Text);
        }

        public Task<string> CreateWorksheet(string spreadsheetUri, GoogleAuthDTO authDTO, string worksheetname)
        {
            return Task.Run(() => CreateWorksheetImpl(spreadsheetUri, authDTO, worksheetname));
        }

        public Task DeleteWorksheet(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => DeleteWorksheetImpl(spreadsheetUri, worksheetUri, authDTO));
        }

        private void DeleteWorksheetImpl(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            var spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
            {
                throw new ArgumentException("Cannot find a spreadsheet", nameof(spreadsheetUri));
            }
            var worksheet = spreadsheet.Worksheets.Entries.FindById(new AtomId(worksheetUri));
            if (worksheet == null)
            {
                throw new ArgumentException("Cannot find a worksheet", nameof(worksheetUri));
            }
            worksheet.Delete();
        }

        private string CreateWorksheetImpl(string spreadsheetUri, GoogleAuthDTO authDTO, string worksheetname)
        {
            var spreadsheet = FindSpreadsheet(spreadsheetUri, authDTO);
            if (spreadsheet == null)
            {
                throw new ArgumentException("Cannot find a spreadsheet", nameof(spreadsheetUri));
            }
            var service = (SpreadsheetsService)spreadsheet.Service;

            var newWorksheet = new WorksheetEntry(2000, 100, worksheetname);

            var wfeed = spreadsheet.Worksheets;
            newWorksheet = service.Insert(wfeed, newWorksheet);

            return newWorksheet.Id.AbsoluteUri;
        }

        public async Task DeleteSpreadSheet(string spreadSheetId, GoogleAuthDTO authDTO)
        {
            var driveService = await _googleDrive.CreateDriveService(authDTO);
            driveService.Files.Delete(spreadSheetId).Execute();
        }

        public async Task<string> CreateSpreadsheet(string spreadsheetname, GoogleAuthDTO authDTO)
        {
            var driveService = await _googleDrive.CreateDriveService(authDTO);

            var file = new Google.Apis.Drive.v3.Data.File();
            file.Name = spreadsheetname;
            file.Description = $"Created via Fr8 at {DateTime.Now}";
            file.MimeType = "application/vnd.google-apps.spreadsheet";

            var request = driveService.Files.Create(file);
            var result = request.Execute();

            return result.Id;
        }

        private Dictionary<string, string> EnumerateColumnHeaders(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            var firstRow = EnumerateRows(spreadsheetUri, worksheetUri, authDTO).FirstOrDefault();
            if (firstRow == null)
                return new Dictionary<string, string>();
            return firstRow.Elements.Cast<ListEntry.Custom>().ToDictionary(e => e.LocalName, e => e.Value);
        }

        private IEnumerable<TableRowDTO> GetDataImpl(string spreadsheetUri, string worksheetUri, GoogleAuthDTO authDTO)
        {
            foreach (var row in EnumerateRows(spreadsheetUri, worksheetUri, authDTO))
            {
                yield return new TableRowDTO
                {
                    Row = row.Elements
                        .Cast<ListEntry.Custom>()
                        .Select(c => new TableCellDTO
                        {
                            Cell = new KeyValueDTO(c.LocalName, c.Value)
                        })
                        .ToList()
                };
            }
        }

        public Task WriteData(string spreadsheetUri, string worksheetUri, StandardTableDataCM data, GoogleAuthDTO authDTO)
        {
            return Task.Run(() => WriteDataImpl(spreadsheetUri, worksheetUri, data, authDTO));
        }

        private void WriteDataImpl(string spreadsheetUri, string worksheetUri, StandardTableDataCM data, GoogleAuthDTO authDTO)
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
                if (spreadSheetId.ToLower().Contains("http"))//remove http url
                    spreadSheetId = spreadSheetId.Substring(spreadSheetId.LastIndexOf('/') + 1, spreadSheetId.Length - (spreadSheetId.LastIndexOf('/') + 1));

                CellQuery cellQuery = new CellQuery(spreadSheetId, worksheetId, "private", "full");
                CellFeed cellFeed = service.Query(cellQuery);

                // Build list of cell addresses to be filled in
                List<CellAddress> cellAddrs = new List<CellAddress>();
                for (int row = 0; row < MAX_ROWS; row++)
                {
                    for (int col = 0; col < MAX_COLS; col++)
                    {
                        cellAddrs.Add(new CellAddress(row + 1, col + 1, data.Table[row].Row[col].Cell.Value));
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
