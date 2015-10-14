using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{
    /// <summary>
    /// Data transfer object for SubrouteDO entity.
    /// </summary>
    public class SubrouteDTO
    {
        public int Id { get; set; }

        public int? RouteId { get; set; }

        public string Name { get; set; }

        public string TransitionKey { get; set; }
    }
}