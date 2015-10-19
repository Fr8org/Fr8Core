using System.Collections.Generic;

namespace Data.Interfaces.DataTransferObjects
{
    /// <summary>
    /// Route DTO that contains full graph of other DTO objects
    /// Specifically used in Workflow Designer to draw entire process.
    /// </summary>
    public class RouteDTO : RouteOnlyDTO
    {
        /// <summary>
        /// List of Subroute DTOs.
        /// </summary>
        public IEnumerable<FullSubrouteDTO> Subroutes { get; set; }
    }
    
    /// <summary>
    /// Subroute DTO that contains full graph of other DTO objects.
    /// </summary>
    public class FullSubrouteDTO : SubrouteDTO
    {
        /// <summary>
        /// List of ActionList DTOs.
        /// </summary>
        public List<ActionDTO> Actions { get; set; }
    }
}