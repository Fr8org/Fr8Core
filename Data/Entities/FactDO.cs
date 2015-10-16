using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{

    
    public class FactDO : HistoryItemDO
    {
        public FactDO()
        {
            var t = 0;//For debug breakpoint purposes
        }

        [ForeignKey("CreatedBy")]
        public string CreatedByID { get; set; }
        public virtual Fr8AccountDO CreatedBy { get; set; }
    }
}
