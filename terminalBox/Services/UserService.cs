using Box.V2;
using Box.V2.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalBox.DataTransferObjects;

namespace terminalBox.Services
{
    public static class UserService
    {
        public static async Task<string> GetUserExternalId(BoxAuthDTO authToken)
        {
            var config = new BoxConfig(BoxHelpers.ClientId, BoxHelpers.Secret, new Uri(BoxHelpers.RedirectUri));
            Box.V2.Auth.OAuthSession session = new Box.V2.Auth.OAuthSession(authToken.AccessToken, authToken.RefreshToken, (authToken.Expires - DateTime.UtcNow).Seconds, "bearer");
            var client = new BoxClient(config, session);

            var user_info = await client.UsersManager.GetCurrentUserInformationAsync();
            return user_info.Login;
        }


    }
}