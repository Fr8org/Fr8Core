
namespace Fr8Data.Manifests
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
              : base(Constants.MT.SalesforceEvent)
        {

        }
    }
}
