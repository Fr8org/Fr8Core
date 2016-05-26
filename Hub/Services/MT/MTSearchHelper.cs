using System;

namespace Hub.Services
{
    // Helper for querying MT DB
    public static partial class MTSearchHelper
    {
        // Crate query provider for given manifest type
        // Query provider can be used for converting criterias built with Query Builder
        public static IMtQueryProvider CreateQueryProvider(Type manifestType)
        {
            var builderType = typeof (MtQueryProvider<>).MakeGenericType(manifestType);
            var builder = (IMtQueryProvider) Activator.CreateInstance(builderType);

            return builder;
        }
    }
}