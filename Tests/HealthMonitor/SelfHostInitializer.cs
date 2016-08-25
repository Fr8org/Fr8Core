using Data.Entities;
using Data.Interfaces;
using HealthMonitor.Configuration;
using StructureMap;
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
using Data;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Fr8.Infrastructure.StructureMap;

namespace HealthMonitor
{
    public class SelfHostInitializer : IDisposable
    {
        IList<IDisposable> _selfHostedTerminals = new List<IDisposable>();
        ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);
        Process _hubProcess;

        private const string HUB_ENDPOINT = "http://localhost:30643";
        private const int CURRENT_TERMINAL_VERSION = 1;

        public void Initialize(string connectionString)
        {
            IEnumerable<TerminalDO> terminals;

            var _container = new Container();
            _container.Configure(expression => expression.AddRegistry<DatabaseStructureMapBootStrapper.LiveMode>());

            var selfHostedApps = GetSelfHostedApps();

            using (var uow = _container.GetInstance<IUnitOfWork>())
            {
                terminals = uow.TerminalRepository.GetAll();
            }
            try
            {
                foreach (SelfHostedAppsElement app in selfHostedApps)
                {
                    Type calledType = Type.GetType(app.Type);
                    if (calledType == null)
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Unable to instantiate the terminal type {0}.",
                                app.Type
                            )
                        );
                    }

                    if (string.Equals(app.Name, "Hub", StringComparison.InvariantCultureIgnoreCase))
                    {
                        app.Endpoint = HUB_ENDPOINT;
                        // Run the Hub in a separate appdomain to avoid conflict with StructureMap configurations for
                        // termianls and the Hub.
                        StartHub(app, connectionString);
                    }
                    else
                    {
                        if (string.Equals(app.Name, "PlanDirectory", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //now PD and Hub is the same thing so PlanDirectoryApiBaseUrl can be replaced with HubApiBaseUrl
                            var uri = new Uri(ConfigurationManager.AppSettings["HubApiBaseUrl"]);
                            app.Endpoint = uri.GetLeftPart(UriPartial.Authority);
                        }
                        else
                        {
                            var terminal = terminals.FirstOrDefault(t => t.Name == app.Name && t.Version == CURRENT_TERMINAL_VERSION.ToString());
                            if (terminal != null)
                            {
                                app.Endpoint = terminal.Endpoint;
                            }
                            else
                            {
                                //hmm this is probably a new terminal - let's check it's endpoint on config file as a last resort
                                app.Endpoint = ConfigurationManager.AppSettings[app.Name+ ".TerminalEndpoint"];
                                if (app.Endpoint == null)
                                {
                                    Console.WriteLine($"Failed to find endpoint settings for terminal {app.Name}");
                                    continue;
                                    //throw new ApplicationException($"Cannot find terminal {app.Name}, version {CURRENT_TERMINAL_VERSION} in the Terminals table.");
                                }
                            }
                        }

                        try
                        {
                            app.Endpoint = Fr8.Testing.Integration.Utilities.NormalizeSchema(app.Endpoint);
                            MethodInfo curMethodInfo = calledType.GetMethod("CreateServer", BindingFlags.Static | BindingFlags.Public);
                            _selfHostedTerminals.Add((IDisposable) curMethodInfo.Invoke(null, new string[] {app.Endpoint}));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to initialize terminal '{app.Name}' at '{app.Endpoint}'", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                throw ;
            }
        }

        /// <summary>
        /// Determine directory where Hub Launcher utility is located. 
        /// It depends on the current build configuration.
        /// </summary>
        /// <returns></returns>
        private string GetHubLauncherDirectory()
        {
            const string CONFIG =
#if DEV
            "dev";
#elif RELEASE
            "release";
#elif DEMO
            "demo";
#else
            "debug";
#endif
            var directory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).FullName).FullName);
            return Path.Combine(directory.FullName, "HealthMonitor.HubLauncher\\bin\\", CONFIG);
        }

        /// <summary>
        /// The function spins off a new process to lanuch the Hub. This is necessary in order 
        /// to avoid component configuration for the Hub and terminals mixing up. 
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="connectionString"></param>
        private void StartHub(SelfHostedAppsElement hub, string connectionString)
        {
            Console.WriteLine("Starting HubLauncher...");
            string hubLauncherDirectory = GetHubLauncherDirectory();
            string args = "--endpoint " + hub.Endpoint + " --selfHostFactory \"" + hub.Type + "\" --connectionString \"" + connectionString + "\"";
            ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(hubLauncherDirectory, "HealthMonitor.HubLauncher.exe"), args);
            Console.WriteLine("HubLauncher Path: " + psi.FileName);
            psi.UseShellExecute = false;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            _hubProcess = new Process();
            _hubProcess.StartInfo = psi;
            _hubProcess.OutputDataReceived += _hubProcess_OutputDataReceived;
            _hubProcess.ErrorDataReceived += _hubProcess_OutputDataReceived;
            _hubProcess.EnableRaisingEvents = true;
            bool started = _hubProcess.Start();
            _hubProcess.BeginOutputReadLine();
            _hubProcess.BeginErrorReadLine();

            if (!started)
            {
                throw new Exception("Cannot start HubLauncher for an unknown reason. Test runner aborted.");
            }

            // Wait for the message from HubLauncher indicating that the Hub has been launched. 
            _waitHandle.Wait(new TimeSpan(0, 3, 0));
            Console.WriteLine("Proceeding to Tests");

        }

        private void _hubProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null) return;

            // Hub Launcher posts  "Listening..." to the standard output when the Hub is ready. 
            if (e.Data.IndexOf("Listening...") > -1)
            {
                // HubLauncher is ready, can start tests
                _waitHandle.Set();
            }
            Console.WriteLine("      HubLauncher:\\> " + e.Data);
        }

        private SelfHostedTerminalsCollection GetSelfHostedApps()
        {
            var healthMonitorCS = (HealthMonitorConfigurationSection)
                ConfigurationManager.GetSection("healthMonitor");

            if (healthMonitorCS == null || healthMonitorCS.SelfHostedApps == null)
            {
                return null;
            }

            return healthMonitorCS.SelfHostedApps;
        }

        public void Dispose()
        {
            foreach (IDisposable selfHostedTerminal in _selfHostedTerminals)
            {
                selfHostedTerminal.Dispose();
            }

            if (_hubProcess != null && !_hubProcess.HasExited)
            {
                Console.WriteLine("Terminating HubLauncher...");
                _hubProcess.StandardInput.WriteLine("quit");
                _hubProcess.WaitForExit();
            }
        }
    }
}
