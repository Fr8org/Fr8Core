using System.Threading;
using System.Threading.Tasks;

namespace terminalGoogle.Interfaces
{
    public interface IOAuthAuthorizer
    {
        Task<IOAuthAuthorizationResult> AuthorizeAsync(string userId, string email, string callbackUrl, string currentUrl, CancellationToken cancellationToken);
        Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken);
        Task RefreshTokenAsync(string userId, CancellationToken cancellationToken);
        Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken);
        Task<string> GetRefreshTokenAsync(string userId, CancellationToken cancellationToken);
    }
}