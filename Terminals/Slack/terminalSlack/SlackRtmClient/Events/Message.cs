using terminalSlack.RtmClient.Entities;

namespace terminalSlack.RtmClient.Events
{
    public class Message : MessageBase
    {
        public string Text { get; set; }

        public Edited Edited { get; set; }
    }
}
