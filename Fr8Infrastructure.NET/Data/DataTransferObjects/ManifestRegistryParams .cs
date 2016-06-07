using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fr8.Infrastructure.Data.DataTransferObjects
{
    /// <summary>
    /// Used in ManifestRegistryController
    /// </summary>
    public class ManifestRegistryParams
    {
        public string name { get; set; }
        public string version { get; set; }
    }
}
