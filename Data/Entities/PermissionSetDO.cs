using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.States.Templates;

namespace Data.Entities
{
    public class PermissionSetDO : BaseObject
    {
        public PermissionSetDO()
        {
            Permissions = new List<_PermissionTypeTemplate>();
        }

        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
       
        /// <summary>
        /// Determine for what Object from Fr8 System, this permission set relates
        /// </summary>
        public string ObjectType { get; set; }

        [ForeignKey("Profile")]
        public Guid? ProfileId { get; set; }
        public virtual ProfileDO Profile {get; set; }

        public bool HasFullAccess { get; set; }

        public virtual ICollection<_PermissionTypeTemplate> Permissions { get; set; }

        public PermissionSetDO Clone()
        {
            var clone = new PermissionSetDO();

            clone.Id = this.Id;
            clone.Name = this.Name;
            clone.ObjectType = this.ObjectType;
            clone.ProfileId = this.ProfileId;
            clone.HasFullAccess = this.HasFullAccess;
            foreach (var item in Permissions)
            {
                clone.Permissions.Add(new _PermissionTypeTemplate() { Id =  item.Id, Name = item.Name});
            }
            return clone;
        }
    }
}
