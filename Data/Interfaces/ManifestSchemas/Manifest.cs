using Data.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Data.Interfaces.ManifestSchemas
{
    public class Manifest
    {
        public MT ManifestType { get; private set; }
        public int ManifestId
        {
            get { return (int)ManifestType; }
        }
        public string ManifestName
        {
            get { return ManifestType.GetEnumDisplayName(); }
        }

        public Manifest(MT manifestType)
        {
            ManifestType = manifestType;
        }
    }
}
