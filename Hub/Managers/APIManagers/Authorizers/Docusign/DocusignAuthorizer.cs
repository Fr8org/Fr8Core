using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Data.Infrastructure;
using Data.Interfaces;
using Utilities;

namespace Hub.Managers.APIManagers.Authorizers.Docusign
{
    public class DocusignAuthorizer : IDocusignAuthorizer
    {
        private readonly IConfigRepository _configRepository;

        public DocusignAuthorizer(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        private DocuSignAuthFlow CreateFlow(string userId)
        {
            return new DocuSignAuthFlow(userId)
            {
                Endpoint = _configRepository.Get("endpoint"),
                IntegratorKey = _configRepository.Get("IntegratorKey"),
            };
        }

        public async Task<IOAuthAuthorizationResult> AuthorizeAsync(string userId, string email, string callbackUrl, string currentUrl,
            CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            var result = await flow.AuthorizeAsync(callbackUrl, currentUrl);
            return result;
        }

        public async Task ObtainAccessTokenAsync(string userId, string userName, string password)
        {
            EventManager.TokenRequestInitiated(userId);
            var flow = CreateFlow(userId);
            await flow.ObtainTokenAsync(userName, password);
            EventManager.TokenObtained(userId);
        }

        public async Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            await flow.RevokeTokenAsync();
            EventManager.TokenRevoked(userId);
        }

        public Task RefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("DocuSign doesn't support refresh tokens");
        }

        public async Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            return await flow.GetTokenAsync();
        }

        public Task<string> GetRefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("DocuSign doesn't support refresh tokens");
        }
    }
}
