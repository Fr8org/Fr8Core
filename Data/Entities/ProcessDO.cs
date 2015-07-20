using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure;
using Data.Interfaces;
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
        


    }
}
