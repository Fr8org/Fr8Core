using System;
using System.Net;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using SendGrid;

namespace terminalUtilities.SendGrid
{
    public class TransportFactory
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
