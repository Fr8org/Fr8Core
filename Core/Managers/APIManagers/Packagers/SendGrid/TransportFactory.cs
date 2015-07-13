using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using Utilities;

namespace Core.Managers.APIManagers.Packagers.SendGrid
{
    class TransportFactory
    {
        public static ITransport CreateWeb(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            var credentials = new NetworkCredential
            {
                UserName = configRepository.Get("OutboundUserName"),
                Password = configRepository.Get("OutboundUserPassword")
            };
            var web = new Web(credentials);
            return new TransportWrapper(web);
        }
    }
}
