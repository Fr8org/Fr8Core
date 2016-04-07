using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Config;
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
            
            var folders = await client.FoldersManager.GetFolderItemsAsync("0", 500);

            return folders.Entries.Select(folder => new KeyValuePair<string, string>(folder.SequenceId, folder.Name));
        }
    }
}