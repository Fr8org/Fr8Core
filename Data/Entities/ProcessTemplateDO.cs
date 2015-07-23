using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.States;

namespace Data.Entities
{
    public class ProcessTemplateDO : BaseDO 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public ProcessTemplateState ProcessState { get; set; }
    }
}
