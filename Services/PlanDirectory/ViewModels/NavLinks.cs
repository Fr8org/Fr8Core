using System.Configuration;

namespace PlanDirectory.ViewModels
{
    public static class NavLinks
    {
        public static string BaseUrl = ConfigurationManager.AppSettings["ServerUrl"];
        public static string Blog = "http://blog.fr8.co";
        public static string Developers = "https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md";
        public static string PlanDiscovery = ConfigurationManager.AppSettings["PlanDirectoryUrl"];
    }
}