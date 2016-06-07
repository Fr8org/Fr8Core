using fr8.Infrastructure.Data.DataTransferObjects;

namespace fr8.Infrastructure.Data.Crates.Helpers
{
    public class LoggingDataCrateFactory
    {
        public Crate Create(LoggingDataCM loggingDataCm)
        {
            return Crate.FromContent("Dockyard Terminal Fact or Incident Report", loggingDataCm);
        }
    }
}
