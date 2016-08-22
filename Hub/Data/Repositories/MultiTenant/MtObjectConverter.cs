using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Data.Repositories.MultiTenant
{
    class MtObjectConverter : IMtObjectConverter
    {
        private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>
        {
            typeof(string), typeof(bool), typeof(bool?), typeof(byte), typeof(byte?),
            typeof(char), typeof(char?), typeof(short), typeof(short?), typeof(int),
            typeof(int?), typeof(long), typeof(long?), typeof(float), typeof(float?),
            typeof(double), typeof(double?), typeof(DateTime), typeof(DateTime?)
        };

        public MtObject ConvertToMt(object instance, MtTypeDefinition mtTypeDefinition)
        {
            var mtObject = new MtObject(mtTypeDefinition);
            var properties = instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var mtProp = mtTypeDefinition.Properties.FirstOrDefault(x => x.Name == propertyInfo.Name);
                if (mtProp == null)
                {
                    continue;
                }
                
                var value = propertyInfo.GetValue(instance);

                mtObject.Values[mtProp.Index] = ConvertToDbCanonicalFormat(value);
            }

            return mtObject;
        }

        public bool IsPrimitiveType(Type type)
        {
            return PrimitiveTypes.Contains(type);
        }

        private bool IsOfPrimitiveType(object value)
        {
            if (value == null)
            {
                return false;
            }

            return IsPrimitiveType(value.GetType());
        }

        public object ConvertToObject(MtObject mtObject)
        {
            var instance = Activator.CreateInstance(mtObject.MtTypeDefinition.ClrType);

            var properties = mtObject.MtTypeDefinition.ClrType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var propertyInfo in properties)
            {
                if (!propertyInfo.CanWrite || propertyInfo.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var mtProp = mtObject.MtTypeDefinition.Properties.FirstOrDefault(x => x.Name == propertyInfo.Name);
                if (mtProp == null)
                {
                    continue;
                }

                var value = mtObject.Values[mtProp.Index];

                // if property is of value type but we have null - skip
                if (!(propertyInfo.PropertyType.IsValueType) && value == null)
                {
                    continue;
                }

                propertyInfo.SetValue(instance, ConvertFromDbCanonicalFormat(value, mtProp.MtPropertyType));
            }

            return instance;
        }

        public string ConvertToDbCanonicalFormat(object value)
        {
            string convertedValue;

            if (value == null)
            {
                return null;
            }

            if (!IsOfPrimitiveType(value))
            {
                return JsonConvert.SerializeObject(value);
            }

            var type = value.GetType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return ConvertToDbCanonicalFormat(type.GetProperty("Value").GetValue(value));
            }

            if (value is DateTime)
            {
                var val = (DateTime) value;
                
                //ISO8601 (127)
                convertedValue = val.ToString("o");
            }
            else
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }

            return convertedValue;
        }

        public object ConvertFromDbCanonicalFormat(string value, MtTypeDefinition type)
        {
            if (value == null)
            {
                return null;
            }

            var isNullable = type.ClrType.IsGenericType && type.ClrType.GetGenericTypeDefinition() == typeof (Nullable<>);
            object rawValue;

            if (type.IsComplexType)
            {
                return JsonConvert.DeserializeObject(value, type.ClrType);
            }

            if (type.ClrType == typeof(DateTime) || type.ClrType == typeof(DateTime?))
            {
                rawValue = DateTime.Parse(value, null);
            }
            else
            {
                rawValue = Convert.ChangeType(value, type.ClrType, CultureInfo.InvariantCulture);
            }

            if (isNullable)
            {
                return Activator.CreateInstance(type.ClrType, rawValue);
            }

            return rawValue;
        }
    }
}