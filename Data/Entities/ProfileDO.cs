using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ProfileDO : BaseDO
    {
        public ProfileDO()
        {
            ProfileNodes = new List<ProfileNodeDO>();
        }

        [Key]
        public int Id { get; set; }

        public String Name { get; set; }

        [ForeignKey("User")]
        public String UserID { get; set; }
        public virtual UserDO User { get; set; }

        [InverseProperty("Profile")]
        public virtual IList<ProfileNodeDO> ProfileNodes { get; set; }
    }
}
