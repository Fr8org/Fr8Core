using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HealthMonitor.Utility
{
    public class JsonRestClient
    {
        public async Task<TResp> PostAsync<TReq, TResp>(string url, TReq req)
        {
            var httpClient = new HttpClient();

            using (var response = await httpClient.PostAsJsonAsync(url, req))
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TResp>(content);

                return result;
            }
        }
    }
}
