using System;

namespace terminalFr8Core.Infrastructure
{
    public static partial class MTSearchHelper
    {
        public static IMtQueryProvider CreateQueryProvider(Type manifestType)
        {
            var builderType = typeof (MtQueryProvider<>).MakeGenericType(manifestType);
            var builder = (IMtQueryProvider) Activator.CreateInstance(builderType);

            return builder;
        }
    }
}