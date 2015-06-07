using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;

namespace Data.Entities
{

    //Kact stands for a Kwasant Action. It's a loggable event that we'll record for our Business Intelligence activities
    //We'll switch to a better name when we think of one, but Event, Action, Activity are all taken in one way or another...
    public class FactDO : HistoryItemDO
    {
        public FactDO()
        {
            var t = 0;//For debug breakpoint purposes
        }

        [ForeignKey("CreatedBy")]
        public string CreatedByID { get; set; }
        public virtual UserDO CreatedBy { get; set; }
    }
}
