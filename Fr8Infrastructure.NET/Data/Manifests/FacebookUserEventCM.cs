using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class FacebookUserEventCM : Manifest
    {
        public string Time { get; set; }

        [MtPrimaryKey]
        public string Id { get; set; }
        public string[] ChangedFields { get; set; }
        public string Uid { get; set; }

        public FacebookUserEventCM(): base(MT.FacebookUserEvent)
        {

        }
    }
}
