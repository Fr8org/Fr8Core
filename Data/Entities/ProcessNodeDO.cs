using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class ProcessNodeDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int ParentProcessId { get; set; }

        public virtual ProcessDO ParentProcess { get; set; }

        [ForeignKey("ProcessNodeStatusTemplate")]
        public int? ProcessNodeState { get; set; }

        public virtual _ProcessNodeStatusTemplate ProcessNodeStatusTemplate { get; set; }

        public ProcessNodeTemplateDO ProcessNodeTemplate { get; set; }
    }
}