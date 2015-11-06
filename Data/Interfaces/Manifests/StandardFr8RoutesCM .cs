using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardFr8RoutesCM : Manifest
    {
        public DateTime CreateDate{ get; set; }

        public DateTime LastUpdated { get; set; }

        public String Description{ get; set; }

        public String Name{ get; set; }

        public int Ordering{ get; set; }

        public String RouteState{ get; set; }

        public List<SubrouteDTO> SubRoutes { get; set; }

        public StandardFr8RoutesCM()
            : base(Constants.MT.StandardFr8Routes)
        {
        }
    }
}
