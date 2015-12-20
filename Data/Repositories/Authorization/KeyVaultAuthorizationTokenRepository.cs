using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Utilities.Configuration.Azure;

namespace Data.Repositories
{
    class KeyVaultAuthorizationTokenRepository : AuthorizationTokenRepositoryBase
    {
        private readonly ClientCredential _clientCredentials;
        private readonly KeyVaultClient _client;
        private readonly string _ketVaultUrl;

        public KeyVaultAuthorizationTokenRepository(IUnitOfWork uow) 
            : base(uow)
        {
            _client = new KeyVaultClient(GetAccessToken);
            _clientCredentials = new ClientCredential(CloudConfigurationManager.GetSetting("KeyVaultClientId"), CloudConfigurationManager.GetSetting("KeyVaultClientSecret"));
            _ketVaultUrl = CloudConfigurationManager.GetSetting("KeyVaultUrl");
        }

        protected override void ProcessChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes)
        {
            var tasks = new List<Task>();

            foreach (var token in adds)
            {
                if (string.IsNullOrWhiteSpace(token.Token))
                {
                    continue;
                }

                tasks.Add(UpdateSecretAsync(FormatSecretName(token.Id), token.Token));
            }

            foreach (var token in updates)
            {
                var secretId = FormatSecretName(token.Id);

                if (string.IsNullOrWhiteSpace(token.Token))
                {
                    tasks.Add(DeleteSecretAsync(secretId));
                }
                else
                {
                    tasks.Add(UpdateSecretAsync(secretId, token.Token));
                }
            }

            foreach (var token in deletes)
            {
                tasks.Add(DeleteSecretAsync(FormatSecretName(token.Id)));
            }

            if (tasks.Count == 0)
            {
                return;
            }
            
            Task.WaitAll(tasks.ToArray());
        }

        protected override string QuerySecurePart(Guid id)
        {
            return QuerySecurePartAsync(id).Result;
        }

        private async Task<string> QuerySecurePartAsync(Guid id)
        {
            var secretId = FormatSecretName(id);

            try
            {
                return (await _client.GetSecretAsync(_ketVaultUrl, secretId).ConfigureAwait(false)).Value;
            }
            catch
            {
                return null;
            }
        }

        private async Task UpdateSecretAsync(string secret, string value)
        {
            try
            {
                await _client.SetSecretAsync(_ketVaultUrl, secret, value).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        private async Task DeleteSecretAsync(string secret)
        {
            try
            {
                await _client.DeleteSecretAsync(_ketVaultUrl, secret).ConfigureAwait(false);
            }
            catch
            {
            }
        }

        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, _clientCredentials);

            return result.AccessToken;
        }

        private static string FormatSecretName(Guid id)
        {
            return id.ToString("N");
        }
    }
}
