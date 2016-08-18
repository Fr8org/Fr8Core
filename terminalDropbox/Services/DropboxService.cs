using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;
using terminalDropbox.Interfaces;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalDropbox.Services
{
    public class DropboxService : IDropboxService
    {
        private const string Path = "";
        private const string UserAgent = "DockyardApp";
        private const int ReadWriteTimeout = 10 * 1000;
        private const int Timeout = 20;

        /// <summary>
        /// Gets file paths from dropbox
        /// </summary>
        /// <param name="authorizationTokenDO"></param>
        /// <returns></returns>
        public async Task<List<string>> GetFileList(AuthorizationToken authorizationToken)
        {
            var client = new DropboxClient(authorizationToken.Token, CreateDropboxClientConfig(UserAgent));

            var result = await client.Files.ListFolderAsync(Path);

            return result.Entries.Where(x => x.IsFile).Select(x => x.PathLower).ToList();
        }

        public async Task<StandardFileDescriptionCM> GetFile(AuthorizationToken authorizationToken, string path)
        {

            var client = new DropboxClient(authorizationToken.Token, CreateDropboxClientConfig(UserAgent));

            var result = await client.Files.DownloadAsync(path);

            var byteArray = await result.GetContentAsByteArrayAsync();

            var fileDescription = new StandardFileDescriptionCM
            {
                TextRepresentation = Convert.ToBase64String(byteArray),
                Filetype = System.IO.Path.GetExtension(result.Response.PathDisplay),
                Filename = System.IO.Path.GetFileName(result.Response.Name)
            };


            return fileDescription;
        }

        /// <summary>
        /// Gets file shared link. If file not shared, shares it.
        /// </summary>
        /// <param name="authorizationTokenDO"></param>
        /// <param name="path">Path to file</param>
        /// <returns></returns>
        public string GetFileSharedUrl(AuthorizationToken authorizationToken, string path)
        {
            var client = new DropboxClient(authorizationToken.Token, CreateDropboxClientConfig(UserAgent));

            // Trying to get file links
            var links = client.Sharing.ListSharedLinksAsync(path).Result.Links;
            if (links.Count > 0)
                return links[0].Url;

            // If file is not shared already, we create a sharing ulr for this file.
            var createResult = client.Sharing.CreateSharedLinkWithSettingsAsync(path).Result;
            return createResult.Url;
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