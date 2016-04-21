using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Security.Entities
{
    /// <summary>
    /// Data Object Wrapper for Permission. Not connected to EF Context and contains only basic data.
    /// Used for Security Objects Manipulation.
    /// </summary>
    public class PermissionDO
    {
        public string Name { get; set; }
    }
}
