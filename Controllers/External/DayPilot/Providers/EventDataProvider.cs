using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Data.Interfaces;
using Data.States;
using DayPilot.Web.Mvc.Events.Calendar;
using StructureMap;

namespace KwasantWeb.Controllers.External.DayPilot.Providers
{
    public class EventDataProvider : IEventDataProvider
    {
        //This code randomly generates a colour for each key. This colour is persisted for the entire lifetime of the web application.
        //They key currently used is the calendar id
        //This makes each event a unique colour, and groups event windows in the same group together
        private static readonly Random Rand = new Random();
        private static readonly Dictionary<string, Color> PairedColours = new Dictionary<string, Color>();
        public Color GetRandomPastelColour(string key)
        {
            lock (PairedColours)
            {
                if (PairedColours.ContainsKey(key))
                    return PairedColours[key];

                // to create lighter colours:
                // take a random integer between 0 & 128 (rather than between 0 and 255)
                // and then add 127 to make the colour lighter
                byte[] colorBytes = new byte[3];
                colorBytes[0] = (byte)(Rand.Next(128) + 127);
                colorBytes[1] = (byte)(Rand.Next(128) + 127);
                colorBytes[2] = (byte)(Rand.Next(128) + 127);

                Color color = PairedColours[key] = Color.FromArgb(255, colorBytes[0], colorBytes[1], colorBytes[2]);

                return color;
            }
        }

        private readonly bool _includeLinkedCalendars;
        private readonly int[] _calendarIDs;
        public EventDataProvider(bool includeLinkedCalendars, params int[] calendarIDs)
        {
            _includeLinkedCalendars = includeLinkedCalendars;
            _calendarIDs = calendarIDs;
        }

        public List<DayPilotTimeslotInfo> LoadData()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var providedCalendars = uow.CalendarRepository.GetQuery().Where(c => _calendarIDs.Contains(c.Id));
                if (_includeLinkedCalendars)
                {
                    var linkedUserIDs = providedCalendars.Select(c => c.OwnerID).ToList();
                    //providedCalendars = providedCalendars.Union(uow.CalendarRepository.GetQuery().Where(c => linkedUserIDs.Contains(c.OwnerID)));
                }

                return providedCalendars.SelectMany(c => c.Events.Where(e => e.EventStatus != EventState.Deleted)).ToList().Select(e =>
                new DayPilotTimeslotInfo
                {
                    StartDate = e.StartDate.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffff"),
                    EndDate = e.EndDate.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffff"),
                    Text = e.EventStatus != Data.States.EventState.Draft ?
                            (e.Summary != null ? e.Summary.Replace("DRAFT:", "") : e.Summary) :
                            e.Summary,
                    Id = e.Id,
                    GroupingID = e.CalendarID.ToString(),
                    IsAllDay = e.IsAllDay,
                    CalendarID = e.CalendarID,
                }).ToList();
            }
        }

        public string GetTimeslotBubbleText(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var eventDO = uow.EventRepository.GetQuery().FirstOrDefault(ev => ev.Id == id);
                return String.Format("Belongs to calendar owned by '{0} {1}", eventDO.Calendar.Owner.FirstName, eventDO.Calendar.Owner.LastName);
            }
        }

        //This method is for the day/week view. It assigns a colour to each bubble
        public void BeforeCellRender(BeforeEventRenderArgs e)
        {
            var source = e.DataItem.Source as DayPilotTimeslotInfo;
            if (source == null)
            {
                return;
            }
            Color color = GetRandomPastelColour(source.GroupingID);

            const string cssColourString = "#{0}{1}{2}";
            e.BackgroundColor = String.Format(cssColourString, color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
        }

        //This method is for the month view. It assigns a colour to each bubble
        //Unfortunately, we need to copy & paste, as the types are not compatable here, even though they have the same properties/methods (at least for our use).
        public void BeforeCellRender(global::DayPilot.Web.Mvc.Events.Month.BeforeEventRenderArgs e)
        {
            var source = e.DataItem.Source as DayPilotTimeslotInfo;
            if (source == null)
            {
                return;
            }
            Color color = GetRandomPastelColour(source.GroupingID);

            const string cssColourString = "#{0}{1}{2}";
            e.BackgroundColor = String.Format(cssColourString, color.R.ToString("X2"), color.G.ToString("X2"), color.B.ToString("X2"));
        }
    }
}