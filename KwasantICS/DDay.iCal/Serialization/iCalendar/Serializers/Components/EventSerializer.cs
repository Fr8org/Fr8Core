using System;
using KwasantICS.DDay.iCal.Interfaces.Serialization;

namespace KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.Components
{
    public class EventSerializer :
        UniqueComponentSerializer
    {
        #region Constructor

        public EventSerializer() : base()
        {
        }

        public EventSerializer(ISerializationContext ctx)
            : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(DDayEvent); }
        }

        public override string SerializeToString(object obj)
        {
            if (obj is DDayEvent)
            {
                DDayEvent evt = ((DDayEvent)obj).Copy<DDayEvent>();

                // NOTE: DURATION and DTEND cannot co-exist on an event.
                // Some systems do not support DURATION, so we serialize
                // all events using DTEND instead.
                if (evt.Properties.ContainsKey("DURATION") && evt.Properties.ContainsKey("DTEND"))
                    evt.Properties.Remove("DURATION");

                return base.SerializeToString(evt);
            }
            return base.SerializeToString(obj);
        }

        #endregion
    }
}
