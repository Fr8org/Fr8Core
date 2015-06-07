using System;

namespace KwasantICS.DDay.iCal.Factory
{
    public class UIDFactory
    {
        virtual public string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
