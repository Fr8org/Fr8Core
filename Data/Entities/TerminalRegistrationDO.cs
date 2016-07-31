using Data.States.Templates;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class TerminalRegistrationDO : BaseObject
    {
        /// <summary>
        /// Currently active endpoint. 
        /// </summary>
        /// <remarks>
        /// The 'Endpoint' property contains the currently active endpoint which may be changed 
        /// by deployment scripts or by promoting the terminal from Dev to Production 
        /// while ProdUrl/DevUrl contains  whatever user or administrator have supplied.
        /// </remarks>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Endpoint { get; set; }

        [ForeignKey("User")]
        public String UserId { get; set; }
        public Fr8AccountDO User { get; set; }
        public bool IsFr8OwnTerminal { get; set; }

        /// <summary>
        /// Development endpoint URL. 
        /// </summary>
        public string DevUrl { get; set; }
        public string ProdUrl { get; set; }

        [Required]
        [ForeignKey("OperationalStateTemplate")]
        public int OperationalState { get; set; }
        public virtual _OperationalStateTemplate OperationalStateTemplate { get; set; }

        [Required]
        [ForeignKey("ParticipationStateTemplate")]
        public int ParticipationState { get; set; }
        public virtual _ParticipationStateTemplate ParticipationStateTemplate { get; set; }
    }
}
