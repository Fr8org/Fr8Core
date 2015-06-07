using System.Collections.Generic;

namespace KwasantICS.DDay.iCal.Interfaces.DataTypes
{
    public interface IPeriodList :
        IEncodableDataType,
        IList<IPeriod>
    {
        string TZID { get; set; }

        IPeriod this[int index] { get; set; }
        void Add(IDateTime dt);
        void Remove(IDateTime dt);
    }
}
