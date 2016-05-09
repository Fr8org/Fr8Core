using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Config;
using Box.V2.Models;

namespace terminalBox.Infrastructure
{
    public class BoxService : IBoxService
    {
        private readonly BoxClient _boxClient;

        public BoxService(BoxAuthTokenDO authToken)
        {
            var config = new BoxConfig(BoxHelpers.ClientId, BoxHelpers.Secret, new Uri(BoxHelpers.RedirectUri));
            Box.V2.Auth.OAuthSession session = new Box.V2.Auth.OAuthSession(authToken.AccessToken, authToken.RefreshToken, (authToken.ExpiresAt - DateTime.UtcNow).Seconds, "bearer");
            _boxClient = new BoxClient(config, session);
        }

        public async Task<ReadOnlyDictionary<string, string>> GetFolderNames()
        {
            var folders = await _boxClient.FoldersManager.GetFolderItemsAsync("0", 500);
            return new ReadOnlyDictionary<string, string>(folders.Entries.ToDictionary(f => f.Id, f => f.Name));

        }

        public async Task<string> SaveFile(string fileName, Stream content)
        {
            var request = new BoxFileRequest
            {
                Name = fileName,
                ContentCreatedAt = DateTime.UtcNow,
                Type = BoxType.file,
                Parent = new BoxRequestEntity { Id = "0" }
            };
            var items = await _boxClient.FoldersManager.GetFolderItemsAsync("0", 500);
            BoxFile resultFile;
            
            if (items.Entries.Any(x => x.Name == fileName))
            {
                resultFile = await _boxClient.FilesManager.UploadNewVersionAsync(fileName, items.Entries.First(x => x.Name == fileName).Id, content);
                return resultFile.Id;
            }
            resultFile = await _boxClient.FilesManager.UploadAsync(request, content);
            return resultFile.Id;
        }

        public async Task<string> GetFileLink(string id)
        {
            return (await _boxClient.FilesManager.GetDownloadUriAsync(id)).AbsoluteUri;
        }

        public async Task<string> GetCurrentUserLogin()
        {
            var user = await _boxClient.UsersManager.GetCurrentUserInformationAsync();
            return user.Login;
        }
    }
}