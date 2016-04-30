using System;
using Data.Entities;

namespace Data.Repositories.Security.Entities
{
    public class RolePermission
    {
        public RolePermission()
        {
            PermissionSet = new PermissionSetDO();
            Role = new RoleDO();
        }
        public Guid Id { get; set; }
        public PermissionSetDO PermissionSet { get; set; }
        public RoleDO Role { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
