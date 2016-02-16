using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitor.HubLauncher
{
    public class SelfHostInitializer : IDisposable
    {
        IDisposable _hubReference = null;

        public void Dispose()
        {
            _hubReference.Dispose();
        }

        public void Initialize(string selfHostFactory, string endpoint)
        {
            Type calledType = Type.GetType(selfHostFactory);
            if (calledType == null)
            {
                throw new ArgumentException(
                    string.Format(
                        "Unable to instantiate the terminal type {0}.",
                        selfHostFactory
                    )
                );
            }

            MethodInfo curMethodInfo = calledType.GetMethod("CreateServer", BindingFlags.Static | BindingFlags.Public);
            _hubReference = (IDisposable)curMethodInfo.Invoke(null, new string[] { endpoint });
        }
    }
}
