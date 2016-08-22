using System.Collections.Generic;
using Fr8Data.DataTransferObjects;

namespace Fr8Data.Manifests
{
    public class ExternalObjectHandlesCM : Manifest
    {
        public ExternalObjectHandlesCM()
            : base(Constants.MT.ExternalObjectHandles)
        {
        }

        public ExternalObjectHandlesCM(IEnumerable<ExternalObjectHandleDTO> handles) : this()
        {
            Handles = new List<ExternalObjectHandleDTO>(handles);
        }

        public ExternalObjectHandlesCM(params ExternalObjectHandleDTO[] handles) : this()
        {
            Handles = new List<ExternalObjectHandleDTO>(handles);
        }


        public List<ExternalObjectHandleDTO> Handles { get; set; }
    }
}
