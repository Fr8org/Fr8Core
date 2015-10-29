using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.Manifests
{
    public class StandardParsingRecordCM : Manifest
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Service { get; set; }

        public string ExternalAccountId { get; set; }

        public string InternalAccountid { get; set; }

        public StandardParsingRecordCM()
            : base(Constants.MT.StandardParsingRecord)
        {
        }

    }
}
