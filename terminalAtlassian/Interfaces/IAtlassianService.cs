using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Control;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianService
    {
        Task<bool> CheckAuthenticationAsync(CredentialsDTO credentials);
        Task<bool> CheckDomain(string domain);
        Task<List<KeyValueDTO>> GetJiraIssue(string jiraKey, AuthorizationToken authToken);
        List<KeyValueDTO> GetProjects(AuthorizationToken authToken);
        List<KeyValueDTO> GetIssueTypes(string projectKey, AuthorizationToken authToken);
        List<KeyValueDTO> GetPriorities(AuthorizationToken authToken);
        List<KeyValueDTO> GetCustomFields(AuthorizationToken authToken);
        Task CreateIssue(IssueInfo issueInfo, AuthorizationToken authToken);
        Task<List<UserInfo>> GetUsersAsync(string projectCode, AuthorizationToken token);

        Task<List<ListItem>> GetSprints(AuthorizationToken authToken, string projectName);
    }
}