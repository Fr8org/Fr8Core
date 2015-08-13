using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States;
using Data.States.Templates;

namespace Data.Entities
{
    public class ProcessNodeTemplateDO
    {
        public ProcessNodeTemplateDO()
        {
            this.ActionLists = new List<ActionListDO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int? ParentTemplateId { get; set; }

        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        /// <summary>
        /// this is a JSON structure that is a array of key-value pairs that represent possible transitions. Example:
        ///[{'Flag':'true','Id':'234kljdf'},{'Flag':'false','Id':'dfgkjfg'}]. In this case the values are Id's of other ProcessNodes.
        /// </summary>
        public string TransitionKey { get; set; }

        public virtual CriteriaDO Criteria { get; set; }

        public virtual List<ActionListDO> ActionLists { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}