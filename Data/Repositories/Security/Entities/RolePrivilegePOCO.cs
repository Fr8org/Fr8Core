using System;

namespace Data.Repositories.Security.Entities
{
    public class RolePrivilege
    {
        public RolePrivilege()
        {
            Role = new RoleDO();
        }
        public Guid Id { get; set; }
        public string PrivilegeName { get; set; }
        public RoleDO Role { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
