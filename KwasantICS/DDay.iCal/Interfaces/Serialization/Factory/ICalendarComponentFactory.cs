using KwasantICS.DDay.iCal.Interfaces.Components;

namespace KwasantICS.DDay.iCal.Interfaces.Serialization.Factory
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Build(string objectName, bool uninitialized);
    }
}
