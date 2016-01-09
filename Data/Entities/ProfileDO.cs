using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ProfileDO : BaseObject
    {
        public ProfileDO()
        {
            ProfileNodes = new List<ProfileNodeDO>();
        }

        [Key]
        public int Id { get; set; }

        public String Name { get; set; }

        [ForeignKey("DockyardAccount")]
        public String DockyardAccountID { get; set; }
        public virtual Fr8AccountDO DockyardAccount { get; set; }

        [InverseProperty("Profile")]
        public virtual IList<ProfileNodeDO> ProfileNodes { get; set; }
    }
}