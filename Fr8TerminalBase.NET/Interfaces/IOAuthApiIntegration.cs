using System;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Interfaces
{
    public interface IOAuthApiIntegration
    {
        Task<TResponse> ApiCall<TResponse>(Func<AuthorizationToken, Task<TResponse>> apiCall, AuthorizationToken auth);
    }
}