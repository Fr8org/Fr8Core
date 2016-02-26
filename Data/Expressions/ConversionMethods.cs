using System;
using System.Reflection;

namespace Data.Expressions
{
    // This class contains MethodInfos for type casting in ExpressionTransformation. 
    // We want them to be static to avoid unecessary reflection calls
    // It is a bad practice to place static memeber into generics classes, so we crate a dedicated class for this
    static class ConversionMethods
    {
        public static readonly MethodInfo GenericChangeTypeMethodInfo;

        static ConversionMethods()
        {
            GenericChangeTypeMethodInfo = typeof(ConversionMethods).GetMethod("ChangeType", new[] { typeof(object), typeof(Type), typeof(IFormatProvider) });
        }

        public static object ChangeType(object value, Type targetType, IFormatProvider formatProvider)
        {
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                {
                    return null;
                }

                var rawValue = Convert.ChangeType(value, targetType.GetGenericArguments()[0], formatProvider);
                return Activator.CreateInstance(targetType, new[] { rawValue });
            }

            return Convert.ChangeType(value, targetType, formatProvider);
        }
    }
}