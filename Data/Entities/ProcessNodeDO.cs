using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class ProcessNodeDO : BaseObject
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("ParentContainer")]
        public Guid ParentContainerId { get; set; }

        public virtual ContainerDO ParentContainer { get; set; }

        [ForeignKey("ProcessNodeStatusTemplate")]
        public int? ProcessNodeState { get; set; }

        public virtual _ProcessNodeStatusTemplate ProcessNodeStatusTemplate { get; set; }

        public Guid SubrouteId { get; set; }
        public virtual SubrouteDO Subroute { get; set; }
    }
}