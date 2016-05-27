using TerminalBase.Infrastructure;

namespace TerminalBase.Models
{
    // This is for data requiered for generic activity requests processing
    // We use dedicated class to avoid ugly Win32 API-like methods with enormous number of parameters
    // Also this will help to add new parameters without forcing ALL activities to be rewritten beacuse of signature change.
    public class ActivityContext
    {
        public ActivityPayload ActivityPayload { get; set; }
        public AuthorizationToken AuthorizationToken { get; set; }
        public string UserId { get; set; }
        public IHubCommunicator HubCommunicator { get; set; }
    }
}
