using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class CommunicationConfigurationDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CommunicationTypeTemplate")]
        public int? CommunicationType { get; set; }
        public _CommunicationTypeTemplate CommunicationTypeTemplate { get; set; }
        
        public String ToAddress { get; set; }
    }
}
