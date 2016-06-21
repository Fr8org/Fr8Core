using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class KeyValueListCM : Manifest
    {
        public List<KeyValueDTO> Values { get; set; } = new List<KeyValueDTO>();

        public KeyValueListCM()
            : base(MT.KeyValueList)
        {
        }

        public KeyValueListCM(params KeyValueDTO[] values)
            : this ((IEnumerable<KeyValueDTO>) values)
        {
        }

        public KeyValueListCM(IEnumerable<KeyValueDTO> keyValue)
            : this()
        {
            Values.AddRange(keyValue);
        }
    }
}
