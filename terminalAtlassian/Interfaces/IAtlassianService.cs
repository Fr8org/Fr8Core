using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace terminalAtlassian.Interfaces
{
    public interface IAtlassianService
    {
        bool IsValidUser(CredentialsDTO curCredential);
        List<FieldDTO> GetJiraIssue(string jiraKey, AuthorizationTokenDO authorizationTokenDO);
    }
}