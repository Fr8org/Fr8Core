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
        public string Id { get; set; }
        public string[] ChangedFields { get; set; }
        public string UserId { get; set; }

        public FacebookUserEventCM(): base(MT.FacebookUserEvent)
        {

        }
    }
}
