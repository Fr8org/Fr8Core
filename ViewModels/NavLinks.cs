using Fr8.Infrastructure.Utilities.Configuration;

namespace HubWeb.ViewModels
{
    public class NavLinks
    {
        public string BaseUrl = CloudConfigurationManager.GetSetting("BaseUrl");
        public string Blog = "http://blog.fr8.co";
        public string Developers = "https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md";
        public string PlanDiscovery = "http://discovery.fr8.co/";
    }
}