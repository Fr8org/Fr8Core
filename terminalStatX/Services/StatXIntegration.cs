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

            //check for errors
            //JToken errorsToken;
            //if (jObject.TryGetValue("errors", out errorsToken))
            //{
            //    if (errorsToken is JArray)
            //    {
            //        var firstError = (JArray)errorsToken.First;
            //        return new StatXAuthResponseDTO()
            //        {
            //            Error = firstError["message"].ToString()
            //        };
            //    }
            //}

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

            var headers = new Dictionary<string, string>();
            headers.Add("X-API-KEY", statXAuthDTO.ApiKey);
            headers.Add("X-AUTH-TOKEN", statXAuthDTO.AuthToken);

            var response = await _restfulServiceClient.GetAsync(uri, null, headers);

            var jObject = JObject.Parse(response);

            //check for errors
            //JToken errorsToken;
            //if (jObject.TryGetValue("errors", out errorsToken))
            //{
            //    if (errorsToken is JArray)
            //    {
            //        var firstError = (JArray)errorsToken.First;
            //        return new StatXAuthResponseDTO()
            //        {
            //            Error = firstError["message"].ToString()
            //        };
            //    }
            //}

            var resultSet = new List<StatXGroupDTO>();

            JToken dataToken;
            if (jObject.TryGetValue("data", out dataToken))
            {
                if (dataToken is JArray)
                {
                    resultSet.AddRange(dataToken.Select(item => new StatXGroupDTO()
                    {
                        Id = item["id"].ToString(),
                        Name = item["name"].ToString(),
                        Description = item["description"].ToString()
                    }));
                }
            }

            return resultSet;
        }

        public async Task<List<StatDTO>> GetStatsForGroup(StatXAuthDTO statXAuthDTO, string groupId)
        {
            var headers = new Dictionary<string, string>
            {
                {"X-API-KEY", statXAuthDTO.ApiKey},
                {"X-AUTH-TOKEN", statXAuthDTO.AuthToken}
            };

            var uri = new Uri(StatXBaseApiUrl + "/groups/" + groupId);
            var response = await _restfulServiceClient.GetAsync(
                uri, null, headers);

            var jObject = JObject.Parse(response);

            //check for errors
            //JToken errorsToken;
            //if (jObject.TryGetValue("errors", out errorsToken))
            //{
            //    if (errorsToken is JArray)
            //    {
            //        var firstError = (JArray)errorsToken.First;
            //        return new StatXAuthResponseDTO()
            //        {
            //            Error = firstError["message"].ToString()
            //        };
            //    }
            //}

            //return response

            var resultSet = new List<StatDTO>();

            JToken dataToken;
            if (jObject.TryGetValue("data", out dataToken))
            {
                if (dataToken is JArray)
                {
                    resultSet.AddRange(dataToken.Select(item => new StatDTO()
                    {
                        Id = item["id"].ToString(),
                        Title = item["title"].ToString(),
                        VisualType = item["visualType"].ToString(),
                        Value = item["value"].ToString()
                    }));
                }
            }

            return resultSet;
        }
    }
}