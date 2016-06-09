
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class SalesforceEventCM : Manifest
    {
        public string ObjectType { get; set; }
        [MtPrimaryKey]
        public string ObjectId { get; set; }
        public string CreatedDate { get; set; }
        public string LastModifiedDate { get; set; }
        public string OccuredEvent { get; set; }

        public SalesforceEventCM()
              : base(MT.SalesforceEvent)
        {

        }
    }
}
