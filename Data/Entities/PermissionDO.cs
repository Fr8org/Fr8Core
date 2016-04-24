using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.Entities
{
    public class PermissionDO : BaseObject
    {
        public PermissionDO()
        {
            CreateObject = true;
            ReadObject = true;
            EditObject = true;
            DeleteObject = true;
        }

        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public bool ReadObject { get; set; }
        public bool CreateObject { get; set; }
        public bool EditObject { get; set; }
        public bool DeleteObject { get; set; }
        public bool ViewAllObjects { get; set; }
        public bool ModifyAllObjects { get; set; }
    }
}
