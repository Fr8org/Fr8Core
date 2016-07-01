using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalAsana.Interfaces
{
    public interface IAsanaParameters
    {
        string ApiVersion   { get; }
        string DomainName   { get; }
        string ApiEndpoint  { get; }


        string WorkspacesUrl { get; }
        string TasksUrl      { get; }
        string UsersUrl      { get; }
        string UsersInWorkspaceUrl { get; }
        string UsersMeUrl    { get; }



    }
}
