using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class ProcessDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string DockyardAccountId { get; set; }
        public string EnvelopeId { get; set; }

        [ForeignKey("CurrentProcessNode")]
        public int? CurrentProcessNodeId { get; set; }
        public virtual ProcessNodeDO CurrentProcessNode { get; set; }

        public virtual ICollection<ProcessNodeDO> ProcessNodes { get; set; }
            
        [Required]
        [ForeignKey("ProcessStateTemplate")]
        public int ProcessState { get; set; }

        public virtual _ProcessStateTemplate ProcessStateTemplate { get; set; }
    }
}