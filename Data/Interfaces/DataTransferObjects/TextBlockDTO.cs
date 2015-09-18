using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Interfaces.DataTransferObjects
{
    public class TextBlockFieldDTO : FieldDefinitionDTO
    {
        [JsonProperty("class")]
        public string cssClass;
    }
}
