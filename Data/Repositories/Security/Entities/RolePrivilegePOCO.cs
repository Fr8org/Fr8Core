using System;

namespace Data.Repositories.Security.Entities
{
    public class RolePrivilege
    {
        public RolePrivilege()
        {
            Privilege = new PrivilegeDO();
            Role = new RoleDO();
        }
        public Guid Id { get; set; }
        public PrivilegeDO Privilege { get; set; }
        public RoleDO Role { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
