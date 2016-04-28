using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using Data.Infrastructure.StructureMap;
using Data.Repositories.Encryption;
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
                {"connectionString", new Argument (false)},
                {"kvClientId", new Argument (false)},
                {"kvSecret", new Argument (false)},
                {"kvUrl", new Argument (false)},
                {"overrideDbName", new Argument (true)}
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
                Run(container);
            }
            
            return 0;
        }


        private static void Run(IContainer container)
        {
            var securityService = container.GetInstance<IEncryptionService>();
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
