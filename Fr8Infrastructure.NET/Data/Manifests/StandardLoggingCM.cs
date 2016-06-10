using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardLoggingCM : Manifest
    {
        public List<LogItemDTO> Item { get; set; }

        [MtPrimaryKey]
        public string LoggingMTkey { get; set; }

        public StandardLoggingCM()
            : base(MT.StandardLoggingCrate)
        {
            Item = new List<LogItemDTO>();
        }
    }
}
