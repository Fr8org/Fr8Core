using System.Net;
using System.Xml.Linq;
using terminalSalesforce.Infrastructure;
using System.Threading.Tasks;
using System.Web.Http;
using Fr8.Infrastructure.Interfaces;
using Fr8.TerminalBase.Infrastructure;

namespace terminalSalesforce.Controllers
{
    [RoutePrefix("terminals/terminalSalesforce")]
    public class EventController : ApiController
    {
        private readonly IEvent _event;
        private readonly BaseTerminalEvent _baseTerminalEvent;

        public EventController(IEvent @event, IRestfulServiceClient restfulServiceClient)
        {
            _event = @event;
            _baseTerminalEvent = new BaseTerminalEvent(restfulServiceClient);
        }

        [HttpPost]
        [Route("events")]
        public async Task<IHttpActionResult> ProcessIncomingNotification()
        {
            string eventPayLoadContent = Request.Content.ReadAsStringAsync().Result;
            await _baseTerminalEvent.Process(eventPayLoadContent, _event.ProcessEvent);

            //We need to acknowledge the request from Salesforce
            //Creating a SOAP XML response to acknowledge
            string response = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                            xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                             <soapenv:Body>
                              <notificationsResponse xmlns=""http://soap.sforce.com/2005/09/outbound"">
                                     <Ack>true</Ack>
                                  </notificationsResponse>
                              </soapenv:Body>
                            </soapenv:Envelope>";
            var responeXml = XElement.Parse(response);
            return Content(HttpStatusCode.OK, responeXml, GlobalConfiguration.Configuration.Formatters.XmlFormatter);
        }
    }
}