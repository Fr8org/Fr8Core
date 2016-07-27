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

        public async Task<string> GetAuthToken(string UserId)
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

        public async Task<bool> Logout(string UserId)
        {
            var result = false;
            var uri = new Uri(CloudConfigurationManager.GetSetting("PlanDirectoryUrl") + "/api/authentication/logout");
            var headers =
                await
                    _hmacService.GenerateHMACHeader(uri, "PlanDirectory",
                        CloudConfigurationManager.GetSetting("PlanDirectorySecret"), UserId);
            Logger.GetLogger().Debug($"Start sending logout request for user {UserId}");
            try
            {
                var json = await _client.PostAsync<JObject>(uri, headers: headers).ConfigureAwait(false);
                result = json.Value<bool>("Result");
            }
            catch (Exception exp)
            {
                Logger.GetLogger().Debug($"Error while logging out from plan directory {exp}");
            }

            return result;
        }
    }
}
