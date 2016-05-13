using System;
using System.Net;
using SendGrid;
using Utilities;
using Utilities.Configuration.Azure;

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
