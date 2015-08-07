using System.Runtime.Serialization;

namespace pluginAzureSqlServer.Services.Json.Models
{
    [DataContract]
    public class ActionTypeList
    {
        [DataMember(Name = "type_name")]
        public string TypeName { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }
    }
}