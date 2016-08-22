using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Entities
{
    [Table("AspNetUserClaims")]
    public class AspNetUserClaimDO : IdentityUserClaim, IAspNetUserClaimDO, ISaveHook
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
