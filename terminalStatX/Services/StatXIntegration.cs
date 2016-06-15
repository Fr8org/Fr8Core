using System;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using terminalStatX.DataTransferObjects;
using terminalStatX.Interfaces;

namespace terminalStatX.Services
{
    public class StatXIntegration: IStatXIntegration
    {
        private readonly IRestfulServiceClient _restfulServiceClient;
        public StatXIntegration(IRestfulServiceClient restfulServiceClient)
        {
            _restfulServiceClient = restfulServiceClient;
        }

        private string StatXBaseApiUrl => CloudConfigurationManager.GetSetting("StatXApiUrl");
        private const string AuthLoginRelativeUrl = "/auth/login";

        /// <summary>
        /// Returns Client Id for provided client name and phone number. 
        /// Send verification code to user StatX mobile app for authorization
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public async Task<StatXAuthResponseDTO> Login(string clientName, string phoneNumber)
        {
            var statXAuthLoginDTO = new StatXAuthLoginDTO()
            {
                PhoneNumber = phoneNumber,
                ClientName = clientName
            };

            var uri = new Uri(StatXBaseApiUrl + AuthLoginRelativeUrl);
            var response = await _restfulServiceClient.PostAsync<StatXAuthLoginDTO>(
                uri,statXAuthLoginDTO);

            var jObject = JObject.Parse(response);
            
            //check for errors
            JToken errorsToken;
            if (jObject.TryGetValue("errors", out errorsToken))
            {
                if (errorsToken is JArray)
                {
                    var firstError = (JArray) errorsToken.First;
                    return new StatXAuthResponseDTO()
                    {
                        Error = firstError["message"].ToString()
                    };
                }                    
            }

            //return response
            return new StatXAuthResponseDTO()
            {
                PhoneNumber = jObject["phoneNumber"].ToString(),
                ClientName = jObject["clientName"].ToString(),
                ClientId = jObject["clientId"].ToString()
            };
        }

        public Task<StatXAuthDTO> VerifyCodeAndGetAuthToken(string clientId, string phoneNumber, string verificationCode)
        {
            throw new NotImplementedException();
        }
   }
}