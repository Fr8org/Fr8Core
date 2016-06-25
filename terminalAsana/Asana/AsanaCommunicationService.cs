using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8.Infrastructure.Utilities.Configuration;
using terminalAsana.Interfaces;

namespace terminalAsana.Asana
{
    public class AsanaCommunicationService : IAsanaCommunication
    {
        public string ApiVersion => CloudConfigurationManager.GetSetting("AsanaApiVersion");
    }
}