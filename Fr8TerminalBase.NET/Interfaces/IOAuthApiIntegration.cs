using System;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Interfaces
{
    /// <summary>
    /// Provides a way to call API of service that uses OAuth 2.0
    /// </summary>
    public interface IOAuthApiIntegration
    {
        /// <summary>
        /// Perform specified call that will be provided with specified authorization token
        /// </summary>
        Task<TResponse> ApiCall<TResponse>(Func<AuthorizationToken, Task<TResponse>> apiCall, AuthorizationToken auth);
    }
}