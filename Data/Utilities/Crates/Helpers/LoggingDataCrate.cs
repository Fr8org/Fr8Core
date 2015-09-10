using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Core.Utilities.Crates.Helpers
{
    public static class LoggingDataCrate
    {
        public static CrateDTO Create(LoggingData loggingData)
        {

            var crateDTO = new CrateDTO()
            {
                Id = Guid.NewGuid().ToString(),
                Label = "Dockyard Plugin Event or Incident Report",
                Contents = null,
                ManifestType = "Dockyard Plugin Event or Incident Report",
                ManifestId = 3
            };

            return crateDTO;
        }
    }
}
