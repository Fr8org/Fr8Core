using System;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    [Table("AspNetRoles")]
    public class AspNetRolesDO : IdentityRole, IAspNetRolesDO, ISaveHook
    {
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset CreateDate { get; set; }

        public void BeforeCreate()
        {
            if (CreateDate == default(DateTimeOffset))
                CreateDate = DateTimeOffset.UtcNow;
        }

        public void BeforeSave()
        {
            LastUpdated = DateTimeOffset.UtcNow;
        }
    }
}
