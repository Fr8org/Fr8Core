using System;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.Components;
using KwasantICS.DDay.iCal.Interfaces.Serialization;

namespace KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.Components
{
    public class UniqueComponentSerializer :
        ComponentSerializer
    {
        #region Constructor

        public UniqueComponentSerializer()
        {
        }

        public UniqueComponentSerializer(ISerializationContext ctx)
            : base(ctx)
        {
        }

        #endregion

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(UniqueComponent); }
        }

        public override string SerializeToString(object obj)
        {
            IUniqueComponent c = obj as IUniqueComponent;
            if (c != null)
            {                
                if (c.DTStamp != null &&
                    !c.DTStamp.IsUniversalTime)
                {
                    // Ensure the DTSTAMP property is in universal time before
                    // it is serialized
                    c.DTStamp = new iCalDateTime(c.DTStamp.UTC);
                }
            }
            return base.SerializeToString(obj);
        }
        
        #endregion
    }
}
