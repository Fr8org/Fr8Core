using System;
using Fr8.Infrastructure.Data.Crates;

namespace Hub.Services.UpstreamValueExtractors
{
    public interface IUpstreamValueExtractor
    {
        Type ConfigurationControlType { get; }

        void ExtractUpstreamValue(object configurationControl, ICrateStorage crateStorage);
    }
}
