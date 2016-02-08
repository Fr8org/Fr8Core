using StructureMap;
using System.Web.Mvc;
using Utilities;

namespace HubWeb.Controllers
{
    public class RedirectController : Controller
    {
        public ActionResult TwilioSMS()
        {
            var configRepository = ObjectFactory.GetInstance<IConfigRepository>();

            string smsURL = configRepository.Get("DocumentationFr8Site_SMSLink");
            return Redirect(smsURL);
        }
    }
}