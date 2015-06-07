using System;
using System.Collections.Generic;
using KwasantICS.Collections;
using KwasantICS.DDay.iCal.Interfaces.DataTypes;

namespace KwasantICS.DDay.iCal.Interfaces.Components
{
    public interface IUniqueComponent :
        ICalendarComponent
    {
        event EventHandler<ObjectEventArgs<string, string>> UIDChanged;
        string UID { get; set; }

        IList<IAttendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DTStamp { get; set; }
        IOrganizer Organizer { get; set; }
        IList<IRequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}
