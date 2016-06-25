using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace terminalAsana.Interfaces
{
    public interface IAsanaOAuth
    {
        string CreateAuthUrl(string state);

        Task<string> GetOAuthToken(string code);

        
    }
}