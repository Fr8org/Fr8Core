using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Security.Entities
{
    /// <summary>
    /// Simple Data Object Wrapper for Object Role Permissions. Not connected to EF Context and contains only basic data.
    /// Used for Security Object Manipulation.
    /// </summary>
    public class RoleDO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
