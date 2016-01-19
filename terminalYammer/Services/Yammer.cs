﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StructureMap;
using Data.Interfaces.DataTransferObjects;
using terminalYammer.Interfaces;
using Utilities.Configuration.Azure;
using terminalYammer.Model;
using Hub.Managers.APIManagers.Transmitters.Restful;

namespace terminalYammer.Services
{
    public class Yammer : IYammer
    {
        private readonly IRestfulServiceClient _client;
        public Yammer()
        {
            _client = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }
        /// <summary>
        /// Build external Yammer OAuth url.
        /// </summary>
        public string CreateAuthUrl(string externalStateToken)
        {
            var url = CloudConfigurationManager.GetSetting("YammerOAuthUrl");
            url = url.Replace("%STATE%", externalStateToken);

            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            var template = CloudConfigurationManager.GetSetting("YammerOAuthAccessUrl");
            var url = template.Replace("%CODE%", code);
            var authEnvelope = await _client.GetAsync<YammerAccessToken>(new Uri(url));
            return authEnvelope.TokenResponse.Token;    
        }

        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }

        private Dictionary<string, string> GetAuthenticationHeader(string oauthToken)
        {
            return new Dictionary<string, string>
            {
                { System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("Bearer {0}", oauthToken) }
            };
        }

        public async Task<string> GetUserId(string oauthToken)
        {
            var url = PrepareTokenUrl("YammerOAuthCurrentUserUrl", oauthToken);
            var jsonObj = await _client.GetAsync<JObject>(new Uri(url), null, GetAuthenticationHeader(oauthToken));
            return jsonObj.Value<string>("email");
        }

        public async Task<List<FieldDTO>> GetGroupsList(string oauthToken)
        {
            var url = PrepareTokenUrl("YammerGroupListUrl", oauthToken);

            var groupsDTO = await _client.GetAsync<List<YammerGroup>>(new Uri(url), null, GetAuthenticationHeader(oauthToken));
            var result = new List<FieldDTO>();
            foreach (var group in groupsDTO)
            {
                result.Add(new FieldDTO()
                {
                    Key = group.Name,
                    Value = group.GroupID
                });
            }

            return result;
            
        }

        public async Task<bool> PostMessageToGroup(string oauthToken, string groupId, string message)
        {
            var url = CloudConfigurationManager.GetSetting("YammerPostMessageUrl");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + oauthToken);

            var content = new FormUrlEncodedContent(
                new[] { 
                    new KeyValuePair<string, string>("group_id", groupId),
                    new KeyValuePair<string, string>("body", message)
                }
            );

            using (var response = await httpClient.PostAsync(url, content))
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    return true;
                }
                return false;
            }
        }
    }
}