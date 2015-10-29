using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalTwilio.Services
{
    public interface IEvent
    {
        void Process(string curExternalEventPayload);
    }
}