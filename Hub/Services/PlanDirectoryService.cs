using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.Infrastructure.Utilities.Logging;
using Hub.Interfaces;
using Newtonsoft.Json.Linq;

namespace Hub.Services
{
    public class PlanDirectoryService : IPlanDirectory
    {
        private readonly IHMACService _hmacService;
        private readonly IRestfulServiceClient _client;

        public PlanDirectoryService(IHMACService hmac, IRestfulServiceClient client)
        {
            _hmacService = hmac;
            _client = client;
        }

        public async Task<string> GetToken(string UserId)
        {
            var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/authentication/token");
            var headers =
                await
                    _hmacService.GenerateHMACHeader(uri, "PlanDirectory",
                        CloudConfigurationManager.GetSetting("PlanDirectorySecret"), UserId);

            var json = await _client.PostAsync<JObject>(uri, headers: headers);
            var token = json.Value<string>("token");

            return token;
        }

        public string LogOutUrl()
        {
            return CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/Home/LogoutByToken";
        }
        
    }
}
