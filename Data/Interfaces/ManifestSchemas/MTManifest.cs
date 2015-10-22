using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Constants;

namespace Data.Interfaces.ManifestSchemas
{
    public class MTManifest : Manifest
    {
        public MTManifest(MT curMTManifestType) : base(curMTManifestType)
        {
            
        }

        public string Fr8AccountId { get; set; }
    }
}
