using Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.Manifests
{
    public class StandardSecurityCM : Manifest
    {
        public string AuthenticateAs { get; set; }

        public StandardSecurityCM()
            : base(MT.StandardSecurityCrate)
        { 
        }
    }
}
