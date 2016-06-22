using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;
using Fr8.Infrastructure.Utilities.Configuration;
using Fr8.TerminalBase.Errors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PhoneNumbers;
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
            phoneNumber = GeneralisePhoneNumber(phoneNumber);
            var statXAuthLoginDTO = new StatXAuthLoginDTO()
            {
                PhoneNumber = phoneNumber,
                ClientName = "Fr8"
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
            statXAuthResponse.ClientName = clientName;
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
                PhoneNumber = GeneralisePhoneNumber(phoneNumber),
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
                    foreach (var item in dataToken)
                    {
                        var stat = new StatDTO()
                        {
                            Id = item["id"]?.ToString(),
                            Title = item["title"]?.ToString(),
                            VisualType = item["visualType"]?.ToString(),
                            Value = item["value"]?.ToString(),
                            LastUpdatedDateTime = item["lastUpdatedDateTime"]?.ToString()
                        };

                        //check for items 
                        JToken itemsToken;

                        var items = JObject.Parse(item.ToString());
                        if (items.TryGetValue("items", out itemsToken))
                        {
                            foreach (var valueItem in itemsToken)
                            {
                                if (valueItem is JValue)
                                {
                                    stat.StatItems.Add(new StatItemDTO()
                                    {
                                        Name = valueItem.ToString(),
                                        Value = valueItem.ToString()
                                    });
                                }
                                else
                                {
                                    stat.StatItems.Add(new StatItemDTO()
                                    {
                                        Name = valueItem["name"]?.ToString(),
                                        Value = valueItem["value"]?.ToString()
                                    });
                                }
                            }
                        }

                        resultSet.Add(stat);
                    }
                }
            }

            return resultSet;
        }

        public async Task<StatDTO> GetStat(StatXAuthDTO statXAuthDTO, string groupId, string statId)
        {
            var uri = new Uri($"{StatXBaseApiUrl}/groups/{groupId}/stats/{statId}");
            var response = await _restfulServiceClient.GetAsync(uri, null, GetStatxAPIHeaders(statXAuthDTO));

            var jObject = JObject.Parse(response);

            CheckForExistingErrors(jObject);

            var stat = new StatDTO()
            {
                Id = jObject["id"]?.ToString(),
                Title = jObject["title"]?.ToString(),
                VisualType = jObject["visualType"]?.ToString(),
                Value = jObject["value"]?.ToString(),
                LastUpdatedDateTime = jObject["lastUpdatedDateTime"]?.ToString()
            };
          
            //check for items 
            JToken itemsToken;

            var items = JObject.Parse(jObject.ToString());
            if (items.TryGetValue("items", out itemsToken))
            {
                foreach (var valueItem in itemsToken)
                {
                    if (valueItem is JValue)
                    {
                        stat.StatItems.Add(new StatItemDTO()
                        {
                            Name = valueItem.ToString(),
                            Value = valueItem.ToString()
                        });
                    }
                    else
                    {
                        stat.StatItems.Add(new StatItemDTO()
                        {
                            Name = valueItem["name"]?.ToString(),
                            Value = valueItem["value"]?.ToString()
                        });
                    }
                }
            }

            return stat;
        }

        public async Task UpdateStatValue(StatXAuthDTO statXAuthDTO, string groupId, string statId, Dictionary<string, string> statValues)
        {
            var uri = new Uri($"{StatXBaseApiUrl}/groups/{groupId}/stats/{statId}");

            //get the stat and look for value
            var currentStat = await GetStat(statXAuthDTO, groupId, statId);
            if (currentStat != null)
            {
                string response;
                if (string.IsNullOrEmpty(currentStat.Value) && currentStat.StatItems.Any())
                {
                    var updateStatContent = new UpdateStatWithItemsDTO() { LastUpdatedDateTime = DateTime.UtcNow };

                    updateStatContent.Items.AddRange(statValues.Select(x=>new StatItemValueDTO()
                    {
                        Name = x.Key,
                        Value = x.Value
                    }).ToList());

                    response = await _restfulServiceClient.PutAsync<UpdateStatWithItemsDTO>(uri, updateStatContent, null, GetStatxAPIHeaders(statXAuthDTO));
                }
                else
                {
                    var updateStatContent = new UpdateStatDTO
                    {
                        LastUpdatedDateTime = DateTime.UtcNow,
                        Value = statValues.First().Value
                    };

                    response = await _restfulServiceClient.PutAsync<UpdateStatDTO>(uri, updateStatContent, null, GetStatxAPIHeaders(statXAuthDTO));
                }

                var jObject = JObject.Parse(response);

                CheckForExistingErrors(jObject, true);
            }
        }

        #region Helper Methods

        private static Dictionary<string, string> GetStatxAPIHeaders(StatXAuthDTO statXAuthDTO)
        {
            var headers = new Dictionary<string, string>
            {
                {"X-API-KEY", statXAuthDTO.ApiKey},
                {"X-AUTH-TOKEN", statXAuthDTO.AuthToken}
            };

            return headers;
        }

        private static void CheckForExistingErrors(JObject jObject, bool isInRunMode = false)
        {
            JToken errorsToken;
            if (!jObject.TryGetValue("errors", out errorsToken)) return;

            if (!(errorsToken is JArray)) return;

            var firstError = (JArray)errorsToken.First;

            if (string.IsNullOrEmpty(firstError["message"]?.ToString())) return;

            if (isInRunMode)
            {
                throw new ActivityExecutionException(firstError["message"].ToString());
            }
               
            throw new ApplicationException(firstError["message"].ToString());
        }

        private static string GeneralisePhoneNumber(string phoneNumber)
        {
            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();
            phoneNumber = new string(phoneNumber.Where(s => char.IsDigit(s) || s == '+' || (phoneUtil.IsAlphaNumber(phoneNumber) && char.IsLetter(s))).ToArray());
            if (phoneNumber.Length == 10 && !phoneNumber.Contains("+"))
                phoneNumber = "+1" + phoneNumber; //we assume that default region is USA

            if (phoneNumber.Length == 11 && !phoneNumber.Contains("+"))
                phoneNumber = "+" + phoneNumber;
            
            return phoneNumber;
        }

        #endregion
    }
}