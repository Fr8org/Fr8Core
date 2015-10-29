using System.Collections.Generic;
using Data.Interfaces.DataTransferObjects;

namespace Data.Interfaces.Manifests
{
    public class StandardPayloadDataCM : Manifest
    {
        public StandardPayloadDataCM()
			  :base(Constants.MT.StandardPayloadData)
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
