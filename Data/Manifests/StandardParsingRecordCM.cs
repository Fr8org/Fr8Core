using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.Manifests
{
    public class StandardParsingRecordCM : Manifest
    {
        public string Name { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Service { get; set; }

        public string ExternalAccountId { get; set; }

        public string InternalAccountId { get; set; }

        public StandardParsingRecordCM()
            : base(Constants.MT.StandardParsingRecord)
        {
        }

    }
}
