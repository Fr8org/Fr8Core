using Data.Entities;
using Data.States;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {
        public RemoteCalendarProviderDO TestRemoteCalendarProvider()
        {
            return new RemoteCalendarProviderDO()
            {
                AppCreds = "",
                AuthType = ServiceAuthorizationType.OAuth2,
                CalDAVEndPoint = "https://test_caldav_endpoint",
                Name = "Test CalDAV provider"
            };
        }

        public RemoteCalendarAuthDataDO TestRemoteCalendarAuthData(RemoteCalendarProviderDO provider, UserDO user)
        {
            return new RemoteCalendarAuthDataDO()
                       {
                           AuthData = "{access_token:test}",
                           ProviderID = provider.Id,
                           Provider = provider,
                           User = user,
                           UserID = user.Id
                       };
        }

        public RemoteCalendarLinkDO TestRemoteCalendarLink(RemoteCalendarProviderDO provider, UserDO user)
        {
            var calendar = new CalendarDO()
                               {
                                   Name = "Test calendar",
                                   Owner = user,
                                   OwnerID = user.Id
                               };
            return new RemoteCalendarLinkDO()
                       {
                           LocalCalendar = calendar,
                           LocalCalendarID = calendar.Id,
                           Provider = provider,
                           ProviderID = provider.Id,
                           RemoteCalendarHref = "url",
                           RemoteCalendarName = "name"
                       };
        }
    }
}
