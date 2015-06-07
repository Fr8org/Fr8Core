using KwasantICS.Collections.Interfaces;

namespace KwasantICS.DDay.iCal.Interfaces.General
{
    public interface ICalendarParameter :
        ICalendarObject,
        IValueObject<string>
    {
        string Value { get; set; }
    }
}
