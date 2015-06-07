using KwasantICS.Collections.Interfaces.Proxies;

namespace KwasantICS.DDay.iCal.Interfaces.General.Proxies
{
    public interface ICalendarParameterCollectionProxy :
        ICalendarParameterCollection,
        IGroupedCollectionProxy<string, ICalendarParameter, ICalendarParameter>
    {                
    }
}
