using System;

namespace Data.Repositories.Security.Entities
{
    public class RolePrivilege
    {
        public Guid Id { get; set; }
        public string PrivilegeName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }
    }
}
