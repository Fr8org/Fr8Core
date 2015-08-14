using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class DocuSignTemplateSubscriptionDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int? ProcessTemplateId { get; set; }

        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        public string DocuSignTemplateId { get; set; }
    }
}