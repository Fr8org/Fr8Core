using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Crates.Helpers
{
    public class LoggingDataCrateFactory
    {
        public Crate Create(LoggingDataCM loggingDataCm)
        {
            return Crate.FromContent("Dockyard Terminal Fact or Incident Report", loggingDataCm);
        }
    }
}
