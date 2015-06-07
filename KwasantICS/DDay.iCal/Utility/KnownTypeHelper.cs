using System;
using System.Collections.Generic;
using KwasantICS.DDay.iCal.General;

namespace KwasantICS.DDay.iCal.Utility
{
    static public class KnownTypeHelper
    {
        static public IList<Type> GetKnownTypes()
        {
            List<Type> types = new List<Type>();

            types.Add(typeof(CalendarPropertyList));
            types.Add(typeof(CalendarParameterList));

            return types;
        }
    }
}
