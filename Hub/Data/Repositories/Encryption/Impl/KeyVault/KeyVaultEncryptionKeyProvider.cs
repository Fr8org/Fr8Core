using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Data.Infrastructure;
using Data.Repositories.Encryption.Impl.Rijndael;
using Data.Repositories.SqlBased;
using Fr8.Infrastructure.Utilities.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Data.Repositories.Encryption.Impl.KeyVault
{
    class KeyVaultEncryptionKeyProvider : IEncryptionKeyProvider
    {
        private readonly ISqlConnectionProvider _sqlConnectionProvider;
        private readonly ClientCredential _clientCredentials;
        private readonly KeyVaultClient _client;
        private readonly string _keyVaultUrl;
        private readonly int _maxWaitTimeout = 5000;
        private readonly RandomNumberGenerator _randomNumber;

        public KeyVaultEncryptionKeyProvider(ISqlConnectionProvider sqlConnectionProvider)
        {
            _sqlConnectionProvider = sqlConnectionProvider;

            _randomNumber = new RNGCryptoServiceProvider();
            
            var clientId = CloudConfigurationManager.GetSetting("KeyVaultClientId");
            var clientSecret = CloudConfigurationManager.GetSetting("KeyVaultClientSecret");

            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
            {
                _clientCredentials = new ClientCredential(clientId, clientSecret);
                _client = new KeyVaultClient(GetAccessToken);
            }

            _keyVaultUrl = CloudConfigurationManager.GetSetting("KeyVaultUrl");

            int timeout;
            string timeoutStr = CloudConfigurationManager.GetSetting("KeyVaultTimeout");

            if (!string.IsNullOrWhiteSpace(timeoutStr) && int.TryParse(timeoutStr, out timeout))
            {
                _maxWaitTimeout = timeout;
            }
        }

        private EncryptionKey QueryEncryptionKey(string secretId)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    var val = (await _client.GetSecretAsync(_keyVaultUrl, secretId)).Value;
                    return val;
                }
                catch (Exception ex)
                {
                    EventManager.KeyVaultFailed("GetSecretAsync", ex);
                    return null;
                }
            });

            task.Wait(_maxWaitTimeout);

            if (string.IsNullOrWhiteSpace(task.Result))
            {
                return null;
            }

            return EncryptionKey.FromByteArray(Convert.FromBase64String(task.Result));
        }

        private void UpdateEncryptionKey(string secretId, EncryptionKey encryptionKey)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await _client.SetSecretAsync(_keyVaultUrl, secretId, Convert.ToBase64String(encryptionKey.ToByteArray()));
                }
                catch (Exception ex)
                {
                    EventManager.KeyVaultFailed("SetSecretAsync", ex);
                }
            });

            task.Wait(_maxWaitTimeout);
        }

        private void ValidateCredentials()
        {
            if (_clientCredentials == null)
            {
                throw new NotSupportedException("KeyVault encryption key provider can't be used here. Provide correct KeyVaultClientId, KeyVaultClientSecret and KeyVaultUrl settings first.");
            }
        }

        public Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            return Task.Run(async () =>
            {
                var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
                var result = await context.AcquireTokenAsync(resource, _clientCredentials);
                return result.AccessToken;
            });
        }

        public EncryptionKey GetEncryptionKey(string peerId, int keyLength, int ivLength)
        {
            ValidateCredentials();

            using (var connection = new SqlConnection { ConnectionString = (string)_sqlConnectionProvider.ConnectionInfo })
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                using (var command = new SqlCommand { Connection = connection, Transaction = transaction})
                {

                    command.CommandText = "select * from dbo.EncryptionTokens where PeerId = @peerId";
                    command.Parameters.AddWithValue("@peerId", peerId);

                    string keyId = null;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keyId = (string) reader["KeyId"];
                            break;
                        }
                    }

                    EncryptionKey encryptionKey;

                    if (string.IsNullOrWhiteSpace(keyId))
                    {
                        keyId = Guid.NewGuid().ToString("N");
                        encryptionKey = GenerateEncryptionKey(keyLength, ivLength);

                        UpdateEncryptionKey(keyId, encryptionKey);

                        command.CommandText = "insert into dbo.EncryptionTokens values (@peerId, @keyId, @createDate)";
                        command.Parameters.AddWithValue("@keyId", keyId);
                        command.Parameters.AddWithValue("@createDate", DateTimeOffset.Now);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        encryptionKey = QueryEncryptionKey(keyId);

                        if (encryptionKey == null)
                        {
                            throw new Exception("Oops... KeyVault lost encryption key information.");
                        }
                    }
                    
                    transaction.Commit();

                    return encryptionKey;
                }
            }
        }
        
        private EncryptionKey GenerateEncryptionKey(int keyLength, int ivLength)
        {
            var keyBytes = new byte[keyLength];
            var ivBytes = new byte[ivLength];

            _randomNumber.GetBytes(keyBytes);
            _randomNumber.GetBytes(ivBytes);

            return new EncryptionKey(keyBytes, ivBytes);
        }
    }
}
