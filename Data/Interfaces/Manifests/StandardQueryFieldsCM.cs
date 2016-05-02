using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Crates;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    // TODO: FR-3003, remove this.
    // [CrateManifestSerializer(typeof(StandardQueryFieldsSerializer))]
    // public class TypedFieldsCM : Manifest
    // {
    //     public TypedFieldsCM()
	// 		  : base(Constants.MT.TypedFields)
    //     {
    //         Fields = new List<TypedFieldDTO>();
    //     }
    // 
    //     public TypedFieldsCM(IEnumerable<TypedFieldDTO> fields) : this()
    //     {
    //         Fields.AddRange(fields);
    //     }
    // 
    //     public List<TypedFieldDTO> Fields { get; set; }
    // }
    // 
    // 
    // public class StandardQueryFieldsSerializer : IManifestSerializer
    // {
    //     public void Initialize(ICrateStorageSerializer manager)
    //     {
    //     }
    // 
    //     public object Deserialize(JToken crateContent)
    //     {
    //         var converter = new ControlDefinitionDTOConverter();
    // 
    //         var serializer = JsonSerializer.Create(new JsonSerializerSettings()
    //         {
    //             Converters = new List<JsonConverter>
    //             {
    //                 converter
    //             }
    //         });
    // 
    //         return crateContent.ToObject<TypedFieldsCM>(serializer);
    //     }
    // 
    //     public JToken Serialize(object content)
    //     {
    //         return JToken.FromObject(content);
    //     }
    // }
}
