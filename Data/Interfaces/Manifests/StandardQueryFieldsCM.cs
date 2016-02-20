using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    [CrateManifestSerializer(typeof(StandardQueryFieldsSerializer))]
    public class StandardQueryFieldsCM : Manifest
    {
        public StandardQueryFieldsCM()
			  : base(Constants.MT.StandardQueryFields)
        {
            Fields = new List<QueryFieldDTO>();
        }

        public StandardQueryFieldsCM(IEnumerable<QueryFieldDTO> fields) : this()
        {
            Fields.AddRange(fields);
        }

        public List<QueryFieldDTO> Fields { get; set; }
    }


    public class StandardQueryFieldsSerializer : IManifestSerializer
    {
        public void Initialize(ICrateStorageSerializer manager)
        {
        }

        public object Deserialize(JToken crateContent)
        {
            var converter = new ControlDefinitionDTOConverter();

            var serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>
                {
                    converter
                }
            });

            return crateContent.ToObject<StandardQueryFieldsCM>(serializer);
        }

        public JToken Serialize(object content)
        {
            return JToken.FromObject(content);
        }
    }

}
