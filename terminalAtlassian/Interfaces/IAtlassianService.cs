using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.TerminalBase.Models;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianService
    {
        bool IsValidUser(CredentialsDTO curCredential);
        List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationToken authorizationToken);
    }
}