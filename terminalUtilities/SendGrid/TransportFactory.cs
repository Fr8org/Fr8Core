using System;
using System.Net;
using SendGrid;
using Utilities;
using Utilities.Configuration.Azure;

namespace terminalUtilities.SendGrid
{
    class TransportFactory
    {
        public static ITransport CreateWeb(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException(nameof(configRepository));
            var credentials = new NetworkCredential
            {
                UserName = CloudConfigurationManager.GetSetting("OutboundUserName"),
                Password = CloudConfigurationManager.GetSetting("OutboundUserPassword")
            };
            var web = new Web(credentials);
            return new TransportWrapper(web);
        }
    }
}
