using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Atlassian.Jira;
using Newtonsoft.Json;
using Fr8Data.DataTransferObjects;
using Fr8Infrastructure.Interfaces;
using TerminalBase.Errors;
using terminalAtlassian.Interfaces;
using TerminalBase.Models;
using terminalAtlassian.Helpers;
using System.Threading.Tasks;

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
                await GetAsync("configuratin", credentials).ConfigureAwait(false);
                return true;
            }
            catch (AuthorizationTokenExpiredOrInvalidException)
            {
                return false;
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

        public List<FieldDTO> GetIssueTypes(string projectKey, AuthorizationToken authToken)
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

        public void CreateIssue(IssueInfo issueInfo, AuthorizationToken authToken)
        {
            InterceptJiraExceptions(() =>
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

                issue.SaveChanges();
                issueInfo.Key = issue.Key.Value;
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

        #region Implementation details

        private const int MaxResults = 1000;

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
            var credentialsDTO = JsonConvert.DeserializeObject<CredentialsDTO>(token).EnforceDomainSchema();
            return Jira.CreateRestClient(credentialsDTO.Domain, credentialsDTO.Username, credentialsDTO.Password);
        }

        #endregion
    }
}