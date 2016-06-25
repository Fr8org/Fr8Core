using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Utilities.Configuration;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class AsanaOAuthService: IAsanaOAuth
    {
        public string CreateAuthUrl(string state)
        {
            var resultUrl = CloudConfigurationManager.GetSetting("AsanaOAuthUrl");
            resultUrl.Replace("%STATE%", state);
            return resultUrl;
        }

        public Task<string> GetOAuthToken(string code)
        {
            throw new NotImplementedException();
        }

       
    }
}