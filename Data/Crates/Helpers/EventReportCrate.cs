using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using Utilities.Serializers.Json;

namespace Data.Crates.Helpers
{
    public static class EventReportCrate
    {
        public static CrateDTO Create(params CrateDTO[] crates)
        {
            return Create(crates.ToList());
        }

        public static CrateDTO Create(List<CrateDTO> crates)
        {
            var serializer = new JsonSerializer();
            var contents = serializer.Serialize(crates);
            return new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Dockyard Plugin Event or Incident Report",
                Contents = contents,
                ManifestType = "Dockyard Plugin Event or Incident Report",
                ManifestId = 3
            };
        }
    }
}
