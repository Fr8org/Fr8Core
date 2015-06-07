using System;
using KwasantICS.DDay.iCal.Interfaces;
using KwasantICS.DDay.iCal.Interfaces.Components;
using KwasantICS.DDay.iCal.Interfaces.DataTypes;
using KwasantICS.DDay.iCal.Interfaces.General;
using KwasantICS.DDay.iCal.Interfaces.Serialization;
using KwasantICS.DDay.iCal.Interfaces.Serialization.Factory;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.Components;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers.Other;

namespace KwasantICS.DDay.iCal.Serialization.iCalendar.Factory
{
    public class SerializerFactory :
        ISerializerFactory
    {
        #region Private Fields

        ISerializerFactory m_DataTypeSerializerFactory;

        #endregion

        #region Constructors

        public SerializerFactory()
        {
            m_DataTypeSerializerFactory = new DataTypeSerializerFactory();
        }

        #endregion

        #region ISerializerFactory Members

        /// <summary>
        /// Returns a serializer that can be used to serialize and object
        /// of type <paramref name="objectType"/>.
        /// <note>
        ///     TODO: Add support for caching.
        /// </note>
        /// </summary>
        /// <param name="objectType">The type of object to be serialized.</param>
        /// <param name="ctx">The serialization context.</param>
        virtual public ISerializer Build(Type objectType, ISerializationContext ctx)
        {
            if (objectType != null)
            {
                ISerializer s = null;

                if (typeof(IICalendar).IsAssignableFrom(objectType))
                    s = new iCalendarSerializer();
                else if (typeof(ICalendarComponent).IsAssignableFrom(objectType))
                {
                    if (typeof(IEvent).IsAssignableFrom(objectType))
                        s = new EventSerializer();
                    else if (typeof(IUniqueComponent).IsAssignableFrom(objectType))
                        s = new UniqueComponentSerializer();
                    else
                        s = new ComponentSerializer();
                }
                else if (typeof(ICalendarProperty).IsAssignableFrom(objectType))
                    s = new PropertySerializer();
                else if (typeof(ICalendarParameter).IsAssignableFrom(objectType))
                    s = new ParameterSerializer();
                else if (typeof(string).IsAssignableFrom(objectType))
                    s = new StringSerializer();
                else if (objectType.IsEnum)
                    s = new EnumSerializer(objectType);
                else if (typeof(TimeSpan).IsAssignableFrom(objectType))
                    s = new TimeSpanSerializer();
                else if (typeof(int).IsAssignableFrom(objectType))
                    s = new IntegerSerializer();
                else if (typeof(Uri).IsAssignableFrom(objectType))
                    s = new UriSerializer();
                else if (typeof(ICalendarDataType).IsAssignableFrom(objectType))
                    s = m_DataTypeSerializerFactory.Build(objectType, ctx);
                // Default to a string serializer, which simply calls
                // ToString() on the value to serialize it.
                else
                    s = new StringSerializer();
                
                // Set the serialization context
                if (s != null)
                    s.SerializationContext = ctx;

                return s;
            }
            return null;
        }

        #endregion
    }
}
