using Fr8Data.DataTransferObjects;

namespace Fr8Data.Crates.Helpers
{
    public class LoggingDataCrateFactory
    {
        public Crate Create(LoggingDataCM loggingDataCm)
        {
            return Crate.FromContent("Dockyard Terminal Fact or Incident Report", loggingDataCm);
        }
    }
}
