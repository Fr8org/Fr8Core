using System.Net;

namespace terminalSalesforce
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public override void Init()
        {
            base.Init();
            //FR-4999: Salesforce can be configured to reject requests that support only TLS 1.0 because of vulnarability. This can be configured on per user basis and it will be forced
            //for every user starting from 04 Mar 2017. This way we additionaly allow TLS 1.1 to be used in our communication with Salesforce
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls11;
        }
    }
}
