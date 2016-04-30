﻿using System;
using System.Collections.Generic;
using Data.Entities;

namespace Data.Entities
{
    public class ProfileDO : BaseObject
    {
        public ProfileDO()
        {
            PermissionSets = new List<PermissionSetDO>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<PermissionSetDO> PermissionSets { get; set; } 
    }
}
