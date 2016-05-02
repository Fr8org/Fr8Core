using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Repositories.Encryption;
using Data.Repositories.SqlBased;
using StructureMap;

namespace DataEncryptionTool
{
    class Program
    {
        private class Argument
        {
            public string Value;
            public readonly bool IsOptional;

            public bool HasValue => !string.IsNullOrWhiteSpace(Value);

            public Argument(bool isOptional)
            {
                IsOptional = isOptional;
            }
        }

        static int Main(string[] args)
        {
            var arguments = new Dictionary<string, Argument>
            {
                {"connectionString", new Argument(false)},
                {"kvClientId", new Argument(false)},
                {"kvSecret", new Argument(false)},
                {"kvUrl", new Argument(false)},
                {"overrideDbName", new Argument(true)},
                {"encryptionProvider", new Argument(true)}
            };
            
            for (var i = 0; i < args?.Length; i ++)
            {
                var argKey = args[i].Trim();

                if (argKey.StartsWith("--") && arguments.ContainsKey(argKey.Substring(2)) && i + 1 < args.Length)
                {
                    arguments[argKey.Substring(2)].Value = args[i + 1];
                    i++;
                }
            }

            var mssingAgruments = arguments.Where(x => !x.Value.IsOptional && !x.Value.HasValue).ToArray();
            
            if (mssingAgruments.Length > 0)
            {
                Console.WriteLine("Missing arguments: " + string.Join(", ", mssingAgruments.Select(x => "--" + x.Key)));
                return 1;
            }

            UpdateConfiguration(arguments);

            using (var container = new Container(new DatabaseStructureMapBootStrapper.LiveMode()))
            {
                if (arguments["encryptionProvider"].HasValue)
                {
                    var encyptionProviderName = arguments["encryptionProvider"].Value;
                    var encryptor = container.GetAllInstances<IEncryptionProvider>().FirstOrDefault(x => x.GetType().Name == encyptionProviderName);

                    if (encryptor == null)
                    { 
                        Console.WriteLine($"Unable to find encryption provider {encyptionProviderName}");
                        return 1;
                    }

                    container.Configure(x => { x.For<IEncryptionProvider>().Use(encryptor); });
                }

                EncryptActionStorage(container);
            }
            
            return 0;
        }


        private static void EncryptActionStorage(IContainer container)
        {
            var securityService = container.GetInstance<IEncryptionService>();
            var connectionProvider = container.GetInstance<ISqlConnectionProvider>();
            var cs = (string)connectionProvider.ConnectionInfo;

            using (var connection = new SqlConnection(cs))
            {
                var ids = new List<Guid>();

                connection.Open();

                using (var command = new SqlCommand("select Id from dbo.Actions"))
                {
                    command.Connection = connection;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader["Id"];

                            ids.Add((Guid) id);
                        }
                    }
                }

                int batchSize = 2000;
                var encryptedStorage = new List<KeyValuePair<Guid, byte[]>>();

                for (int i = 0; i < ids.Count; i += batchSize)
                {
                    var batch = ReadBatch(connection, ids, i, batchSize);

                    encryptedStorage.Clear();

                    foreach (var item in batch)
                    {
                        encryptedStorage.Add(new KeyValuePair<Guid, byte[]>(item.Item1, securityService.EncryptData(item.Item2, item.Item3)));
                    }

                    UpdateBatch(connection, encryptedStorage);
                }
            }

        }

        private static void UpdateBatch(SqlConnection connection, List<KeyValuePair<Guid, byte[]>> updatedData)
        {
            using (var command = new SqlCommand())
            {
                StringBuilder updateCommandText = new StringBuilder();

                for (int index = 0; index < updatedData.Count; index++)
                {
                    var keyValuePair = updatedData[index];
                    updateCommandText.AppendFormat("update dbo.Actions set EncryptedCrateStorage = @storage{0} where Id = '{1}'", index, keyValuePair.Key);
                    command.Parameters.AddWithValue("@storage" + index, keyValuePair.Value);
                }

                command.CommandText = updateCommandText.ToString();
                
                command.Connection = connection;

                command.ExecuteNonQuery();
            }
        }


        private static List<Tuple<Guid, string, string>> ReadBatch(SqlConnection connection, List<Guid> ids, int offset, int batchSize)
        {
            var results = new List<Tuple<Guid, string, string>>();

            using (var command = new SqlCommand("select Actions.Id, Fr8AccountId, CrateStorage from dbo.Actions inner join PlanNodes on PlanNodes.Id = Actions.Id where " + string.Join(" OR ", ids.Skip(offset).Take(batchSize).Select(x => $"Actions.Id='{x}'"))))
            { 
                command.Connection = connection;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var accountId = reader["Fr8AccountId"];
                        var storage = reader["CrateStorage"];

                        results.Add(new Tuple<Guid, string, string>((Guid) reader["Id"],
                            accountId != DBNull.Value ? (string) accountId : null,
                            storage != DBNull.Value ? (string) storage : null));
                    }
                }
            }

            return results;
        }

        private static void UpdateConfiguration(Dictionary<string, Argument> arguments)
        {
            var regex = new System.Text.RegularExpressions.Regex("([\\w\\d]{1,})=([\\s\\S]+)");
            var match = regex.Match(arguments["connectionString"].Value);
            if (match == null || !match.Success || match.Groups.Count != 3)
            {
                throw new ArgumentException("Please specify connection string in the following format: \"{ConnectionStringName}={ConnectionString}\".");
            }

            var connectionString = match.Groups[2].Value;
            var csName = match.Groups[1].Value;

            if (arguments["overrideDbName"].HasValue)
            {
                // Override database name in the connection string
                var builder = new SqlConnectionStringBuilder(connectionString);
                builder.InitialCatalog = arguments["overrideDbName"].Value;
                connectionString = builder.ToString();
            }

            UpdateConnectionString(csName, connectionString);

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            configuration.AppSettings.Settings.Remove("KeyVaultClientId");
            configuration.AppSettings.Settings.Add("KeyVaultClientId", arguments["kvClientId"].Value);

            configuration.AppSettings.Settings.Remove("KeyVaultClientSecret");
            configuration.AppSettings.Settings.Add("KeyVaultClientSecret", arguments["kvSecret"].Value);

            configuration.AppSettings.Settings.Remove("KeyVaultUrl");
            configuration.AppSettings.Settings.Add("KeyVaultUrl", arguments["kvUrl"].Value);

            configuration.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void UpdateConnectionString(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.ConnectionStrings.ConnectionStrings[key].ConnectionString = value;

            configuration.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
        }
    }
}
