using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    /// <summary>
    /// This class holds minimum information to distinguish between activities
    /// </summary>
    public class ActivityTemplateSummaryDTO
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("terminalName")]
        public string TerminalName { get; set; }

        [JsonProperty("terminalVersion")]
        public string TerminalVersion { get; set; }
    }
}
