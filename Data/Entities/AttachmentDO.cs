using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{
    public class AttachmentDO : StoredFileDO, IAttachmentDO
    {
        public String Type { get; set; }
        public String ContentID { get; set; }
        public bool BoundaryEmbedded { get; set; }

        [ForeignKey("Email"), Required]
        public int? EmailID { get; set; }
        public EmailDO Email { get; set; }
    }
}
