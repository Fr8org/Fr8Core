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
        private const string UserAgent = "DockyardApp";
        private const int ReadWriteTimeout = 10 * 1000;
        private const int Timeout = 20;

        public async Task<Dictionary<string, string>> GetFileList(AuthorizationTokenDO authorizationTokenDO)
        {
            var client = new DropboxClient(authorizationTokenDO.Token, CreateDropboxClientConfig(UserAgent));

            var result = await client.Files.ListFolderAsync(Path);

            return result.Entries.Select(x => new { x.Name, x.PathLower }).ToDictionary(f => f.Name, f => f.PathLower);
        }

        public async Task<string> GetFileSharedUrl(AuthorizationTokenDO authorizationTokenDO, string path)
        {
            var client = new DropboxClient(authorizationTokenDO.Token, CreateDropboxClientConfig(UserAgent));
            try
            {
                var result = await client.Sharing.CreateSharedLinkWithSettingsAsync(path);

                return result.Url;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static DropboxClientConfig CreateDropboxClientConfig(string userAgent)
        {
            return new DropboxClientConfig
            {
                UserAgent = userAgent,
                HttpClient = CreateHttpClient()
            };
        }

        private static HttpClient CreateHttpClient()
        {
            return new HttpClient(new WebRequestHandler { ReadWriteTimeout = ReadWriteTimeout })
            {
                // Specify request level timeout which decides maximum time that can be spent on
                // download/upload files.
                Timeout = TimeSpan.FromMinutes(Timeout)
            };
        }
    }
}