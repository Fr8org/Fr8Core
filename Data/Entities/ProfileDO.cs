using System;
using System.Collections.Generic;
using Data.Entities;

namespace Data.Entities
{
    public class ProfileDO : BaseObject
    {
        public ProfileDO()
        {
            Permissions = new List<PermissionDO>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<PermissionDO> Permissions { get; set; } 
    }
}
