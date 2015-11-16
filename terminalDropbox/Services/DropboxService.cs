using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using terminalDropbox.Interfaces;

namespace terminalDropbox.Services
{
    public class DropboxService : IDropboxService
    {
        public async Task<List<string>> GetFileList(AuthorizationTokenDO authorizationTokenDO)
        {
            List<string> fileNames = new List<string>();
            string path = "";

            var client = CreateDropboxClient(authorizationTokenDO.Token);

            var result = await client.Files.ListFolderAsync(path);
            foreach (var x in result.Entries)
                fileNames.Add(x.Name);

            return fileNames;
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                // Specify request level timeout which decides maximum time taht can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(20)
            };
            return httpClient;
        }

        private DropboxClient CreateDropboxClient(string token)
        {
            return new DropboxClient(token, userAgent: "DockyardApp", httpClient: CreateHttpClient());
        }
    }
}