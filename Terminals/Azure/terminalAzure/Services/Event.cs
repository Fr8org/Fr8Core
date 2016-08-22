using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalAzure.Services
{
    public class Event : IEvent
    {
        public void Process(string curExternalEventPayload)
        {
            //Process external event payload from Azure Sql Server terminal

        }
    }
}