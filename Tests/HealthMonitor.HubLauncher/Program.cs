using System;
using System.Runtime.InteropServices;
using System.Threading;

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
                }
            }
            else
            {
                ShowHelp();
            }

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(selfHostFactory))
                ShowHelp();

            _initializer = new SelfHostInitializer();
            _initializer.Initialize(selfHostFactory, endpoint);

            Console.WriteLine("Listening...");

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
            Thread.Sleep(100);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: HealthMonitor.HubLauncher --endpoint localhost: " +
                "{xxxx} --selfHostFactory {hub type} \r\nwhere xxxx is port number to start the Hub on, usually 30643\r\n" +
                "hub type is type which acts as the self-host factory for the Hub, usually 'HubWeb.SelfHostFactory, HubWeb'");
            Environment.Exit(1);
        }
    }
}
