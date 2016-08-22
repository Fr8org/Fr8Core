using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using log4net;

namespace HealthMonitor.HubLauncher
{
    class Program
    {
        static SelfHostInitializer _initializer;
        private static bool _quitRequested = false;
        private static object _syncLock = new object();
        private static AutoResetEvent _waitHandle = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            string endpoint = string.Empty;
            string selfHostFactory = string.Empty;
            string connectionString = string.Empty;

            Debug.AutoFlush = true;

            //@tony.yakovets: we need it to load Fr8.Infrastructure otherwise we got an exception trying to configure log4net appenders
            var appender = new Fr8.Infrastructure.Utilities.Logging.fr8RemoteSyslogAppender();
            log4net.Config.XmlConfigurator.Configure(new FileInfo("..\\..\\Config\\log4net.tests.healthMonitor.config"));
            LogManager.GetLogger("HubLauncher").Warn("Hub Launcher starting");


            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            // Start the message pumping thread that enables graceful closing the app
            Thread msgThread = new Thread(MessagePump);
            msgThread.Start();

            if (args != null)
            {
                for (var i = 0; i < args.Length; ++i)
                {
                    if (i > 0 && args[i - 1] == "--endpoint" && args[i] != null)
                    {
                        endpoint = args[i];
                    }

                    if (i > 0 && args[i - 1] == "--selfHostFactory" && args[i] != null)
                    {
                        selfHostFactory = args[i];
                    }
                    else if (i > 0 && args[i - 1] == "--connectionString" && args[i] != null)
                    {
                        connectionString = args[i];
                    }
                }
            }
            else
            {
                ShowHelp();
            }

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(selfHostFactory) || string.IsNullOrEmpty(connectionString))
                ShowHelp();

            var regex = new System.Text.RegularExpressions.Regex("([\\w\\d]{1,})=([\\s\\S]+)");
            var match = regex.Match(connectionString);
            if (match == null || !match.Success || match.Groups.Count != 3)
            {
                throw new ArgumentException("Please specify connection string in the following format: \"{Name}={Value}\".");
            }
            UpdateConnectionString(match.Groups[1].Value, match.Groups[2].Value);

            Console.WriteLine("{0} About to launch self-hosting.", DateTime.UtcNow.ToLongTimeString());
            _initializer = new SelfHostInitializer();
            _initializer.Initialize(selfHostFactory, endpoint);

            Console.WriteLine("{0} Listening...", DateTime.UtcNow.ToLongTimeString());

            // read input to detect "quit" command
            string command = string.Empty;
            do 
            {
                command = Console.ReadLine();
            } while (!command.Equals("quit", StringComparison.InvariantCultureIgnoreCase));
            // signal that we want to quit
            SetQuitRequested();
            // wait until the message pump says it's done
            _waitHandle.WaitOne();
        }

        private static void SetQuitRequested()
        {
            lock (_syncLock)
            {
                _quitRequested = true;
            }
        }

        private static void MessagePump()
        {
            do
            {
                // act on messages
            } while (!_quitRequested);
            _waitHandle.Set();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Exiting...");
            _initializer.Dispose();
        }

        private static void UpdateConnectionString(string key, string value)
        {
            System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.ConnectionStrings.ConnectionStrings[key].ConnectionString = value;
            configuration.Save();

            ConfigurationManager.RefreshSection("connectionStrings");
        }
        
        private static void ShowHelp()
        {
            Console.WriteLine("Usage: HealthMonitor.HubLauncher --endpoint localhost: " +
                "{xxxx} --selfHostFactory {hub type} --connectionString \"{Name}={Value}\" \r\nwhere xxxx is port number to start the Hub on, usually 30643\r\n" +
                "hub type is type which acts as the self-host factory for the Hub, usually 'HubWeb.SelfHostFactory, HubWeb'");
            Environment.Exit(1);
        }
    }
}
