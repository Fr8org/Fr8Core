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
using terminalAtlassian.Interfaces;
using terminalAtlassian.Helpers;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;

namespace terminalAtlassian.Services
{
    public class AtlassianService : IAtlassianService
    {
        private readonly IRestfulServiceClient _client;

        public AtlassianService(IRestfulServiceClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            _client = client;
        }

        public async Task<bool> CheckAuthenticationAsync(CredentialsDTO credentials)
        {
            try
            {
                await GetAsync("configuration", credentials).ConfigureAwait(false);
                return true;
            }
            catch (AuthorizationTokenExpiredOrInvalidException)
            {
                return false;
            }           
        }

        public async Task<bool> CheckDomain(string domain)
        {
            Uri uri = new Uri(domain);
            if(uri.AbsolutePath == "/")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<KeyValueDTO>> GetJiraIssue(string jiraKey, AuthorizationToken authToken)
        {
            return await InterceptJiraExceptions(() =>
            {
                Jira jira = CreateRestClient(authToken.Token);
                var issue = jira.GetIssue(jiraKey);
                
                return CreateKeyValuePairList(issue, jira);
            });
        }


        public IEnumerable<Issue> GetIssueFromJql(string jql, AuthorizationToken authToken)
        {
            Jira jira = CreateRestClient(authToken.Token);
            var issues = jira.GetIssuesFromJql(jql);
            return issues;
        }

        public void DeleteIssue(Issue issue, AuthorizationToken authToken)
        {
            Jira jira = CreateRestClient(authToken.Token);
            jira.DeleteIssue(issue);
        }

        public List<KeyValueDTO> GetProjects(AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);

                var projects = jira.GetProjects();
                var result = projects
                    .Select(x => new KeyValueDTO()
                    {
                        Key = x.Name,
                        Value = x.Key
                    }
                    )
                    .ToList();

                return result;
            });
        }

        public List<KeyValueDTO> GetIssueTypes(string projectKey, AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);

                var issueTypes = jira.GetIssueTypes(projectKey);
                var result = issueTypes
                    .Select(x => new KeyValueDTO()
                        {
                            Key = x.Name,
                            Value = x.Id
                        }
                    )
                    .ToList();

                return result;
            });
        }

        public List<KeyValueDTO> GetPriorities(AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);

                var priorities = jira.GetIssuePriorities();
                var result = priorities
                    .Select(x => new KeyValueDTO()
                        {
                            Key = x.Name,
                            Value = x.Id
                        }
                    )
                    .ToList();

                return result;
            });
        }

        public List<KeyValueDTO> GetCustomFields(AuthorizationToken authToken)
        {
            return InterceptJiraExceptions(() =>
            {
                var jira = CreateRestClient(authToken.Token);
                var customFields = jira.GetCustomFields();

                var result = customFields
                    .Select(x => new KeyValueDTO()
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
                issue.Assignee = issueInfo.Assignee;

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

        public async Task<List<UserInfo>> GetUsersAsync(string projectCode, AuthorizationToken token)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                throw new ArgumentException("Project code can't be empty", nameof(projectCode));
            }
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            var response = await GetAsync($"user/assignable/search?project={projectCode}&maxResults={MaxResults}", token);
            var result = JsonConvert.DeserializeObject<List<UserInfo>>(response);
            return result;
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

        #region Implementation details

        private const int MaxResults = 1000;

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

        private async Task<string> GetAsync(string apiRequest, CredentialsDTO credentials)
        {
            credentials = credentials.EnforceDomainSchema();
            using (var httpClient = new HttpClient())
            {
                var userPassword = $"{credentials.Username}:{credentials.Password}";
                userPassword = Convert.ToBase64String(Encoding.Default.GetBytes(userPassword));
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", userPassword);
                var result = await httpClient.GetAsync($"{credentials.Domain}/rest/api/2/{apiRequest}");
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new AuthorizationTokenExpiredOrInvalidException("Please make sure that username, password and domain are correct");
                }
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception($"Response code ({(int)result.StatusCode}) doesn't indicate a successfull request");
                }
                return await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private async Task<string> GetAsync(string apiRequest, AuthorizationToken token)
        {
            return await GetAsync(apiRequest, JsonConvert.DeserializeObject<CredentialsDTO>(token.Token)).ConfigureAwait(false);
        }
        
        private async Task<List<KeyValueDTO>> CreateKeyValuePairList(Issue curIssue, Jira jira)
        {
            var returnList = new List<KeyValueDTO>();
            returnList.Add(new KeyValueDTO("Key", curIssue.Key.Value));
            returnList.Add(new KeyValueDTO("Summary", curIssue.Summary));
            returnList.Add(new KeyValueDTO("ReporterUserName", curIssue.Reporter));
            returnList.Add(new KeyValueDTO("Priority", curIssue.Priority != null ? curIssue.Priority.Name : ""));
            returnList.Add(new KeyValueDTO("Type", curIssue.Type != null ? curIssue.Type.Name : ""));
            returnList.Add(new KeyValueDTO("Status", curIssue.Status != null ? curIssue.Status.Name : ""));
            returnList.Add(new KeyValueDTO("Resolution", curIssue.Resolution != null ? curIssue.Resolution.Name : ""));
            returnList.Add(new KeyValueDTO("Created", curIssue.Created.HasValue ? curIssue.Created.Value.ToString("dd-MM-yyyy") : ""));
            returnList.Add(new KeyValueDTO("Updated", curIssue.Updated.HasValue ? curIssue.Updated.Value.ToString("dd-MM-yyyy") : ""));
            returnList.Add(new KeyValueDTO("Resolved", curIssue.ResolutionDate.HasValue ? curIssue.ResolutionDate.Value.ToString("dd-MM-yyyy") : ""));

            if (curIssue.Components != null && curIssue.Components.Any())
            {
                returnList.Add(new KeyValueDTO("Component", curIssue.Components.First().Name));
            }
            else
            {
                returnList.Add(new KeyValueDTO("Component", ""));
            }

            if (!string.IsNullOrEmpty(curIssue.Assignee))
            {
                var assigneeDisplayName = (await jira.GetUserAsync(curIssue.Assignee)).DisplayName;
                returnList.Add(new KeyValueDTO("Assignee", assigneeDisplayName));
            }
            else
            {
                returnList.Add(new KeyValueDTO("Assignee", ""));
            }

            var reporterDisplayName = (await jira.GetUserAsync(curIssue.Reporter)).DisplayName;
            returnList.Add(new KeyValueDTO("ReporterFullname", reporterDisplayName));

            return returnList;
        }

        private Jira CreateRestClient(string token)
        {
            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(token).EnforceDomainSchema();
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
                if (issue.Assignee != null)
                {
                    obj.fields.Add("assignee", new { name = issue.Assignee });
                }

                token = await jira.RestClient.ExecuteRequestAsync(RestSharp.Method.POST, "/rest/api/2/issue", JsonConvert.SerializeObject(obj));
                return token["key"].ToString();
            });

        }

        #endregion
    }
}