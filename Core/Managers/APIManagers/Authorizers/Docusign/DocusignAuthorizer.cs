using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data.Interfaces;
using Newtonsoft.Json;
using StructureMap;

namespace Core.Managers.APIManagers.Authorizers.Docusign
{
    public class DocusignAuthorizer : IOAuthAuthorizer
    {
        private readonly Uri _docusignLoginFormUri;

        public DocusignAuthorizer(Uri docusignLoginFormUri)
        {
            _docusignLoginFormUri = docusignLoginFormUri;
        }

        private DocusignAuthFlow CreateFlow(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var provider = uow.RemoteCalendarProviderRepository.GetByName("Docusign");
                var creds = JsonConvert.DeserializeAnonymousType(provider.AppCreds,
                    new
                    {
                        Server = "",
                        ApiVersion = "",
                        IntegratorKey = ""
                    });
                return new DocusignAuthFlow(userId, _docusignLoginFormUri)
                {
                    Server = creds.Server,
                    ApiVersion = creds.ApiVersion,
                    IntegratorKey = creds.IntegratorKey,
                };
            } 
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
            var flow = CreateFlow(userId);
            await flow.ObtainTokenAsync(userName, password);
        }

        public async Task RevokeAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            await flow.RevokeTokenAsync();
        }

        public Task RefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotSupportedException("DocuSign doesn't support refreshing tokens");
        }

        public async Task<string> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var flow = CreateFlow(userId);
            return await flow.GetTokenAsync();
        }
    }
}
