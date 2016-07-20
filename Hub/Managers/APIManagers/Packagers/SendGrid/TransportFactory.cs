using System;
using System.Net;
using Fr8.Infrastructure.Utilities;
using Fr8.Infrastructure.Utilities.Configuration;
using SendGrid;

namespace Hub.Managers.APIManagers.Packagers.SendGrid
{
    class TransportFactory
    {
        public static ITransport CreateWeb(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
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