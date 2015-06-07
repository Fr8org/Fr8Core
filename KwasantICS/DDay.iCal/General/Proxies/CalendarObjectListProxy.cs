using System.Linq;
using KwasantICS.Collections.Interfaces;
using KwasantICS.Collections.Proxies;
using KwasantICS.DDay.iCal.Interfaces.General;

namespace KwasantICS.DDay.iCal.General.Proxies
{
    public class CalendarObjectListProxy<TType> :
        GroupedCollectionProxy<string, ICalendarObject, TType>,
        ICalendarObjectList<TType>
        where TType : class, ICalendarObject
    {
        public CalendarObjectListProxy(IGroupedCollection<string, ICalendarObject> list) : base(list)
        {
        }

        virtual public TType this[int index]
        {
            get
            {
                return this.Skip(index).FirstOrDefault();
            }
        }
    }
}
