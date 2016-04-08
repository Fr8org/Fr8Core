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
        private const string Path = "";

        public async Task<List<string>> GetFileList(AuthorizationTokenDO authorizationTokenDO)
        {
            var client = CreateDropboxClient(authorizationTokenDO.Token);

            var result = await client.Files.ListFolderAsync(Path);

            return result.Entries.Select(x => x.Name).ToList();
        }

        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient(new WebRequestHandler { ReadWriteTimeout = 10 * 1000 })
            {
                // Specify request level timeout which decides maximum time that can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(20)
            };
            return httpClient;
        }

        private static DropboxClient CreateDropboxClient(string token)
        {
            return new DropboxClient(token, userAgent: "DockyardApp", httpClient: CreateHttpClient());
        }
    }
}