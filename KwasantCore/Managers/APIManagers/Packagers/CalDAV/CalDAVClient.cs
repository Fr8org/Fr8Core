using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers.APIManagers.Transmitters.Http;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;

namespace KwasantCore.Managers.APIManagers.Packagers.CalDAV
{
    /// <summary>
    /// CalDAV requests packager
    /// </summary>
    class CalDAVClient : ICalDAVClient
    {
        protected readonly IAuthorizingHttpChannel Channel;

        private readonly string _endPoint;
        private readonly Uri _endPointUri;
        private readonly string _userUrlFormat;

        public CalDAVClient(string endPoint, IAuthorizingHttpChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException("channel");
            if (string.IsNullOrEmpty(endPoint))
                throw new ArgumentException("CalDAV endpoint cannot be empty or null.", "endPoint");
            Channel = channel;
            _endPoint = endPoint;
            _endPointUri = new Uri(_endPoint);
            _userUrlFormat = string.Concat(_endPoint, "/{0}");
        }

        private const string EventsQueryFormat =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
            "<C:calendar-query xmlns:D=\"DAV:\" xmlns:C=\"urn:ietf:params:xml:ns:caldav\">\r\n" +
            "<D:prop>\r\n" +
            "<D:getetag/>\r\n" +
            "<C:calendar-data/>\r\n" +
            "</D:prop>\r\n" +
            "<C:filter>\r\n" +
            "<C:comp-filter name=\"VCALENDAR\">\r\n" +
            "<C:comp-filter name=\"VEVENT\">\r\n" +
            "<C:time-range start=\"{0:yyyyMMddTHHmmssZ}\" end=\"{1:yyyyMMddTHHmmssZ}\"/>\r\n" +
            "</C:comp-filter>\r\n" +
            "</C:comp-filter>\r\n" +
            "</C:filter>\r\n" +
            "</C:calendar-query>";

        private const string CalendarsQuery =
            "<D:propfind xmlns:D=\"DAV:\">\r\n" +
            "  <D:prop>\r\n" +
            "    <D:displayname/>\r\n" +
            "    <D:resourcetype/>\r\n" +
            "  </D:prop>\r\n" +
            "</D:propfind>";        
        
        /// <summary>
        /// Creates a request to CalDAV service for retrieving all calendar events in range of from..to. 
        /// See CalDAV reference for more details.
        /// </summary>
        /// <param name="calendarLink"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<IEnumerable<iCalendar>> GetEventsAsync(IRemoteCalendarLinkDO calendarLink, DateTimeOffset @from, DateTimeOffset to)
        {
            if (calendarLink == null)
                throw new ArgumentNullException("calendarLink");

            var calendarId = calendarLink.RemoteCalendarHref;
            if (string.IsNullOrEmpty(calendarId))
                throw new ArgumentException("Remote calendar url is empty.", "calendarLink");
            var userId = calendarLink.LocalCalendar.Owner.Id;
            
            // Standard structure to get responses from CalDAV (WebDAV) services.
            MultiStatus multiStatus;

            // We need factory method rather than just an instance here as it is required by IHttpChannel.SendRequestAsync. 
            // See that method documentation for more details.
            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("REPORT"), new Uri(_endPointUri, calendarId));
                request.Headers.Add("Depth", "1");
                request.Content = new StringContent(string.Format(EventsQueryFormat, @from.UtcDateTime, to.UtcDateTime), Encoding.UTF8, "application/xml");
                return request;
            };

            using (var response = await Channel.SendRequestAsync(requestFactoryMethod, userId))
            {
                using (var xmlStream = await response.Content.ReadAsStreamAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MultiStatus));
                    multiStatus = (MultiStatus)serializer.Deserialize(xmlStream);
                }
            }

            // extract ICS objects from response
            return multiStatus.Items != null 
                ? multiStatus.Items
                    .Select(r =>
                            {
                                using (var stringReader = new StringReader(r.PropStat.First().Prop.CalendarData))
                                {
                                    return (iCalendar) iCalendar.LoadFromStream(stringReader)[0];
                                }
                            })
                    .ToArray() 
                : new iCalendar[0];
        }

        public async Task CreateEventAsync(IRemoteCalendarLinkDO calendarLink, iCalendar calendarEvent)
        {
            if (calendarLink == null)
                throw new ArgumentNullException("calendarLink");
            if (calendarEvent == null)
                throw new ArgumentNullException("calendarEvent");
            if (calendarEvent.Events == null || calendarEvent.Events.Count == 0)
                throw new ArgumentException("iCalendar object must contain at least one event.", "calendarEvent");

            var calendarId = calendarLink.RemoteCalendarHref;
            var userId = calendarLink.LocalCalendar.Owner.Id;
            var eventId = calendarEvent.Events.First().UID;
            var uri = new Uri(_endPointUri, calendarId);

            // We need factory method rather than just an instance here as it is required by IHttpChannel.SendRequestAsync. 
            // See that method documentation for more details.
            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, string.Concat(uri, eventId));
                request.Headers.Add("If-None-Match", "*");
                iCalendarSerializer serializer = new iCalendarSerializer(calendarEvent);
                string calendarString = serializer.Serialize(calendarEvent);
                request.Content = new StringContent(calendarString, Encoding.UTF8, "text/calendar");
                return request;
            };

            using (var response = await Channel.SendRequestAsync(requestFactoryMethod, userId))
            {
            }
        }

        protected virtual async Task<string> GetCalIdAsync(IRemoteCalendarAuthDataDO authData)
        {
            return authData.User.EmailAddress.Address;
        }

        public async Task<IDictionary<string, string>> GetCalendarsAsync(IRemoteCalendarAuthDataDO authData)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");

            var calId = await GetCalIdAsync(authData);

            // Standard structure to get responses from CalDAV (WebDAV) services.
            MultiStatus multiStatus;

            // We need factory method rather than just an instance here as it is required by IHttpChannel.SendRequestAsync. 
            // See that method documentation for more details.
            Func<HttpRequestMessage> requestFactoryMethod = () =>
            {
                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PROPFIND"), string.Format(_userUrlFormat, calId));
                request.Headers.Add("Depth", "1");
                request.Content = new StringContent(CalendarsQuery, Encoding.UTF8, "application/xml");
                return request;
            };

            using (var response = await Channel.SendRequestAsync(requestFactoryMethod, authData.UserID))
            {
                using (var xmlStream = await response.Content.ReadAsStreamAsync())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MultiStatus));
                    multiStatus = (MultiStatus)serializer.Deserialize(xmlStream);
                }
            }

            Func<multistatusResponsePropstat, bool> propStatFilter =
                propstat => propstat != null &&
                            string.Equals(propstat.Status, "HTTP/1.1 200 OK", StringComparison.Ordinal) &&
                            propstat.Prop != null &&
                            propstat.Prop.ResourceType != null &&
                            propstat.Prop.ResourceType.Calendar != null;
            return multiStatus.Items != null
                ? multiStatus
                    .Items
                    .Where(r => r.PropStat.Any(propStatFilter))
                    .ToDictionary(r => HttpUtility.UrlDecode(r.Href), r => r.PropStat.First(propStatFilter).Prop.DisplayName)
                : new Dictionary<string, string>();
        }

    }
}