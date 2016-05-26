using System;

namespace terminalDocuSign.Services.MT
{
    // Helper for querying MT DB
    public static partial class MTSearchHelper
    {
        // Crate query provider for given manifest type
        // Query provider can be used for converting criterias built with Query Builder
        public static terminalDocuSign.Services.MT.MTSearchHelper.IMtQueryProvider CreateQueryProvider(Type manifestType)
        {
            var builderType = typeof (terminalDocuSign.Services.MT.MTSearchHelper.MtQueryProvider<>).MakeGenericType(manifestType);
            var builder = (terminalDocuSign.Services.MT.MTSearchHelper.IMtQueryProvider) Activator.CreateInstance(builderType);

            return builder;
        }
    }
}