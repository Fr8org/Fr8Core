using System;
using System.Collections.Generic;
using Data.Entities;

namespace Data.Repositories.Security.Entities
{
    /// <summary>
    /// Data Object Wrapper for Secured Object and his Role Permission. Not connected to EF context. Used in Security Logic.
    /// Contains also possible role permissions for properties of SecuredObject
    /// </summary>
    public class ObjectRolePermissionsWrapper
    {
        public ObjectRolePermissionsWrapper()
        {
            RolePermissions = new List<RolePermission>();
            Properties = new Dictionary<string, List<RolePermission>> ();
        }

        /// <summary>
        /// Identifier of Secured Object. In general Primary Key(GUID) of Data Object. 
        /// </summary>
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Type of the object
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Fr8 Account that created this object. Used in combination with OwnerOfObject Role
        /// </summary>
        public string Fr8AccountId { get; set; }

        public int? OrganizationId { get; set; }

        /// <summary>
        /// All role permissions of current secured object.
        /// </summary>
        public List<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Contains list of Properties of current object, and their RolePermissions, based on PropertyName as Key object inside keyValue Pair
        /// </summary>
        public Dictionary<string, List<RolePermission>> Properties { get; set; } 
    }
}
