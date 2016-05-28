using System.Collections.Generic;
using Fr8Data.DataTransferObjects;
using TerminalBase.Models;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianService
    {
        bool IsValidUser(CredentialsDTO curCredential);
        List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationToken authorizationToken);
    }
}