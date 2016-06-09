using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.Manifests
{
    public class StandardFr8HubsCM : Manifest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public StandardFr8HubsCM()
            : base(Constants.MT.StandardFr8Hubs)
        {
        }
    }
}
