using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class QueryDTO
    {
        public QueryDTO()
        {
            Criteria = new List<FilterConditionDTO>();
        }

        public QueryDTO(string name, IEnumerable<FilterConditionDTO> conditions) : this()
        {
            Name = name;
            Criteria.AddRange(conditions);
        }

        public QueryDTO(string name, params FilterConditionDTO[] conditions) : this()
        {
            Name = name;
            Criteria.AddRange(conditions);
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("criteria")]
        public List<FilterConditionDTO> Criteria { get; set; }
    }
}
