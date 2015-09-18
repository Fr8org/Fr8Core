using Core.Interfaces;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;
using JsonSerializer = Utilities.Serializers.Json.JsonSerializer;

namespace Core.Services
{
    public class Crate : ICrate
    {
        private readonly JsonSerializer _serializer;
        public Crate()
        {
            _serializer = new JsonSerializer();
        }

        public CrateDTO Create(string label, string contents, string manifestType = "", int manifestId = 0)
        {
            var crateDTO = new CrateDTO() 
            { 
                Id = Guid.NewGuid().ToString(), 
                Label = label, 
                Contents = contents, 
                ManifestType = manifestType, 
                ManifestId = manifestId 
            };
            return crateDTO;
        }

        public T GetContents<T>(CrateDTO crate)
        {
            return _serializer.Deserialize<T>(crate.Contents);
        }

        public IEnumerable<JObject> GetElementByKey(IEnumerable<CrateDTO> searchCrates, string key)
        {

            List<JObject> resultsObjects = new List<JObject>();
            foreach (var curCrate in searchCrates)
            {
                JObject curCrateJSON = JsonConvert.DeserializeObject<JObject>( curCrate.Contents);
                JContainer contents = (JContainer)curCrateJSON["contents"];
                var results = contents.Descendants()
                    .OfType<JObject>()
                    .Where(x => x[key] != null);
                resultsObjects.AddRange(results); ;
            }
            return resultsObjects;


        }
    }
}
