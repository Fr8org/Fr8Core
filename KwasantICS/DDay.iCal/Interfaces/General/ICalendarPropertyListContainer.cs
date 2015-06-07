namespace KwasantICS.DDay.iCal.Interfaces.General
{
    public interface ICalendarPropertyListContainer :
        ICalendarObject
    {
        ICalendarPropertyList Properties { get; }
    }
}
