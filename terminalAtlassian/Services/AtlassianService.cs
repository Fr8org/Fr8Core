using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Atlassian.Jira;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Hub.Managers.APIManagers.Transmitters.Restful;

using terminalAtlassian.Interfaces;

namespace terminalAtlassian.Services
{
    public class AtlassianService : IAtlassianService
    {
        private readonly IRestfulServiceClient _client;


        public AtlassianService()
        {
            _client = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

        public bool IsValidUser(CredentialsDTO curCredential)
        {
            /*
            var base64Credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", curCredential.Username, curCredential.Password)));
            var headers = new Dictionary<string, string>()
            {
                { System.Net.HttpRequestHeader.Authorization.ToString(), string.Format("Basic {0}", base64Credentials) },
                { System.Net.HttpRequestHeader.Accept.ToString(), "application/json" }
            };
            try {
                var response = _client.GetAsync(new Uri(curCredential.Domain), null, headers).Result;
            }
            catch {
                return false;
            }

            return true;
            */
            

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", curCredential.Username, curCredential.Password))));

                using (HttpResponseMessage response = client.GetAsync(
                            curCredential.Domain).Result)
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
           
        }

        public void SetBasicAuthHeader(WebRequest request, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

        public List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationTokenDO authorizationTokenDO)
        {
            Jira jira = CreateRestClient(authorizationTokenDO.Token);
            var issue = jira.GetIssue(jiraKey);
            return CreateKeyValuePairList(issue);
        }

        private List<FieldDTO> CreateKeyValuePairList(Issue curIssue)
        {
            List<FieldDTO> returnList = new List<FieldDTO>();
            returnList.Add(new FieldDTO("Key", curIssue.Key.Value));
            returnList.Add(new FieldDTO("Summary", curIssue.Summary));
            returnList.Add(new FieldDTO("Reporter", curIssue.Reporter));
            return returnList;
        }

        private Jira CreateRestClient(string token)
        {
            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(token);
            // credentialsDTO.Domain = credentialsDTO.Domain.Replace("http://", "https://");

            return Jira.CreateRestClient(credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
        }
    }
}