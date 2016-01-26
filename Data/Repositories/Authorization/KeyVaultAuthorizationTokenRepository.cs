using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Segment;
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
        
        protected override void ProcessChanges(
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
                                EventManager.KeyVaultFailed("ProcessChanges", new Exception(taskInfo[i]));
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
                    Logger.WriteSuccess(id.ToString(), val, "query");
                    return val;
                }
                catch (Exception ex)
                {
                    Logger.WriteFailure(id.ToString(), ex.ToString(), "query");
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
                    Logger.WriteSuccess(secret, value, "update");
                }
                catch (Exception ex)
                {
                    Logger.WriteFailure(secret, ex.ToString(), "update");
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
                    Logger.WriteSuccess(secret, null, "delete");
                }
                catch (Exception ex)
                {
                    Logger.WriteFailure(secret, null, "delete");
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
