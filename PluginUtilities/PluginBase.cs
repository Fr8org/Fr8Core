
using System.Configuration;
using Core.ExternalServices.REST;
using Utilities;

namespace PluginUtilities
{
    public static class PluginBase
    {
        public static void AfterStartup(string message)
        {
            string EventWebServerUrl = "EventWebServerUrl";
            
            var restCall = new RestfulCallWrapper();

            string url = ConfigurationManager.AppSettings[EventWebServerUrl];
            restCall.Initialize(url, string.Empty, Method.POST);

            restCall.AddBody(message, "application/json");

            restCall.Execute();
        }
    }
}
