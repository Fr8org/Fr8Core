using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class DocuSignTemplateSubscriptionDO : BaseDO, IEquatable<DocuSignTemplateSubscriptionDO>
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int? ProcessTemplateId { get; set; }

        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        public string DocuSignTemplateId { get; set; }

        public bool Equals(DocuSignTemplateSubscriptionDO other)
        {
            return ProcessTemplateId == other.ProcessTemplateId && DocuSignTemplateId == other.DocuSignTemplateId;
        }
    }
}