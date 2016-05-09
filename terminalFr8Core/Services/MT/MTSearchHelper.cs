using System;

namespace terminalFr8Core.Services.MT
{
    // Helper for querying MT DB
    public static partial class MTSearchHelper
    {
        // Crate query provider for given manifest type
        // Query provider can be used for converting criterias built with Query Builder
        public static terminalFr8Core.Services.MTSearchHelper.IMtQueryProvider CreateQueryProvider(Type manifestType)
        {
            var builderType = typeof (TerminalBase.Services.MTSearchHelper.MtQueryProvider<>).MakeGenericType(manifestType);
            var builder = (terminalFr8Core.Services.MTSearchHelper.IMtQueryProvider) Activator.CreateInstance(builderType);

            return builder;
        }
    }
}