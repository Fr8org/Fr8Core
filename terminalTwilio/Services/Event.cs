
namespace terminalTwilio.Services
{
    public class Event : IEvent
    {
        /*
        private readonly EventReporter _alertReporter;
        private readonly ICrateManager _crate;

        public Event()
        {
            _alertReporter = ObjectFactory.GetInstance<EventReporter>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }
        */
        public void Process(string curExternalEventPayload)
        {
            //parse the external event xml payload
            //List<DocuSignEventDO> curExternalEvents;
            //string curEnvelopeId;
            //Parse(curExternalEventPayload, out curExternalEvents, out curEnvelopeId);

            ////prepare the content from the external event payload
            //var curDocuSignEnvelopeInfo = DocuSignConnectParser.GetEnvelopeInformation(curExternalEventPayload);
            //var eventReportContent = new EventReportMS
            //{
            //    EventNames = "Envelope" + curDocuSignEnvelopeInfo.EnvelopeStatus.Status,
            //    ProcessDOId = "",
            //    EventPayload = ExtractEventPayload(curExternalEvents).ToList(),
            //    ExternalAccountId = curDocuSignEnvelopeInfo.EnvelopeStatus.Email
            //};

            ////prepare the event report
            //CrateDTO curEventReport = ObjectFactory.GetInstance<ICrate>()
            //    .Create("Standard Event Report", JsonConvert.SerializeObject(eventReportContent), "Standard Event Report", 7);

            //string url = Regex.Match(ConfigurationManager.AppSettings["EventWebServerUrl"], @"(\w+://\w+:\d+)").Value + "/api/v1/fr8_events";
            //new HttpClient().PostAsJsonAsync(new Uri(url, UriKind.Absolute), curEventReport);
        }
    }
}