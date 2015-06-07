using System;
using System.Collections.Generic;
using KwasantICS.Collections;

namespace KwasantICS.DDay.iCal.Interfaces.General
{
    public interface IFilteredCalendarObjectList<T> :
        ICollection<T>
        where T : ICalendarObject
    {
        event EventHandler<ObjectEventArgs<T>> ItemAdded;
        event EventHandler<ObjectEventArgs<T>> ItemRemoved;
        T this[int index] { get; }
        int IndexOf(T item);
    }
}
