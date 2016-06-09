using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Atlassian.Jira;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Errors;
using Fr8.TerminalBase.Models;
using Newtonsoft.Json;
using StructureMap;
using terminalAtlassian.Interfaces;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;

namespace terminalAtlassian.Services
{
    public class AtlassianService : IAtlassianService
    {
        public bool IsValidUser(CredentialsDTO curCredential)
        {
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

        private void InterceptJiraExceptions(Action process)
        {
            try
            {
                process();
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("Unauthorized (401)") > -1)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException("Please make sure that username, password and domain are correct.");
                }
                else
                {
                    throw;
                }
            }
        }

        private T InterceptJiraExceptions<T>(Func<T> process)
        {
            try
            {
                return process();
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("Unauthorized (401)") > -1)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException("Please make sure that username, password and domain are correct.");
                }
                else
                {
                    throw;
                }
            }
        }

        public List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                Jira jira = CreateRestClient(authToken.Token);
                var issue = jira.GetIssue(jiraKey);
                return CreateKeyValuePairList(issue);
            });
        }

        public List<FieldDTO> GetProjects(AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);

                var projects = jira.GetProjects();
                var result = projects
                    .Select(x => new FieldDTO()
                    {
                        Key = x.Name,
                        Value = x.Key
                    }
                    )
                    .ToList();

                return result;
            });
        }

        public List<FieldDTO> GetIssueTypes(string projectKey,
            AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);

                var issueTypes = jira.GetIssueTypes(projectKey);
                var result = issueTypes
                    .Select(x => new FieldDTO()
                        {
                            Key = x.Name,
                            Value = x.Id
                        }
                    )
                    .ToList();

                return result;
            });
        }

        public List<FieldDTO> GetPriorities(AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);

                var priorities = jira.GetIssuePriorities();
                var result = priorities
                    .Select(x => new FieldDTO()
                        {
                            Key = x.Name,
                            Value = x.Id
                        }
                    )
                    .ToList();

                return result;
            });
        }

        public List<FieldDTO> GetCustomFields(AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);
                var customFields = jira.GetCustomFields();

                var result = customFields
                    .Select(x => new FieldDTO()
                    {
                        Key = x.Name,
                        Value = x.Id
                    }
                    )
                    .OrderBy(x => x.Key)
                    .ToList();

                return result;
            });
        }

        public async Task CreateIssue(IssueInfo issueInfo, AuthorizationToken authToken)
        {
            await InterceptJiraExceptions(async () =>
             {
                 var jira = CreateRestClient(authToken.Token);

                 var issueTypes = jira.GetIssueTypes(issueInfo.ProjectKey);
                 var issueType = issueTypes.FirstOrDefault(x => x.Id == issueInfo.IssueTypeKey);
                 if (issueType == null)
                 {
                     throw new ApplicationException("Invalid Jira Issue Type specified.");
                 }

                 var priorities = jira.GetIssuePriorities();
                 var priority = priorities.FirstOrDefault(x => x.Id == issueInfo.PriorityKey);
                 if (priority == null)
                 {
                     throw new ApplicationException("Invalid Jira Priority specified.");
                 }

                 var jiraCustomFields = jira.GetCustomFields();

                 var issue = jira.CreateIssue(issueInfo.ProjectKey);
                 issue.Type = issueType;
                 issue.Priority = priority;
                 issue.Summary = issueInfo.Summary;
                 issue.Description = issueInfo.Description;

                 if (issueInfo.CustomFields != null)
                 {
                     var customFieldsCollection = issue.CustomFields.ForEdit();
                     foreach (var customField in issueInfo.CustomFields)
                     {
                         var jiraCustomField = jiraCustomFields.FirstOrDefault(x => x.Id == customField.Key);
                         if (jiraCustomField == null)
                         {
                             throw new ApplicationException($"Invalid custom field {customField.Key}");
                         }

                         customFieldsCollection.Add(jiraCustomField.Name, customField.Value);
                     }
                 }

                 var token = await SaveIssue(jira, issue);
                 issueInfo.Key = token;
             });
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
            credentialsDTO.Domain = credentialsDTO.Domain.Replace("http://", "https://");
            if (!credentialsDTO.Domain.StartsWith("https://"))
            {
                credentialsDTO.Domain = "https://" + credentialsDTO.Domain;
            }
            return Jira.CreateRestClient(credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
        }

        public Task<string> SaveIssue(Jira jira, Issue issue)
        {
            Newtonsoft.Json.Linq.JToken token = null;
            return InterceptJiraExceptions(async () =>
            {
                var obj = new { fields = new Dictionary<string, object>() };
                foreach (var item in issue.CustomFields)
                {
                    var value = item.Values.First();
                    int result;
                    if (int.TryParse(value, out result))
                    {
                        obj.fields.Add(item.Id, result);
                    }
                    else
                    {
                        obj.fields.Add(item.Id, value);
                    }

                }
                if (issue.Description != null)
                {
                    obj.fields.Add("description", issue.Description);
                }
                if (issue.Priority != null)
                {
                    obj.fields.Add("priority", new { id = issue.Priority.Id });
                }
                if (issue.Project != null)
                {
                    obj.fields.Add("project", new { key = issue.Project });
                }
                if (issue.Summary != null)
                {
                    obj.fields.Add("summary", issue.Summary);
                }
                if (issue.Type != null)
                {
                    obj.fields.Add("issuetype", new { id = issue.Type.Id });
                }

                token = await jira.RestClient.ExecuteRequestAsync(RestSharp.Method.POST, "/rest/api/2/issue", JsonConvert.SerializeObject(obj));
                return token["key"].ToString();
            });

        }

        public async Task<List<ListItem>> GetSprints(AuthorizationToken authToken, string projectName)
        {
            List<ListItem> list = new List<ListItem>();

            var jira = CreateRestClient(authToken.Token);
            var board = await jira.RestClient.ExecuteRequestAsync(RestSharp.Method.GET, "/rest/agile/1.0/board?projectKeyOrId=" + projectName);
            var boardId = board["values"].First()["id"].ToString();
            var sprints = await jira.RestClient.ExecuteRequestAsync(RestSharp.Method.GET, "/rest/agile/1.0/board/" + boardId + "/sprint");

            foreach (var value in sprints["values"])
            {
                if (value["state"].ToString().ToLower() != "closed")
                {
                    list.Add(new ListItem() { Key = value["name"].ToString(), Value = value["id"].ToString() });
                }
            }

            return list;
        }
    }
}