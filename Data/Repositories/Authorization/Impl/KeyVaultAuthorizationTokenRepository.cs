using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Repositories.Authorization;
using Fr8.Infrastructure.Utilities.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

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

        public KeyVaultAuthorizationTokenRepository(IAuthorizationTokenStorageProvider storageProvider) 
            : base(storageProvider)
        {
        }
        
        protected override void ProcessSecureDataChanges(
            IEnumerable<AuthorizationTokenDO> adds,
            IEnumerable<AuthorizationTokenDO> updates,
            IEnumerable<AuthorizationTokenDO> deletes)
        {
           // using (WebMonitor.Tracer.Monitor.StartFrame("Processing changes"))
            {
                var tasks = new List<Task>();
                var taskInfo = new List<string>();

                foreach (var token in adds)
                {
                    if (string.IsNullOrWhiteSpace(token.Token))
                    {
                        continue;
                    }

                    var secretId = FormatSecretName(token.Id);

                    tasks.Add(UpdateSecretAsync(secretId, token.Token));
                    taskInfo.Add(
                        string.Format(
                            "Add new token: Id = {0}, ExternalAccountName = {1}, SecretId = {2}",
                            token.Id.ToString(),
                            token.ExternalAccountId,
                            secretId
                        )
                    );
                }

                foreach (var token in updates)
                {
                    var secretId = FormatSecretName(token.Id);

                    if (string.IsNullOrWhiteSpace(token.Token))
                    {
                        tasks.Add(DeleteSecretAsync(secretId));
                        taskInfo.Add(
                            string.Format(
                                "Delete existing token: Id = {0}, ExternalAccountName = {1}, SecretId = {2}",
                                token.Id.ToString(),
                                token.ExternalAccountId,
                                secretId
                            )
                        );
                    }
                    else
                    {
                        tasks.Add(UpdateSecretAsync(secretId, token.Token));
                        taskInfo.Add(
                            string.Format(
                                "Update existing token: Id = {0}, ExternalAccountName = {1}, SecretId = {2}",
                                token.Id.ToString(),
                                token.ExternalAccountId,
                                secretId
                            )
                        );
                    }
                }

                foreach (var token in deletes)
                {
                    var secretId = FormatSecretName(token.Id);

                    tasks.Add(DeleteSecretAsync(secretId));
                    taskInfo.Add(
                        string.Format(
                            "Delete existing token: Id = {0}, ExternalAccountName = {1}, SecretId = {2}",
                            token.Id.ToString(),
                            token.ExternalAccountId,
                            secretId
                        )
                    );
                }

                if (tasks.Count == 0)
                {
                    return;
                }

               // using (WebMonitor.Tracer.Monitor.StartFrame("Commiting"))
                {
                    var allCompleted = Task.WaitAll(tasks.ToArray(), MaxWaitTimeout);
                    if (!allCompleted)
                    {
                        for (var i = 0; i < tasks.Count; ++i)
                        {
                            if (!tasks[i].IsCompleted)
                            {
                                EventManager.KeyVaultFailed("ProcessSecureDataChanges", new Exception(taskInfo[i]));
                            }
                        }
                    }
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
                    var val = (await Client.GetSecretAsync(KeyVaultUrl, secretId)).Value;
                    /// var val = @"{""Email"":""docusign_developer@dockyard.company"",""ApiPassword"":""abcdefghij"",""AccountId"":""1134655""}";
                    return val;
                }
                catch (Exception ex)
                {
                    EventManager.KeyVaultFailed("GetSecretAsync", ex);
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
                catch (Exception ex)
                {
                    EventManager.KeyVaultFailed("SetSecretAsync", ex);
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
                catch (Exception ex)
                {
                    EventManager.KeyVaultFailed("DeleteSecretAsync", ex);
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
