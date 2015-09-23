using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class CrateDTO
    {
        public CrateDTO()
        {
            this.CreateTime = DateTime.Now;
        }

        public string Id { get; set; }

        public string Label { get; set; }

        public string Contents { get; set; }

        public string ParentCrateId { get; set; }

        public string ManifestType { get; set; }

        public int ManifestId { get; set; }

        public ManufacturerDTO Manufacturer { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
