using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalBase.Models
{
    // This is for data requiered for generic activity requests processing
    // We use dedicated class to avoid ugly Win32 API-like methods with enormous number of parameters
    // Also this will help to add new parameters without forcing ALL activities to be rewritten beacuse of signature change.
    public class ActivityContext
    {
        public ActivityPayload ActivityPayload { get; set; }
        public AuthorizationToken AuthorizationToken { get; }
        public string UserId { get; }
    }
}
