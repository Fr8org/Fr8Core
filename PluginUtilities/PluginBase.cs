
using Core.ExternalServices.REST;
using StructureMap;
using Utilities;

namespace PluginUtilities
{
    public static class PluginBase
    {
        public static void AfterStartup(string message)
        {
            var baseUrl = @"http://localhost:46281/api/Event/Event";

            var restCall = new RestfulCallWrapper();
            restCall.Initialize(baseUrl, string.Empty, Method.POST);

            restCall.AddBody(message, "application/json");

            var res = restCall.Execute();
        }
    }
}
