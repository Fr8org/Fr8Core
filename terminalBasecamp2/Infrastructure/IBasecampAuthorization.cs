using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace terminalBasecamp.Infrastructure
{
    public interface IBasecampAuthorization
    {
        ExternalAuthUrlDTO GetExternalAuthUrl();

        Task<AuthorizationTokenDTO> AuthenticateAsync(ExternalAuthenticationDTO externalState);
    }
}