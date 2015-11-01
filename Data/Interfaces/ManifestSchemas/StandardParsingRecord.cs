using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardParsingRecord : Manifest
    {
        public string Name { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Service { get; set; }

        public string ExternalAccountId { get; set; }

        public string InternalAccountId { get; set; }

        public StandardParsingRecord()
            : base(Constants.MT.StandardParsingRecord)
        {

        }
    }
}
