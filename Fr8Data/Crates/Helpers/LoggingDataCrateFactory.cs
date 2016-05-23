using Fr8Data.DataTransferObjects;

namespace Fr8Data.Crates.Helpers
{
    public class LoggingDataCrateFactory
    {
        public Crate Create(LoggingDataCM loggingDataCm)
        {
            return Crate.FromContent("Dockyard Terminal Event or Incident Report", loggingDataCm);

            // var serializer = new JsonSerializer();
            // var contents = serializer.Serialize(loggingData);

//            var contents = JsonConvert.SerializeObject(loggingData);
//
//            return new CrateDTO()
//            {
//                Id = Guid.NewGuid().ToString(),
//                Label = "Dockyard Plugin Event or Incident Report",
//                Contents = contents,
//                ManifestType = "Dockyard Plugin Event or Incident Report",
//                ManifestId = 3
//            };
        }
    }
}
