
using System.Configuration;
using Core.ExternalServices.REST;
using Utilities;

namespace PluginUtilities
{
    public static class PluginBase
    {
        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="pluginName">Name of the plugin which is starting up</param>
        public static void AfterStartup(string pluginName)
        {
            ReportStartUp(pluginName);
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="pluginName"></param>
        private static void ReportStartUp(string pluginName)
        {
            //prepare the event information
            var json = string.Format(
                @"{{
                    ""Source"":""{0}"",
                    ""EventType"":""Plugin Incident"",
                    ""Data"":
                        {{
                            ""ObjectId"":""{0}"",
                            ""CustomerId"":""not_applicable"",
                            ""Data"":""service_start_up"",
                            ""PrimaryCategory"":""Operations"",
                            ""SecondaryCategory"":""System Startup"",
                            ""Activity"":""system startup""                                
                        }}
                }}", 
                pluginName);

            //make Post call
            RestfulCallWrapper restCall = PrepareRestCall();
            restCall.AddBody(json, "application/json");
            restCall.Execute();
        }

        /// <summary>
        /// Initializes a new rest call
        /// </summary>
        private static RestfulCallWrapper PrepareRestCall()
        {
            const string eventWebServerUrl = "EventWebServerUrl";

            var restCall = new RestfulCallWrapper();

            string url = ConfigurationManager.AppSettings[eventWebServerUrl];
            restCall.Initialize(url, string.Empty, Method.POST);

            return restCall;
        }
    }
}
