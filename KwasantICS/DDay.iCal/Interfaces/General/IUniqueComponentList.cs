using KwasantICS.DDay.iCal.Interfaces.Components;

namespace KwasantICS.DDay.iCal.Interfaces.General
{
    public interface IUniqueComponentList<TComponentType> :
        ICalendarObjectList<TComponentType>
        where TComponentType : class, IUniqueComponent
    {
        TComponentType this[string uid] { get; set; }
    }
}
