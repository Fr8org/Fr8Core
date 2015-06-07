using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.States;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.DataTypes;

namespace KwasantTest.Fixtures
{
    partial class FixtureData
    {

        public DDayEvent TestEvent()
        {
            return new DDayEvent()
            {

                //DTStart = (iCalDateTime)DateTime.Parse("20040117"),
                DTStart = new iCalDateTime("20140517"),
                DTEnd = new iCalDateTime("20140610"),
                Location = "San Francisco",
                Description = "First Ever Event",
                Summary = "Here's a Summary",
                WorkflowState = "Undispatched",

                //   DateTimeSerializer serializer = new DateTimeSerializer();
                //CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);

            };
        }


        public EventDO TestEvent1()
        {
            var calendar = TestCalendar1();
            return new EventDO()
            {
                CreatedByID = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Calendar = calendar,
                Description = "Description of  Event",
                Priority = 1,
                Sequence = 1,
                IsAllDay = false
            };
        }

        public EventDO TestEvent2()
        {
            var curEvent = new EventDO()
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Calendar = TestCalendar1(),
                CreatedByID = "1",
                Priority = 1,
                Sequence = 1,
                IsAllDay = false,
                Category = "Birthday",
                Class = "Private",
                Description = "This is a test event description.",
                Location = "Silicon Valley",
                EventStatus = EventState.Booking,
                Summary = "This is a test event summary.",
                Transparency = "Opaque",
                Attendees = TestAttendeeList1().ToList(),
                CreatedBy = TestUser2()
            };
            foreach (var attendee in curEvent.Attendees)
            {
                attendee.Event = curEvent;
                attendee.EventID = curEvent.Id;
            }
            return curEvent;
        }

        public EventDO TestEvent3_TodayDates()
        {
            var curEvent = new EventDO()
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                Calendar = TestCalendar1(),
                Priority = 1,
                Sequence = 1,
                IsAllDay = false,
                Category = "Birthday",
                Class = "Private",
                Description = "This is a test event description.",
                Location = "Silicon Valley",
                EventStatus = EventState.Booking,
                Summary = "This is a test event summary.",
                Transparency = "Opaque",
                Attendees = TestAttendeeList1().ToList(),
                CreatedBy = TestUser2()
            };
            foreach (var attendee in curEvent.Attendees)
            {
                attendee.Event = curEvent;
                attendee.EventID = curEvent.Id;
            }
            return curEvent;
        }

        public EventDO TestEvent4()
        {
            var curEvent = new EventDO()
            {
                CreatedByID = "1",
                EventStatus = EventState.Booking,
                Calendar = TestCalendar1(),
                Description = "This is my test invitation",
                Summary = @"My test invitation",
                Location = @"Some place!",
                StartDate = DateTime.Today.AddMinutes(5),
                EndDate = DateTime.Today.AddMinutes(15),
                Attendees = TestAttendeeList1().ToList(),
                Emails = new List<EmailDO>(),
                CreatedBy = TestUser2()
            };
            foreach (var attendee in curEvent.Attendees)
            {
                attendee.Event = curEvent;
                attendee.EventID = curEvent.Id;
            }
            return curEvent;
        }

        public EventDO TestEvent5()
        {
            var calendar = TestCalendar1();
            return new EventDO()
            {
                CreatedByID = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Calendar = calendar,
                Description = "Description of  Event",
                Priority = 1,
                Sequence = 1,
                IsAllDay = false,
                Id=1,
                CreatedBy = TestUser2()
            };
        }



    }
}
