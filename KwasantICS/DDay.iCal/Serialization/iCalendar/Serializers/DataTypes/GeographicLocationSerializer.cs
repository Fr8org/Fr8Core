using System;
using System.IO;
using KwasantICS.DDay.iCal.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.DataTypes;

namespace KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.DataTypes
{
    public class GeographicLocationSerializer :
        EncodableDataTypeSerializer
    {
        public override Type TargetType
        {
            get { return typeof(GeographicLocation); }
        }

        public override string SerializeToString(object obj)
        {
            IGeographicLocation g = obj as IGeographicLocation;
            if (g != null)
            {
                string value = g.Latitude.ToString("0.000000") + ";" + g.Longitude.ToString("0.000000");
                return Encode(g, value);
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            IGeographicLocation g = CreateAndAssociate() as IGeographicLocation;
            if (g != null)
            {
                // Decode the value, if necessary!
                value = Decode(g, value);

                string[] values = value.Split(';');
                if (values.Length != 2)
                    return false;

                double lat;
                double lon;
                double.TryParse(values[0], out lat);
                double.TryParse(values[1], out lon);
                g.Latitude = lat;
                g.Longitude = lon;

                return g;
            }

            return null;
        }
    }
}
