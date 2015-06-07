using System;

namespace KwasantICS.DDay.iCal.Interfaces.DataTypes
{
    public interface IWeekDay :
        IEncodableDataType,
        IComparable
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}
