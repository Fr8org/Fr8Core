using System;
using System.Collections.Generic;
using System.Linq;
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
        private string AuthLoginRelativeUrl => CloudConfigurationManager.GetSetting("AuthLoginRelativeUrl");
        private string AuthVerifyCodeRelativeUrl => CloudConfigurationManager.GetSetting("AuthVerifyCodeRelativeUrl");

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

            var statXAuthResponse= new StatXAuthResponseDTO();

            JToken errorsToken;
            if (jObject.TryGetValue("errors", out errorsToken))
            {
                if ((errorsToken is JArray))
                {
                    var firstError = (JArray)errorsToken.First;

                    if (!string.IsNullOrEmpty(firstError["message"]?.ToString()))
                    {
                        statXAuthResponse.Error = firstError["message"].ToString();
                    }
                }
            }

            //return response
            statXAuthResponse.PhoneNumber = jObject["phoneNumber"]?.ToString();
            statXAuthResponse.ClientName = jObject["clientName"]?.ToString();
            statXAuthResponse.ClientId = jObject["clientId"]?.ToString();

            if (string.IsNullOrEmpty(statXAuthResponse.ClientId))
            {
                throw new ApplicationException("StatX internal login failed. Please try again!");
            } 

            return statXAuthResponse;
        }

        public async Task<StatXAuthDTO> VerifyCodeAndGetAuthToken(string clientId, string phoneNumber, string verificationCode)
        {
            var statXAutVerifyDTO = new StatXAuthVerifyDTO()
            {
                PhoneNumber = phoneNumber,
                ClientId = clientId,
                VerificationCode = verificationCode
            };

            var uri = new Uri(StatXBaseApiUrl + AuthVerifyCodeRelativeUrl);
            var response = await _restfulServiceClient.PostAsync<StatXAuthVerifyDTO>(
                uri, statXAutVerifyDTO);

            var jObject = JObject.Parse(response);

            CheckForExistingErrors(jObject);

            //return response
            return new StatXAuthDTO()
            {
                AuthToken = jObject["authToken"].ToString(),
                ApiKey = jObject["apiKey"].ToString()
            };
        }

        public async Task<List<StatXGroupDTO>> GetGroups(StatXAuthDTO statXAuthDTO)
        {
            var uri = new Uri(StatXBaseApiUrl + "/groups");

            var response = await _restfulServiceClient.GetAsync(uri, null, GetStatxAPIHeaders(statXAuthDTO));

            var jObject = JObject.Parse(response);

            CheckForExistingErrors(jObject);

            var resultSet = new List<StatXGroupDTO>();

            JToken dataToken;
            if (jObject.TryGetValue("data", out dataToken))
            {
                if (dataToken is JArray)
                {
                    foreach (var item in dataToken)
                    {
                        resultSet.Add(new StatXGroupDTO()
                        {
                            Id = item["id"]?.ToString(),
                            Name = item["name"]?.ToString(),
                            Description = item["description"]?.ToString()
                        });
                    }
                }
            }

            return resultSet;
        }

        public async Task<List<StatDTO>> GetStatsForGroup(StatXAuthDTO statXAuthDTO, string groupId)
        {
            var uri = new Uri($"{StatXBaseApiUrl}/groups/{groupId}/stats");
            var response = await _restfulServiceClient.GetAsync(uri, null, GetStatxAPIHeaders(statXAuthDTO));

            var jObject = JObject.Parse(response);

            var resultSet = new List<StatDTO>();

            CheckForExistingErrors(jObject);

            JToken dataToken;
            if (jObject.TryGetValue("data", out dataToken))
            {
                if (dataToken is JArray)
                {
                    resultSet.AddRange(dataToken.Select(item => new StatDTO()
                    {
                        Id = item["id"]?.ToString(),
                        Title = item["title"]?.ToString(),
                        VisualType = item["visualType"]?.ToString(),
                        Value = item["value"]?.ToString()
                    }));
                }
            }

            return resultSet;
        }

        public async Task UpdateStatValue(StatXAuthDTO statXAuthDTO, string groupId, string statId, string value)
        {
            var uri = new Uri($"{StatXBaseApiUrl}/groups/{groupId}/stats/{statId}");
            var response = await _restfulServiceClient.PutAsync(uri, 
                JsonConvert.SerializeObject(new { visualType = "NUMBER",  value = value, lastUpdatedDateTime = DateTime.UtcNow.ToString()}), null, GetStatxAPIHeaders(statXAuthDTO));

            var jObject = JObject.Parse(response);

            CheckForExistingErrors(jObject);
        }
        
        private Dictionary<string, string> GetStatxAPIHeaders(StatXAuthDTO statXAuthDTO)
        {
            var headers = new Dictionary<string, string>
            {
                {"X-API-KEY", statXAuthDTO.ApiKey},
                {"X-AUTH-TOKEN", statXAuthDTO.AuthToken}
            };

            return headers;
        }

        private void CheckForExistingErrors(JObject jObject)
        {
            JToken errorsToken;
            if (!jObject.TryGetValue("errors", out errorsToken)) return;

            if (!(errorsToken is JArray)) return;

            var firstError = (JArray)errorsToken.First;

            if (!string.IsNullOrEmpty(firstError["message"]?.ToString()))
            {
                throw new ApplicationException(firstError["message"].ToString());
            }
        }
    }
}