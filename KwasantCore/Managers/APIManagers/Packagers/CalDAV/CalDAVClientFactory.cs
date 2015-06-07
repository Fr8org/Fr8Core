using System;
using Data.Interfaces;
using KwasantCore.Managers.APIManagers.Authorizers;
using KwasantCore.Managers.APIManagers.Transmitters.Http;
using StructureMap;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    public class CalDAVClientFactory : ICalDAVClientFactory
    {
        public ICalDAVClient Create(IRemoteCalendarAuthDataDO authData)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");

            var channel = new OAuthHttpChannel(ObjectFactory.GetNamedInstance<IOAuthAuthorizer>(authData.Provider.Name));
            if (string.Equals(authData.Provider.Name, "google", StringComparison.OrdinalIgnoreCase))
            {
                return new GoogleCalDAVClient(authData.Provider.CalDAVEndPoint, channel);
            }
            return new CalDAVClient(authData.Provider.CalDAVEndPoint, channel);
        }
    }
}