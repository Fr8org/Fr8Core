using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class TerminalDTO 
    {
        public TerminalDTO()
        {
            AuthenticationType = States.AuthenticationType.None;
            Roles = new List<string>();
        }
        
        [JsonProperty("InternalId")]
        public Guid InternalId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("label")]
        public string Label { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("terminalStatus")]
        public int TerminalStatus { get; set; }
        [JsonProperty("participationState")]
        public int ParticipationState { get; set; }
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("authenticationType")]
        public int AuthenticationType { get; set; }
        [JsonProperty("devUrl")]
        public string DevUrl { get; set; }
        [JsonProperty("prodUrl")]
        public string ProdUrl { get; set; }
        [JsonProperty("isFr8OwnTerminal")]
        public bool IsFr8OwnTerminal { get; set; }
        /// <summary>
        /// Allowed roles for users, determing Terminal Permissions
        /// </summary>
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }
}