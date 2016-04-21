using System;

namespace Data.Repositories.Security.Entities
{
    public class RolePermission
    {
        public RolePermission()
        {
            Permission = new PermissionDO();
            Role = new RoleDO();
        }
        public Guid Id { get; set; }
        public PermissionDO Permission { get; set; }
        public RoleDO Role { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
