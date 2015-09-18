using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Managers.APIManagers.Authorizers;
using Data.Entities;
using Data.Interfaces;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using StructureMap;
using Utilities;

namespace Core.Services
{
    public class GoogleSheet
    {
        private GOAuth2RequestFactory CreateRequestFactoryAsync(string userId)
        {
            var parameters = new OAuth2Parameters();
            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();
            parameters.ClientId = configRepository.Get("GoogleClientId");
            parameters.ClientSecret = configRepository.Get("GoogleClientSecret");
            var authorizer = ObjectFactory.GetNamedInstance<IOAuthAuthorizer>("Google");
            parameters.AccessToken = authorizer.GetAccessTokenAsync(userId, CancellationToken.None).Result;
            // Initialize the variables needed to make the request
            return new GOAuth2RequestFactory(null, "kwasantcalendar", parameters);
        }

        private IEnumerable<SpreadsheetEntry> EnumerateSpreadsheets(string userId)
        {
            GOAuth2RequestFactory requestFactory = CreateRequestFactoryAsync(userId);
            SpreadsheetsService service = new SpreadsheetsService("kwasantcalendar");
            service.RequestFactory = requestFactory;
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);
            return feed.Entries.Cast<SpreadsheetEntry>();
        }

        public Dictionary<string, string> EnumerateSpreadsheetsUris(string userId)
        {
            if (userId == null)
                throw new ArgumentNullException("userId");
            return EnumerateSpreadsheets(userId).ToDictionary(entry => entry.Id.AbsoluteUri, entry => entry.Title.Text);
        }

        /// <summary>
        /// Exports Google Spreadsheet data to CSV file and save it on storage
        /// </summary>
        /// <param name="spreadsheetUri">Spreadsheet URI</param>
        /// <param name="userId">User ID</param>
        /// <returns>Returns a link to CSV file on storage</returns>
        public string ExtractData(string spreadsheetUri, string userId)
        {
            if (spreadsheetUri == null)
                throw new ArgumentNullException("spreadsheetUri");
            if (userId == null)
                throw new ArgumentNullException("userId");
            var spreadsheets = EnumerateSpreadsheets(userId);

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
                // Iterate through each row, printing its cell values.
                foreach (ListEntry row in listFeed.Entries)
                {
                    // Print the first column's cell value
                    sw.Write(row.Title.Text);
                    //Console.WriteLine(row.Title.Text);
                    // Iterate over the remaining columns, and print each cell value
                    foreach (ListEntry.Custom element in row.Elements)
                    {
                        sw.Write(",{0}", element.Value);
                        //Console.WriteLine(element.Value);
                    }
                    sw.WriteLine();
                }

                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    var url = uow.FileRepository.SaveRemoteFile(ms, string.Format("GoogleSpreadsheets/{0}.csv", spreadsheetUri));
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
