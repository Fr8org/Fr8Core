using KwasantICS.Collections.Interfaces;

namespace KwasantICS.DDay.iCal.Interfaces.General
{
    public interface ICalendarProperty :        
        ICalendarParameterCollectionContainer,
        ICalendarObject,
        IValueObject<object>
    {
        object Value { get; set; }
    }
}
