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
        List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationToken authToken);
        List<FieldDTO> GetProjects(AuthorizationToken authToken);
        List<FieldDTO> GetIssueTypes(string projectKey, AuthorizationToken authToken);
        List<FieldDTO> GetPriorities(AuthorizationToken authToken);
        List<FieldDTO> GetCustomFields(AuthorizationToken authToken);
        Task CreateIssue(IssueInfo issueInfo, AuthorizationToken authToken);
        Task<List<UserInfo>> GetUsersAsync(string projectCode, AuthorizationToken token);

        Task<List<ListItem>> GetSprints(AuthorizationToken authToken, string projectName);
    }
}