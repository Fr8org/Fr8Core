using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Interfaces;

namespace terminalAsana.Interfaces
{
    public interface IAsanaOAuthCommunicator : IRestfulServiceClient
    {
        IAsanaOAuth OAuthService { get; set; }
    }
}
