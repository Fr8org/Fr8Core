using System.Collections.Generic;

namespace Data.Repositories.Security.Entities
{
    /// <summary>
    /// Data Object Wrapper for Secured Object and his Role Privileges. Not connected to EF context. Used in Security Logic.
    /// Contains also possible role privileges for properties of SecuredObject
    /// </summary>
    public class ObjectRolePrivilegesDO
    {
        public ObjectRolePrivilegesDO()
        {
            RolePrivileges = new List<RolePrivilege>();
            Properties = new Dictionary<string, List<RolePrivilege>>();
        }

        /// <summary>
        /// Identifier of Secured Object. In general Primary Key(GUID) of Data Object. 
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Type of the object
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// All role privileges of current secured object.
        /// </summary>
        public List<RolePrivilege> RolePrivileges { get; set; } 

        /// <summary>
        /// Contains list of Properties of current object, and their RolePrivileges, based on PropertyName as Key object inside keyValue Pair
        /// </summary>
        public Dictionary<string, List<RolePrivilege>> Properties { get; set; } 
    }
}
