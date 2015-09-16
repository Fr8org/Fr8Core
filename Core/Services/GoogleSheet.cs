using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Newtonsoft.Json;
using StructureMap;
using Utilities;

namespace Core.Services
{
    public class GoogleSheet
    {
        private GOAuth2RequestFactory CreateRequestFactory(IRemoteOAuthDataDO authData)
        {
            var parameters = new OAuth2Parameters();
            var creds = JsonConvert.DeserializeObject<Dictionary<string, string>>(authData.Provider.AppCreds);
            parameters.ClientId = creds["ClientId"];
            parameters.ClientSecret = creds["ClientSecret"];
            var tokenObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(authData.Token);
            parameters.AccessToken = tokenObject["access_token"];
            // Initialize the variables needed to make the request
            return new GOAuth2RequestFactory(null, "kwasantcalendar", parameters);
        }

        private IEnumerable<SpreadsheetEntry> EnumerateSpreadsheets(IRemoteOAuthDataDO authData)
        {
            GOAuth2RequestFactory requestFactory = CreateRequestFactory(authData);
            SpreadsheetsService service = new SpreadsheetsService("kwasantcalendar");
            service.RequestFactory = requestFactory;
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query);
            return feed.Entries.Cast<SpreadsheetEntry>();
        }

        public IEnumerable<Uri> EnumerateSpreadsheetsUris(IRemoteOAuthDataDO authData)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");
            return EnumerateSpreadsheets(authData).Select(entry => new Uri(entry.FeedUri));
        }

        public void ExtractData(Uri spreadsheetUri, IRemoteOAuthDataDO authData)
        {
            if (spreadsheetUri == null)
                throw new ArgumentNullException("spreadsheetUri");
            if (authData == null)
                throw new ArgumentNullException("authData");
            var spreadsheets = EnumerateSpreadsheets(authData);

            SpreadsheetEntry spreadsheet = spreadsheets.SingleOrDefault(ae => ((SpreadsheetEntry)ae).FeedUri == spreadsheetUri.ToString());
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
                    var url = uow.FileRepository.SaveRemoteFile(ms, spreadsheetUri.PathAndQuery);
                    try
                    {
                        FileDO fileDo = new FileDO();
                        fileDo.CloudStorageUrl = url;
                        fileDo.OriginalFileName = spreadsheetUri.ToString();
                        uow.FileRepository.Add(fileDo);
                        uow.SaveChanges();
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
