using System;
using System.IO;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.General;
using KwasantICS.DDay.iCal.Interfaces.Serialization;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.DataTypes;
using KwasantICS.DDay.iCal.Utility;

namespace KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.Other
{
    public class UriSerializer :
        EncodableDataTypeSerializer
    {
        #region Constructors

        public UriSerializer()
        {
        }

        public UriSerializer(ISerializationContext ctx)
            : base(ctx)
        {
        }

        #endregion        

        #region Overrides

        public override Type TargetType
        {
            get { return typeof(string); }
        }

        public override string SerializeToString(object obj)
        {
            if (obj is Uri)
            {
                Uri uri = (Uri)obj;

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    return Encode(dt, uri.OriginalString);
                }
                return uri.OriginalString;
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            if (tr != null)
            {
                string value = tr.ReadToEnd();

                ICalendarObject co = SerializationContext.Peek() as ICalendarObject;
                if (co != null)
                {
                    EncodableDataType dt = new EncodableDataType();
                    dt.AssociatedObject = co;
                    value = Decode(dt, value);
                }

                value = TextUtil.Normalize(value, SerializationContext).ReadToEnd();

                try
                {
                    Uri uri = new Uri(value);
                    return uri;
                }
                catch
                {
                }
            }
            return null;
        }

        #endregion
    }
}
