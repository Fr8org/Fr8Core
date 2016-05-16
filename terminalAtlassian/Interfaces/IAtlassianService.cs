using Data.Entities;
using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianService
    {
        bool IsValidUser(CredentialsDTO curCredential);
        List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationTokenDO authorizationTokenDO);
    }
}