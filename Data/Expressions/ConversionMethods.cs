using System;
using System.Reflection;

namespace Data.Expressions
{
    static class ConversionMethods
    {
        public static readonly MethodInfo GenericChangeTypeMethodInfo;

        static ConversionMethods()
        {
            GenericChangeTypeMethodInfo = typeof(Convert).GetMethod("ChangeType", new[] { typeof(object), typeof(Type), typeof(IFormatProvider) });
        }
    }
}