using KwasantICS.Collections.Interfaces;
using KwasantICS.DDay.iCal.General;

namespace KwasantICS.DDay.iCal.Interfaces.General
{
    public interface ICalendarPropertyList :
        IGroupedValueList<string, ICalendarProperty, CalendarProperty, object>
    {
        ICalendarProperty this[string name] { get; }
    }
}
