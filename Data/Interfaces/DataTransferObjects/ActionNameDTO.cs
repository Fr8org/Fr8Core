using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class ActionNameDTO
    {
        [JsonProperty("actionType")]
        public string ActionType { get; set; }

        [JsonProperty("version")]
        public string Version  { get; set; }
    }
}
