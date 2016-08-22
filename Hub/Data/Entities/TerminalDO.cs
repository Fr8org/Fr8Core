using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class TerminalDO : BaseObject, ITerminalDO
    {
        public TerminalDO()
        {
            this.AuthenticationType = Fr8.Infrastructure.Data.States.AuthenticationType.None;      
        }

        [Key]
        public Guid Id { get; set; }

        public string Secret { get; set; }
        
        public string Name { get; set; }

        public string Version { get; set; }

        public string Label { get; set; }

        [ForeignKey("TerminalStatusTemplate")]
        public int TerminalStatus { get; set; }
        public _TerminalStatusTemplate TerminalStatusTemplate { get; set; }

        /// <summary>
        /// Currently active endpoint. 
        /// </summary>
        /// <remarks>
        /// The 'Endpoint' property contains the currently active endpoint which may be changed 
        /// by deployment scripts or by promoting the terminal from Dev to Production 
        /// while ProdUrl/DevUrl contains  whatever user or administrator have supplied.
        /// </remarks>
        [Required]
        public string Endpoint { get; set; }
        
        public virtual Fr8AccountDO UserDO { get; set; }

        [ForeignKey("UserDO")]
        public string UserId { get; set; }
        public string Description { get; set; }

        [ForeignKey("AuthenticationTypeTemplate")]
        public int AuthenticationType { get; set; }

        public virtual _AuthenticationTypeTemplate AuthenticationTypeTemplate { get; set; }

        public bool IsFr8OwnTerminal { get; set; }

        /// <summary>
        /// Development endpoint URL. 
        /// </summary>
        public string DevUrl { get; set; }

        /// <summary>
        /// Production endpoint URL. 
        /// </summary>
        public string ProdUrl { get; set; }

        [ForeignKey("ParticipationStateTemplate")]
        public int ParticipationState { get; set; }
        public virtual _ParticipationStateTemplate ParticipationStateTemplate { get; set; }

        [Required]
        [ForeignKey("OperationalStateTemplate")]
        public int OperationalState { get; set; }
        public virtual _OperationalStateTemplate OperationalStateTemplate { get; set; }

    }
}
