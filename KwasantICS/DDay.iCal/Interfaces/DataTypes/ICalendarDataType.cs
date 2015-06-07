using System;
using KwasantICS.DDay.iCal.Interfaces.General;
using IServiceProvider = KwasantICS.DDay.iCal.Interfaces.General.IServiceProvider;

namespace KwasantICS.DDay.iCal.Interfaces.DataTypes
{
    public interface ICalendarDataType :
        ICalendarParameterCollectionContainer,
        ICopyable,
        IServiceProvider
    {
        Type GetValueType();
        void SetValueType(string type);
        ICalendarObject AssociatedObject { get; set; }
        IICalendar Calendar { get; }

        string Language { get; set; }
    }
}
