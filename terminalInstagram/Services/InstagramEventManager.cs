using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Fr8.TerminalBase.Models;
using terminalInstagram.Interfaces;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Web.Http;

namespace terminalInstagram.Services
{
    public class InstagramEventManager : IInstagramEventManager
    {
        private readonly IRestfulServiceClient _client;
        private string clientId = CloudConfigurationManager.GetSetting("InstagramClientId");
        private string clientSecret = CloudConfigurationManager.GetSetting("InstagramClientSecret");

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public InstagramEventManager(IRestfulServiceClient client)
        {
            _client = client;
        }


        public async Task Subscribe(AuthorizationToken token, Guid planId)
        {
            var parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("client_id", clientId));
            parameters.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            parameters.Add(new KeyValuePair<string, string>("object", "user")); 
            parameters.Add(new KeyValuePair<string, string>("aspect", "media"));
            parameters.Add(new KeyValuePair<string, string>("verifyToken", "")); 
            parameters.Add(new KeyValuePair<string, string>("callback_url", "http://3bc6742a.ngrok.io/terminals/terminalinstagram/subscribe"));
            var formContent = new FormUrlEncodedContent(parameters);

            var url = new Uri("https://api.instagram.com/v1/subscriptions");
            await _client.PostAsync<JObject>(url, formContent);
        }

        public void Unsubscribe(Guid planId)
        {
            throw new NotImplementedException();
        }

        private async void OnMessageReceived(object sender)
        {
        }
    }
}