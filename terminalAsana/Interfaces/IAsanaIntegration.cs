using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using terminalAsana.Asana.Entites;

namespace terminalAsana.Interfaces
{
    public interface IAsanaIntegration
    {
        string CreateAuthUrl(string state);
        Task<string> GetOAuthToken(string code);

        Task<AsanaUser> GetUserInfo(string token);

        bool PostComment(string text);
        IEnumerable<string> GetTasks();

    }
}
