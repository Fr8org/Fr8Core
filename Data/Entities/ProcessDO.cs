using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;
using Utilities;

namespace Data.Entities
{
    public class ProcessDO : BaseDO
    {
        public ProcessDO()
        {

            //Notes = "No additional notes";
        }
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set;}
        public string UserId { get; set; }
        [Required, ForeignKey("ProcessStateTemplate")]
        public int ProcessState {get;set;}    
        public virtual _ProcessStateTemplate ProcessStateTemplate {get;set;}    

    }
}
