using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Utilities.Configuration;
using terminalAsana.Infrastructure;
using terminalAsana.Interfaces;

namespace terminalAsana.Services
{
    public class AsanaIntegrationService : IAsanaIntegration
    {
        public string CreateAuthUrl(string state)
        {
            var resultUrl = CloudConfigurationManager.GetSetting("AsanaOAuthUrl");

            return resultUrl;            
        }

        public Task<string> GetOAuthToken(string code)
        {
            throw new NotImplementedException();
        }

        public Task<AsanaUserInfo> GetUserInfo(string token)
        {
            throw new NotImplementedException();
        }

        public bool PostComment(string text)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetTasks()
        {
            throw new NotImplementedException();
        }
    }
}