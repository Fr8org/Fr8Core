using HealthMonitor.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitor
{
    public class SelfHostInitializer : IDisposable
    {
        IList<IDisposable> _selfHostedTerminals = new List<IDisposable>();

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

                    MethodInfo curMethodInfo = calledType.GetMethod("CreateServer", BindingFlags.Static | BindingFlags.Public);
                    _selfHostedTerminals.Add((IDisposable)curMethodInfo.Invoke(null, new string[] { terminal.Url }));
                }
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
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
        }
    }
}
