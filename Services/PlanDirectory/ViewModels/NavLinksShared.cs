using System.Configuration;
using System.Drawing;
using System.Security.Policy;
using Fr8.Infrastructure.Utilities.Configuration;

namespace PlanDirectory.ViewModels
{

    public class NavLinksShared
    {
        public string HowItWorks = CloudConfigurationManager.GetSetting("BaseUrl") + "/#about";
        public string Company = CloudConfigurationManager.GetSetting("BaseUrl") + "/Company";
        public string Services = CloudConfigurationManager.GetSetting("BaseUrl") + "/Services";
        public string Contact = CloudConfigurationManager.GetSetting("BaseUrl") + "/Support";
        public string Blog = "http://blog.fr8.co";
    }
}