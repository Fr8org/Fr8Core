using System.Configuration;

namespace HubWeb.ViewModels
{
    public static class NavLinks
    {
        private static string _baseUrl;
        public static string BaseUrl
        {
            get
            {
                _baseUrl = ConfigurationManager.AppSettings["ServerProtocol"] +
                          ConfigurationManager.AppSettings["ServerDomainName"];
                var port = ConfigurationManager.AppSettings["ServerPort"];
                if ( port != null && !port.Contains("80") && !port.Contains("443"))
                {
                    _baseUrl = _baseUrl + ':' + port;
                }
                return _baseUrl;
            }
        }
        public static string Blog = "http://blog.fr8.co";
        public static string Documentation = "https://github.com/Fr8org/Fr8Core/blob/master/Docs/Home.md";
        public static string GitHub = "https://github.com/Fr8org/Fr8Core";
        public static string PlanDirectory = ConfigurationManager.AppSettings["PlanDirectoryUrl"] + "/plan_directory/";
    }
}