using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Config;
using Box.V2.Models;
using terminalBox.DataTransferObjects;

namespace terminalBox.Services
{
    public class BoxService
    {
        public async Task<IEnumerable<KeyValuePair<string, string>>> ListFolders(BoxAuthDTO authToken)
        {
            var config = new BoxConfig(BoxHelpers.ClientId, BoxHelpers.Secret, new Uri(BoxHelpers.RedirectUri));
            Box.V2.Auth.OAuthSession session = new Box.V2.Auth.OAuthSession(authToken.AccessToken, authToken.RefreshToken, (authToken.Expires - DateTime.UtcNow).Seconds, "bearer");
            var client = new BoxClient(config, session);
            
            //client.FoldersManager.
            var folders = await client.FoldersManager.GetFolderItemsAsync("0", 500);

            return folders.Entries.Select(folder => new KeyValuePair<string, string>(folder.SequenceId, folder.Name));
        }
        
        public async Task<string> StoreFile(BoxAuthDTO authToken, string filename, Stream content)
        {
            var config = new BoxConfig(BoxHelpers.ClientId, BoxHelpers.Secret, new Uri(BoxHelpers.RedirectUri));
            Box.V2.Auth.OAuthSession session = new Box.V2.Auth.OAuthSession(authToken.AccessToken, authToken.RefreshToken, (authToken.Expires - DateTime.UtcNow).Seconds, "bearer");
            var client = new BoxClient(config, session);

            var request = new BoxFileRequest
            {
                Name = filename,
                ContentCreatedAt = DateTime.UtcNow,
                Type = BoxType.file,
                Parent = new BoxRequestEntity { Id = "0" }
            };

            var items = await client.FoldersManager.GetFolderItemsAsync("0", 500);
            string link;

            if (items.Entries.Any(x => x.Name == filename))
            {
                var result = await client.FilesManager.UploadNewVersionAsync(filename, items.Entries.First(x => x.Name == filename).Id, content);
                link = (await client.FilesManager.GetDownloadUriAsync(result.Id)).AbsoluteUri;
            }
            else
            {
                var result = await client.FilesManager.UploadAsync(request, content);
                link = (await client.FilesManager.GetDownloadUriAsync(result.Id)).AbsoluteUri;
            }

            return link;
        }
    }
}