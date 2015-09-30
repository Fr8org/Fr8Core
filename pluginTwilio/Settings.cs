using System.Web.Http;

namespace pluginTwilio
{
    static class Settings
    {
        /**********************************************************************************/
        // Declarations
        /**********************************************************************************/

        public static readonly string PluginName = "pluginTwilio";

        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

        public static void ConfigureRoutes(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Routes.MapHttpRoute(
                name: "PluginTwilio",
                routeTemplate: Settings.PluginName + "/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );
        }

        /**********************************************************************************/
    }
}