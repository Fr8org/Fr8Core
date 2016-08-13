using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class ActivityCategoryDO : BaseObject
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string Type { get; set; }

        protected bool Equals(ActivityCategoryDO other)
        {
            return Id == other.Id;
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ActivityCategoryDO) obj);
        }
        
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
