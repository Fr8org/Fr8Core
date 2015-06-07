using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantCore.Managers.APIManagers.Transmitters.Http;
using Newtonsoft.Json;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    class GoogleCalDAVClient : CalDAVClient
    {
        public GoogleCalDAVClient(string endPoint, IAuthorizingHttpChannel channel) : base(endPoint, channel)
        {
        }

        protected override async Task<string> GetCalIdAsync(IRemoteCalendarAuthDataDO authData)
        {
            Func<HttpRequestMessage> requestFactoryMethod = () =>
                {
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), "https://www.googleapis.com/oauth2/v1/userinfo");
                    return request;
                };

            using (var response = await Channel.SendRequestAsync(requestFactoryMethod, authData.UserID))
            {
                var json = await response.Content.ReadAsStringAsync();
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return dictionary["email"];
            }
        }
    }
}