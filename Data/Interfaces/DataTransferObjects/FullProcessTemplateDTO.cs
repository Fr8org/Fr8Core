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
        /// List of ProcessNodeTemplate DTOs.
        /// </summary>
        public IEnumerable<FullProcessNodeTemplateDTO> ProcessNodeTemplates { get; set; }
    }
    
    /// <summary>
    /// ProcessNodeTemplate DTO that contains full graph of other DTO objects.
    /// </summary>
    public class FullProcessNodeTemplateDTO : ProcessNodeTemplateDTO
    {
        /// <summary>
        /// List of ActionList DTOs.
        /// </summary>
        public List<ActionDTO> Actions { get; set; }
    }
}