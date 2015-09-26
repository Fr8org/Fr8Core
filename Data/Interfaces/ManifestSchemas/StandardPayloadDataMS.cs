using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardPayloadDataMS : ManifestSchema
    {
        public StandardPayloadDataMS()
        {
            PayloadObjects = new List<PayloadObjectDTO>();
            ObjectType = "Unspecified";
        }

        public List<PayloadObjectDTO> PayloadObjects { get; set; }
        public string ObjectType { get; set; }
    }


    public class PayloadObjectDTO
    {

        public PayloadObjectDTO()
        {
            PayloadObject = new List<FieldDTO>();
        }

        public List<FieldDTO> PayloadObject;
    }


}
