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
using Fr8.Infrastructure.Data.Crates;
using StructureMap;
using Newtonsoft.Json;
using InstaSharp.Models;
using terminalInstagram.Models;

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
            parameters.Add(new KeyValuePair<string, string>("verify_token","123")); 
            parameters.Add(new KeyValuePair<string, string>("callback_url", "https://394991e6.ngrok.io/terminals/terminalinstagram/subscribe"));
            var formContent = new FormUrlEncodedContent(parameters);

            var url = new Uri("https://api.instagram.com/v1/subscriptions");
            var x = await _client.PostAsync<JObject>(url, formContent);
            var y = 1;
        }

        public void Unsubscribe(Guid planId)
        {
            throw new NotImplementedException();
        }

        private async void OnMessageReceived(object sender)
        {
        }
        public async Task<List<Crate>> ProcessUserEvents(IContainer container, string curExternalEventPayload)
        {
            var media = JsonConvert.DeserializeObject<InstagramMedia>(curExternalEventPayload);
            if (media.Object != "user")
            {
                throw new Exception("Unknown event source");
            }
            var eventList = new List<Crate>();
            foreach (var entry in media.MediaData)
            {
                var fbEventCM = new FacebookUserEventCM
                {
                    Id = entry.Id,
                    ChangedFields = entry.ChangedFields,
                    Time = entry.Time,
                    UserId = entry.Uid
                };
                var eventReportContent = new EventReportCM
                {
                    EventNames = string.Join(",", fbEventCM.ChangedFields),
                    ContainerDoId = "",
                    EventPayload = new CrateStorage(Crate.FromContent("Facebook user event", fbEventCM)),
                    Manufacturer = "Facebook",
                    ExternalAccountId = fbEventCM.UserId
                };
                ////prepare the event report
                var curEventReport = Crate.FromContent("Facebook user event", eventReportContent);
                eventList.Add(curEventReport);
            }
            return eventList;
        }
    }
}