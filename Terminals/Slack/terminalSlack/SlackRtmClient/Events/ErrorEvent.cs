using terminalSlack.RtmClient.Entities;

namespace terminalSlack.RtmClient.Events
{
    public class ErrorEvent : EventBase
    {
        public Error Error { get; set; }
    }
}
