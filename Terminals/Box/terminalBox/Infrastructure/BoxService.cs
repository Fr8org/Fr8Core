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
            var expiresIn = (int)(authToken.ExpiresAt - DateTime.UtcNow).TotalSeconds;
            Box.V2.Auth.OAuthSession session = new Box.V2.Auth.OAuthSession(authToken.AccessToken, authToken.RefreshToken, expiresIn, "bearer");
            _boxClient = new BoxClient(config, session);
        }

        public async Task<ReadOnlyDictionary<string, string>> GetFolderNames()
        {
            var folders = await _boxClient.FoldersManager.GetFolderItemsAsync("0", 500);
            return new ReadOnlyDictionary<string, string>(folders.Entries.ToDictionary(f => f.Id, f => f.Name));
        }

        public async Task<string> SaveFile(string fileName, Stream content)
        {
            var items = _boxClient.FoldersManager.GetFolderItemsAsync("0", 500).Result.Entries;
            BoxFile resultFile;

            if (items.Any(x => x.Name == fileName))
            {
                resultFile = _boxClient.FilesManager.UploadNewVersionAsync(fileName, items.First(x => x.Name == fileName).Id, content).Result;
                return resultFile.Id;
            }
            var request = new BoxFileRequest
            {
                Name = fileName,
                ContentCreatedAt = DateTime.UtcNow,
                Type = BoxType.file,
                Parent = new BoxRequestEntity { Id = "0" }
            };
            resultFile = _boxClient.FilesManager.UploadAsync(request, content).Result;
            return resultFile.Id;
        }

        public async Task<string> GetFileLink(string id)
        {
            return _boxClient.FilesManager.GetDownloadUriAsync(id).Result.AbsoluteUri;
        }

        public async Task<string> GetCurrentUserLogin()
        {
            var user = await _boxClient.UsersManager.GetCurrentUserInformationAsync();
            return user.Login;
        }
    }
}