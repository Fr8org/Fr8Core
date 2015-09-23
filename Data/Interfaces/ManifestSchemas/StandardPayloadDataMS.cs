using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.ManifestSchemas
{
    public class StandardPayloadDataMS : ManifestSchema
    {
        public StandardPayloadDataMS()
			  :base(Constants.ManifestTypeEnum.StandardPayloadData)
        {
            Payload = new List<PayloadObjectDTO>();
        }

        public List<PayloadObjectDTO> Payload { get; set; }
    }


    public class PayloadObjectDTO
    {

        public PayloadObjectDTO()
        {
            Fields = new List<FieldDTO>();
        }

        public List<FieldDTO> Fields;
    }


}
