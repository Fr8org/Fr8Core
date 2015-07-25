using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.States;
using Data.States.Templates;

namespace Data.Entities
{
    public class ProcessTemplateDO : BaseDO 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        [Required, ForeignKey("ProcessTemplateStateTemplate")]
        public int ProcessState { get; set; }
        public _ProcessTemplateStateTemplate ProcessTemplateStateTemplate { get; set; }
    }
}
