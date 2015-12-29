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
        private static readonly ClientCredential ClientCredentials;
        private static readonly KeyVaultClient Client;
        private static readonly string KeyVaultUrl;
        private static readonly int MaxWaitTimeout = 5000;

        static KeyVaultAuthorizationTokenRepository()
        {
            Client = new KeyVaultClient(GetAccessToken);
            ClientCredentials = new ClientCredential(CloudConfigurationManager.GetSetting("KeyVaultClientId"), CloudConfigurationManager.GetSetting("KeyVaultClientSecret"));
            KeyVaultUrl = CloudConfigurationManager.GetSetting("KeyVaultUrl");

            int timeout;
            string timeoutStr = CloudConfigurationManager.GetSetting("KeyVaultTimeout");

            if (!string.IsNullOrWhiteSpace(timeoutStr) && int.TryParse(timeoutStr, out timeout))
            {
                MaxWaitTimeout = timeout;
            }
        }

        public KeyVaultAuthorizationTokenRepository(IUnitOfWork uow) 
            : base(uow)
        {
        }
        
        protected override void ProcessChanges(IEnumerable<AuthorizationTokenDO> adds, IEnumerable<AuthorizationTokenDO> updates, IEnumerable<AuthorizationTokenDO> deletes)
        {
           // using (WebMonitor.Tracer.Monitor.StartFrame("Processing changes"))
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

               // using (WebMonitor.Tracer.Monitor.StartFrame("Commiting"))
                {
                    Task.WaitAll(tasks.ToArray(), MaxWaitTimeout);
                }
            }
        }

        protected override string QuerySecurePart(Guid id)
        {
           // using (WebMonitor.Tracer.Monitor.StartFrame("QuerySecurePart"))
            {
                var task = QuerySecurePartAsync(id);

                //using (WebMonitor.Tracer.Monitor.StartFrame("waiting for result"))
                {
                    task.Wait(MaxWaitTimeout);
                }

                return task.Result;
            }
        }

        private Task<string> QuerySecurePartAsync(Guid id)
        {
            return Task.Run(async () =>
            {
                var secretId = FormatSecretName(id);

                try
                {
                    return (await Client.GetSecretAsync(KeyVaultUrl, secretId)).Value;
                }
                catch
                {
                    return null;
                }
            });
        }

        private Task UpdateSecretAsync(string secret, string value)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Client.SetSecretAsync(KeyVaultUrl, secret, value);
                }
                catch
                {
                }
            });
        }

        private Task DeleteSecretAsync(string secret)
        {
            return Task.Run(async () =>
            {
                try
                {
                    await Client.DeleteSecretAsync(KeyVaultUrl, secret);
                }
                catch
                {
                }
            });
        }

        public static Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            return Task.Run(async () =>
            {
                //using (WebMonitor.Tracer.Monitor.StartFrame("GetAccessToken"))
                {

                    var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
                    var result = await context.AcquireTokenAsync(resource, ClientCredentials);
                    return result.AccessToken;
                }
            });
        }

        private static string FormatSecretName(Guid id)
        {
            return id.ToString("N");
        }
    }
}
