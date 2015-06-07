using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using KwasantCore.Exceptions;
using KwasantCore.Managers.APIManagers.Packagers.CalDAV;
using KwasantCore.Services;
using Utilities.Logging;
using StructureMap;
using EventCreateType = Data.States.EventCreateType;

namespace KwasantCore.Managers
{
    public class CalendarSyncManager
    {
        class EventComparer : IEqualityComparer<EventDO>
        {
            public bool Equals(EventDO x, EventDO y)
            {
                return x.StartDate == y.StartDate
                    && x.EndDate == y.EndDate
                    && string.Equals(x.Summary, y.Summary);
            }

            public int GetHashCode(EventDO obj)
            {
                unchecked
                {
                    var hashCode = obj.StartDate.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.EndDate.GetHashCode();
                    hashCode = (hashCode * 397) ^ (obj.Summary != null ? obj.Summary.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        static CalendarSyncManager()
        {
            UnitOfWork.EntitiesAdded += UnitOfWork_OnEntitiesAddedOrModified;
            UnitOfWork.EntitiesModified += UnitOfWork_OnEntitiesAddedOrModified;
        }

        /// <summary>
        /// Gets or sets whether to disable auto-synchronization of newly added events.
        /// </summary>
        public static bool DisableAutoSynchronization { get; set; }

        private static void UnitOfWork_OnEntitiesAddedOrModified(object sender, EntitiesStateEventArgs args)
        {
            if (DisableAutoSynchronization)
                return;
            var calendars = args.Entities
                .OfType<EventDO>()
                .Where(e => e.SyncStatus == EventSyncState.SyncWithExternal)
                .GroupBy(e => e.CalendarID)
                .ToArray();
            CalendarSyncManager calendarSyncManager = null;
            foreach (var curCalendarGroup in calendars)
            {
                if (calendarSyncManager == null)
                    calendarSyncManager = ObjectFactory.GetInstance<CalendarSyncManager>();
                // should be run async-ly to prevent user interface blocking.
                calendarSyncManager.SyncByLocalCalendarAsync(
                    curCalendarGroup.Key.Value,
                    curCalendarGroup.Min(e => e.StartDate),
                    curCalendarGroup.Max(e => e.EndDate));
            }
        }

        private readonly ICalDAVClientFactory _clientFactory;
        private readonly EventComparer _eventComparer = new EventComparer();

        public CalendarSyncManager(ICalDAVClientFactory clientFactory)
        {
            if (clientFactory == null)
                throw new ArgumentNullException("clientFactory");
            _clientFactory = clientFactory;
        }

        private void GetPeriod(out DateTimeOffset from, out DateTimeOffset to)
        {
            from = DateTimeOffset.UtcNow;
            to = from + TimeSpan.FromDays(365);
        }

        /// <summary>
        /// Creates IUnitOfWork and performs synchronization in its scope for user with ID=userId
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        public async Task SyncNowAsync(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetByKey(userId);
                if (curUser == null)
                    throw new EntityNotFoundException<UserDO>();
                try
                {
                    await SyncNowAsync(uow, curUser);
                    uow.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error(string.Format("Error occurred on user's (id:{0}) calendars synchronization.", userId), ex);
                    throw;
                }
            }
        }

        public async Task SyncNowAsync(IUnitOfWork uow, IUserDO user)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (user == null)
                throw new ArgumentNullException("user");

            DateTimeOffset from, to;
            GetPeriod(out from, out to);

            foreach (var authData in user.RemoteCalendarAuthData.Where(ad => ad.HasAccessToken()))
            {
                try
                {
                    await SyncByProviderAsync(uow, authData, @from, to);
                    Logger.GetLogger().InfoFormat("User's (id:{0}) calendars synchronized with '{1}' successfully.", user.Id, authData.Provider.Name);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error(string.Format("Error occurred on user's (id:{0}) calendars synchronization with '{1}'.", user.Id, authData.Provider.Name), ex);
                    AlertManager.ErrorSyncingCalendar(authData);
                }
            }
        }

        private async Task SyncByLocalCalendarAsync(int localCalendarId, DateTimeOffset from, DateTimeOffset to)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var calendarLinks = uow.RemoteCalendarLinkRepository
                    .GetQuery()
                    .Where(rcl => rcl.LocalCalendarID == localCalendarId && !rcl.IsDisabled)
                    .ToList();
                foreach (var remoteCalendarLink in calendarLinks)
                {
                    var authDatas = uow.RemoteCalendarAuthDataRepository
                        .GetQuery()
                        .Where(ad => ad.ProviderID == remoteCalendarLink.ProviderID)
                        .ToList();
                    foreach (var authData in authDatas)
                    {
                        var client = _clientFactory.Create(authData);
                        await SyncCalendarAsync(uow, from, to, client, remoteCalendarLink);
                    }
                }
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Performs synchronization for particular provider and user
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="authData">Describes user's authorization to provider</param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private async Task SyncByProviderAsync(IUnitOfWork uow, IRemoteCalendarAuthDataDO authData, DateTimeOffset @from,
                                            DateTimeOffset to)
        {
            var client = _clientFactory.Create(authData);
            var remoteCalendars = await client.GetCalendarsAsync(authData);
            if (remoteCalendars.Count == 0)
                return;
            // if localDefaultCalendar is null then each new calendar link will get a new local calendar.
            var localDefaultCalendar = uow.CalendarRepository.GetQuery().FirstOrDefault(c => c.OwnerID == authData.UserID);
            // take only first remote calendar for now
            foreach (var remoteCalendar in remoteCalendars.Take(1))
            {
                var calendarLink = uow.RemoteCalendarLinkRepository.GetOrCreate(authData, remoteCalendar.Key, localDefaultCalendar);
                calendarLink.RemoteCalendarName = remoteCalendar.Value;

                if (!calendarLink.IsDisabled)
                {
                    try
                    {
                        calendarLink.DateSynchronizationAttempted = DateTimeOffset.UtcNow;
                        await SyncCalendarAsync(uow, @from, to, client, calendarLink);

                        calendarLink.LastSynchronizationResult = "Success";
                        calendarLink.DateSynchronized = calendarLink.DateSynchronizationAttempted;
                    }
                    catch (Exception ex)
                    {
                        calendarLink.LastSynchronizationResult = string.Concat("Error: ", ex.Message);
                        Logger.GetLogger().Warn(
                            string.Format("Error occurred on calendar '{0}' synchronization with '{1} @ {2}'.",
                                          calendarLink.LocalCalendar.Name,
                                          calendarLink.RemoteCalendarHref,
                                          calendarLink.Provider.Name),
                            ex);
                        AlertManager.ErrorSyncingCalendar(authData, calendarLink);
                    }
                }
            }
        }

        private async Task SyncCalendarAsync(IUnitOfWork uow, DateTimeOffset @from, DateTimeOffset to, ICalDAVClient client, IRemoteCalendarLinkDO calendarLink)
        {
            // just a filter by date/time, added to avoid duplicate code.
            Func<EventDO, bool> eventPredictor = e => e.StartDate <= to && e.EndDate >= @from;
            var remoteEvents = await client.GetEventsAsync(calendarLink, @from, to);
            // filter out reccurring events
            var incomingEvents = remoteEvents.Select(cal => Event.CreateEventFromICSCalendar(uow, cal)).Where(eventPredictor).ToArray();
            var calendar = calendarLink.LocalCalendar;
            Debug.Assert(calendar != null, "No local calendar associated with this calendar link.");
            var owner = calendar.Owner;
            Debug.Assert(owner != null, "Local calendar associated with this calendar link has no owner.");
            // add filter by SyncStatus for local events.
            Func<EventDO, bool> existingEventPredictor = e => eventPredictor(e) 
                && e.SyncStatus == EventSyncState.SyncWithExternal
                && e.EventStatus != EventState.Deleted;
            var existingEvents = calendar.Events.Where(existingEventPredictor).ToList();

            foreach (var incomingEvent in incomingEvents)
            {
                if (
                    !incomingEvent.Attendees.Any(a => string.Equals(a.EmailAddress.Address, owner.EmailAddress.Address)))
                {
                    incomingEvent.Attendees.Add(new AttendeeDO()
                                                    {
                                                        EmailAddress = owner.EmailAddress,
                                                        EmailAddressID = owner.EmailAddressID,
                                                        Event = incomingEvent,
                                                        Name = owner.UserName
                                                    });
                }

                var existingEvent = existingEvents.FirstOrDefault(e => _eventComparer.Equals(incomingEvent, e));
                if (existingEvent != null)
                {
                    var provedAttendees = new List<AttendeeDO>(existingEvent.Attendees.Count);
                    foreach (var incomingAttendee in incomingEvent.Attendees)
                    {
                        var attendee =
                            existingEvent.Attendees.FirstOrDefault(
                                a => a.EmailAddress.Address == incomingAttendee.EmailAddress.Address);
                        if (attendee == null)
                        {
                            var existingEmailAddress =
                                uow.EmailAddressRepository.GetOrCreateEmailAddress(incomingAttendee.EmailAddress.Address);
                            attendee = incomingAttendee;
                            attendee.EmailAddress = existingEmailAddress;
                            attendee.EmailAddressID = existingEmailAddress.Id;
                            attendee.Event = existingEvent;
                            attendee.EventID = existingEvent.Id;
                            existingEvent.Attendees.Add(attendee);
                        }
                        provedAttendees.Add(attendee);
                    }
                    existingEvent.Attendees.RemoveAll(a => !provedAttendees.Contains(a));

                    existingEvent.Category = incomingEvent.Category;
                    existingEvent.Class = incomingEvent.Class;
                    existingEvent.Description = incomingEvent.Description;
                    existingEvent.Location = incomingEvent.Location;
                    existingEvent.Sequence = incomingEvent.Sequence;

                    existingEvents.Remove(existingEvent);
                }
                else
                {
                    // created by remote
                    incomingEvent.EventStatus = EventState.DispatchCompleted;
                    incomingEvent.CreateType = EventCreateType.RemoteCalendar;
                    incomingEvent.SyncStatus = EventSyncState.SyncWithExternal;
                    incomingEvent.Calendar = (CalendarDO) calendar;
                    incomingEvent.CalendarID = calendar.Id;
                    incomingEvent.CreatedBy = owner;
                    incomingEvent.CreatedByID = owner.Id;
                    calendar.Events.Add(incomingEvent);
                }
            }

            var createdByKwasant = existingEvents.Where(e => e.CreateDate >= calendarLink.DateSynchronized).ToList();
            foreach (var created in createdByKwasant)
            {
                await PushEventAsync(client, calendarLink, created);
            }

            var deletedByRemote = existingEvents.Where(e => e.CreateDate < calendarLink.DateSynchronized).ToList();
            foreach (var deleted in deletedByRemote)
            {
                deleted.EventStatus = EventState.Deleted;
                deleted.SyncStatus = EventSyncState.DoNotSync;
            }
        }

        private async Task PushEventAsync(ICalDAVClient client, IRemoteCalendarLinkDO calendarLink, EventDO eventDO)
        {
            var iCalendarEvent = Event.GenerateICSCalendarStructure(eventDO);
            await client.CreateEventAsync(calendarLink, iCalendarEvent);
        }
    }
}
