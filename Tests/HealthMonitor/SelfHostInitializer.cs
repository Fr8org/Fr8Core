using HealthMonitor.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitor
{
    public class SelfHostInitializer : IDisposable
    {
        IList<IDisposable> _selfHostedTerminals = new List<IDisposable>();
        Process _hubProcess;

        public void Initialize()
        {
            var selfHostedTerminals = GetSelfHostedTerminals();
            try
            {
                foreach (SelfHostedTerminalsElement terminal in selfHostedTerminals)
                {
                    Type calledType = Type.GetType(terminal.Type);
                    if (calledType == null)
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Unable to instantiate the terminal type {0}.",
                                terminal.Type
                            )
                        );
                    }

                    if (terminal.Type.IndexOf("HubWeb") > -1)
                    {
                        Console.WriteLine("Starting HubLauncher...");
                        // Run the Hub in a separate appdomain to avoid conflict with StructureMap configurations for
                        // termianls and the Hub.
                        StartHub(terminal);
                    }
                    else {
                        MethodInfo curMethodInfo = calledType.GetMethod("CreateServer", BindingFlags.Static | BindingFlags.Public);
                        _selfHostedTerminals.Add((IDisposable)curMethodInfo.Invoke(null, new string[] { terminal.Url }));
                    }
                }
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        private string GetHubLauncherDirectory()
        {
            const string CONFIG =
#if DEV
            "dev";
#elif RELEASE
            "release";
#else
            "debug";
#endif
            var directory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Environment.CurrentDirectory).FullName).FullName);
            return Path.Combine(directory.FullName, "HealthMonitor.HubLauncher\\bin\\", CONFIG);
        }

        private void StartHub(SelfHostedTerminalsElement hub)
        {
            string hubLauncherDirectory = GetHubLauncherDirectory();

            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(hubLauncherDirectory, "HealthMonitor.HubLauncher.exe"), "--endpoint " + hub.Url + " --selfHostFactory \"" + hub.Type + "\"");
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            _hubProcess = new Process();
            _hubProcess.StartInfo = psi;
            _hubProcess.EnableRaisingEvents = true;
            _hubProcess.Start();

            // Waiting for the process to start
            Thread.Sleep(5000);
        }

        private void _hubProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine("HubLauncher: " + e.Data);
        }

        private SelfHostedTerminalsCollection GetSelfHostedTerminals()
        {
            var healthMonitorCS = (HealthMonitorConfigurationSection)
                ConfigurationManager.GetSection("healthMonitor");

            if (healthMonitorCS == null || healthMonitorCS.SelfHostedTerminals == null)
            {
                return null;
            }

            return healthMonitorCS.SelfHostedTerminals;
        }

        public void Dispose()
        {
            foreach (IDisposable selfHostedTerminal in _selfHostedTerminals)
            {
                selfHostedTerminal.Dispose();
            }

            if (_hubProcess != null)
            {
                _hubProcess.StandardInput.WriteLine("quit");
            }
        }
    }
}
