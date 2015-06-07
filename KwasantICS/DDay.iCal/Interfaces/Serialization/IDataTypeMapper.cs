using System;
using KwasantICS.DDay.iCal.Serialization;

namespace KwasantICS.DDay.iCal.Interfaces.Serialization
{
    public interface IDataTypeMapper
    {
        void AddPropertyMapping(string name, Type objectType, bool allowsMultipleValuesPerProperty);
        void AddPropertyMapping(string name, TypeResolverDelegate resolver, bool allowsMultipleValuesPerProperty);
        void RemovePropertyMapping(string name);

        bool GetPropertyAllowsMultipleValues(object obj);
        Type GetPropertyMapping(object obj);
    }
}
