using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8Data.Manifests
{
    public class StandardFr8ContainersCM : Manifest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public StandardFr8ContainersCM()
			  : base(Constants.MT.StandardFr8Containers)
		  { 
		  }

    }
}
